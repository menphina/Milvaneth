using Milvaneth.Common;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Thaliak.Network.Filter;
using Thaliak.Network.Sniffer;
using Thaliak.Network.Utilities;

namespace Thaliak.Network.Analyzer
{
    public sealed class PacketAnalyzer : ISnifferOutput, IDisposable
    {
        private const TCPFlags FlagAckPsh = TCPFlags.ACK | TCPFlags.PSH;
        private const TCPFlags FlagFinRst = TCPFlags.FIN | TCPFlags.RST;
        private const int HeaderLength = 40;

        private BlockingCollection<AnalyzedPacket> _outputQueue;
        private bool _isStopping;
        private long _packetsAnalyzed;
        private long _messagesProcessed;
        private int processedLength;

        private readonly MemoryStream _buffer;
        private readonly IAnalyzerOutput _output;
        private Blowfish _decrypter;

        public long PacketsAnalyzed => _packetsAnalyzed;
        public long MessagesProcessed => _messagesProcessed;

        public PacketAnalyzer(IAnalyzerOutput output)
        {
            this._outputQueue = new BlockingCollection<AnalyzedPacket>();

            this._output = output;

            this._isStopping = false;
            this._buffer = new MemoryStream(65536);
        }

        public void Start()
        {
            this._isStopping = false;

            Task.Run(() =>
            {
                Log.Warning("PacketAnalyzer Output Thread Started");

                foreach (var analyzedPacket in this._outputQueue.GetConsumingEnumerable())
                {
                    this._output.Output(analyzedPacket);
                    Interlocked.Increment(ref this._messagesProcessed);
                }

                Log.Warning("PacketAnalyzer Output Thread Exited");
            });
        }

        public void Stop()
        {
            this._isStopping = true;
        }

        public void Dispose()
        {
            _buffer?.Dispose();
        }

        public unsafe void Output(TimestampedData timestampedData)
        {
            if (timestampedData.Protocol != 6) return; // TCP

            var mark = timestampedData.RouteMark;
            var tcpPacket = new TCPPacket(timestampedData.Data, timestampedData.HeaderLength);

            if (!tcpPacket.IsValid || (tcpPacket.Flags & TCPFlags.ACK) != TCPFlags.ACK)
            {
                if ((tcpPacket.Flags & FlagFinRst) != 0)
                {
                    IpcClient.SendSignal(Signal.InternalConnectionFin, null, true);
                }

                return;
            }

            if (tcpPacket.Payload == null || tcpPacket.Payload.Length == 0) return;

            Interlocked.Increment(ref this._packetsAnalyzed);

            _buffer.Seek(0, SeekOrigin.End); // pos = end
            _buffer.Write(tcpPacket.Payload, 0, tcpPacket.Payload.Length); // pos = end

            var needPurge = false;
            var currentHeader = new byte[HeaderLength];

            for (;;)
            {
                if (!needPurge)
                {
                    _buffer.Seek(processedLength, SeekOrigin.Begin); // pos = 0

                    if (_buffer.Length - processedLength <= HeaderLength || processedLength > 65536) // not enough data
                    {
                        var remaining = _buffer.Length - processedLength;
                        if(remaining < 0) throw new AggregateException("Invalid processedLength");

                        var tmp = new byte[remaining];
                        _buffer.Read(tmp, 0, tmp.Length);
                        _buffer.SetLength(0);
                        _buffer.Write(tmp, 0, tmp.Length);
                        processedLength = 0;
                        return;
                    }

                    _buffer.Read(currentHeader, 0, HeaderLength); // pos = 40


                    if (!IsValidHeader(currentHeader))
                    {
                        needPurge = true;
                        continue;
                    }

                    NetworkPacketHeader header;
                    fixed (byte* p = &currentHeader[0])
                    {
                        header = *(NetworkPacketHeader*) p;
                    }

                    if (header.Malformed)
                    {
                        needPurge = true;
                        continue;
                    }

                    var timeDelta = Math.Abs(Helper.DateTimeToUnixTimeStamp(DateTime.Now) * 1000 - header.Timestamp);

                    if (header.Length < HeaderLength || (header.Timestamp != 0 && timeDelta > 60000)) // > 1 min
                    {
                        needPurge = true;
                        continue;
                    }

                    if (header.Length > _buffer.Length - processedLength)
                        return; // wait for more content

                    var content = GenerateContent(_buffer, header.IsCompressed, header.Length);

                    if (content.Length == 0)
                    {
                        content.Dispose();

                        needPurge = true;
                        continue;
                    }

                    ConsumeContent(content, header, mark);

                    processedLength += header.Length; // pos = 0
                }
                else
                {
                    needPurge = false;

                    var bytes = _buffer.ToArray();
                    var newStart = processedLength + 1;
                    var pos = FindMagic(new ArraySegment<byte>(bytes, newStart, bytes.Length - newStart));

                    if (pos == -1)
                    {
                        _buffer.SetLength(0); // no available data, drop all content
                        processedLength = 0;
                        return;
                    }

                    processedLength += pos + 1;
                }
            }
        }

        private MemoryStream GenerateContent(Stream stream, bool isCompressed, ushort length, int offset = -1)
        {
            if (offset >= 0)
            {
                stream.Seek(offset, SeekOrigin.Begin);
            }

            var content = new MemoryStream();
            if (isCompressed)
            {
                var body = new byte[length - HeaderLength - 2];
                stream.Seek(2, SeekOrigin.Current);
                stream.Read(body, 0, body.Length); // pos = length

                try
                {
                    using (var input = new MemoryStream(body))
                    using (var deflate = new DeflateStream(input, CompressionMode.Decompress))
                    {
                        deflate.CopyTo(content);
                    }
                }
                catch
                {
                    IpcClient.SendSignal(Signal.ClientPacketParseFail, new[] {"unable to deflate"}, true);
                }
            }
            else
            {
                var body = new byte[length - HeaderLength];
                stream.Read(body, 0, body.Length); // pos = bundle.Length
                content = new MemoryStream(body);
            }

            return content;
        }

        private void ConsumeContent(MemoryStream content, NetworkPacketHeader header, MessageAttribute mark)
        {
            var actualLen = 0;
            content.Seek(0, SeekOrigin.Begin);

            while (header.Count-- > 0)
            {
                var lenBytes = new byte[4];
                content.Read(lenBytes, 0, 4);
                var len = BitConverter.ToInt32(lenBytes, 0);

                actualLen += len;
                if (actualLen > content.Length || len < 4) break;

                // length field is zero here. we will not set it, just use msg.Length
                var msg = new byte[len];
                content.Read(msg, 4, len - 4);

                if (mark.GetCatalog() == MessageAttribute.CatalogLobby)
                {
                    var tmp = HandleLobbyPacket(content.ToArray());
                    if (tmp == null) return;
                    msg = tmp;
                }

                Buffer.BlockCopy(lenBytes, 0, msg, 0, 4);

                EnqueueOutput(new AnalyzedPacket(len, msg, header.Timestamp, mark));
            }

            content.Dispose();
        }

        private void EnqueueOutput(AnalyzedPacket analyzedPacket)
        {
            if (this._isStopping)
            {
                this._outputQueue.CompleteAdding();
                return;
            }

            this._outputQueue.Add(analyzedPacket);
        }

        private int FindMagic(IList<byte> target)
        {
            ulong headerRR, header00;

            unchecked
            {
                headerRR = (uint) Searcher.Search(target, new byte[] {0x52, 0x52, 0xa0, 0x41});
                header00 = (uint)Searcher.Search(target,
                    new byte[]
                    {
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
                    });
            }

            return (int)Math.Min(headerRR, header00);
        }

        private bool IsValidHeader(byte[] target)
        {
            var magic = BitConverter.ToUInt32(target, 0);
            if (magic != 0x41a05252u && magic != 0x00000000u) return false;

            var magicBytes = target.Take(16).ToArray();
            if (magic == 0x00000000u && magicBytes.Any(x => x != 0)) return false;

            return true;
        }

        private byte[] HandleLobbyPacket(byte[] lobbyPayload)
        {
            if (lobbyPayload.Length <= 24) return null; // ping, not interested

            if (lobbyPayload[12] == 0x09)
            {
                var key = CalculateEncryption(
                    lobbyPayload.Skip(52).Take(32).ToArray(),
                    lobbyPayload.Skip(116).Take(4).ToArray());
                _decrypter = new Blowfish(key);
                return lobbyPayload; // plain text
            }

            if (_decrypter == null)
            {
                // unable to decrypt
                // but since salt is hard-coded in game binary and time range is relatively small
                // we can brute force the password in a few seconds if we really need its data
                lobbyPayload[0x0E] = 0xDE; // DEcrypt
                lobbyPayload[0x0F] = 0xFA; // FAiled
                return lobbyPayload;
            }

            var dat = _decrypter.DecryptECB(lobbyPayload);
            Buffer.BlockCopy(lobbyPayload, 0, dat, 0, 16); // restore header
            return dat;
        }

        private static byte[] CalculateEncryption(byte[] salt, byte[] time)
        {
            var encKey = new byte[4 + 4 + 4 + 32];
            var version = BitConverter.GetBytes(MessageIdRetriver.Instance.GetVersion());

            encKey[0] = 0x78;
            encKey[1] = 0x56;
            encKey[2] = 0x34;
            encKey[3] = 0x12;
            Buffer.BlockCopy(time, 0, encKey, 4, 4);
            Buffer.BlockCopy(version, 0, encKey, 8, 4);
            Buffer.BlockCopy(salt, 0, encKey, 12, 32);

            using (var md5Hash = MD5.Create())
            {
                return md5Hash.ComputeHash(encKey);
            }
        }
    }
}
