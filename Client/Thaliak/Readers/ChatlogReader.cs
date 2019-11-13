using Milvaneth.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Milvaneth.Common.Logging;
using Thaliak.Signatures;

namespace Thaliak.Readers
{
    internal partial class DataReader
    {
        internal int _privLen;
        internal int _readLenLength;

        private int[] _filters = {0, 1, 2, 57, 62, 64, 65, 66, 67, 71, 74}; // constant

        internal unsafe ChatlogResult GetChatLog(int linesToRead = 1000)
        {
            const int hardLimit = 1000;
            var throwOnNewFragment = false;

            if (linesToRead > hardLimit || linesToRead < 0)
            {
                linesToRead = hardLimit;
            }

            while (true)
            {
                MemoryChatlogEntry ptrStruct;
                var logStruct = _gs.GetPointer(PointerType.ChatLogEntry);
                var ptrArr = _gs.Reader.Read(logStruct, Signature.PointerLib[PointerType.ChatLogEntry].Length);
                fixed (byte* p = &ptrArr[0])
                {
                    ptrStruct = *(MemoryChatlogEntry*) p;
                }

                var lenStart = ptrStruct.LengthStart;
                var lenEnd = ptrStruct.LengthEnd;
                var logStart = ptrStruct.LogStart;
                var logEnd = ptrStruct.LogEnd;
                var ret = new List<ChatlogLine>();

                if (logEnd == IntPtr.Zero || lenEnd == IntPtr.Zero || logStart == IntPtr.Zero || lenStart == IntPtr.Zero)
                    return new ChatlogResult(null, -1);

                if(logStart.ToInt64() == logEnd.ToInt64())
                    return new ChatlogResult(null, 0);

                var remLen = lenEnd.ToInt64() - lenStart.ToInt64() - _readLenLength;

                if (remLen % 4 != 0) throw new InvalidOperationException("Broken length array");

                if (remLen < 0)
                {
                    if (throwOnNewFragment) throw new InvalidOperationException("Broken length pointer");

                    _privLen = 0;
                    _readLenLength = 0;
                    throwOnNewFragment = true;
                    continue;
                }

                var availableLines = remLen / 4;

                linesToRead = (int) Math.Min(availableLines, linesToRead);

                if(linesToRead <= 0) return new ChatlogResult(null, -1);

                var lenArr = _gs.Reader.Read(lenStart + _readLenLength, linesToRead * 4);
                var limitLen = BitConverter.ToInt32(lenArr, (linesToRead - 1) * 4);
                var lenOffset = 0;
                var readLength = limitLen - _privLen;

                if((logStart + limitLen).ToInt64() > logEnd.ToInt64()) throw new InvalidOperationException("Chatlog ptr of bound");
                var logArr = _gs.Reader.Read(logStart + _privLen, readLength); // never put it in loop

                for (var i = 0; i < linesToRead; i++)
                {
                    var curLen = BitConverter.ToInt32(lenArr, i * 4);
                    var datLen = curLen - _privLen;

                    if (lenOffset + datLen > logArr.Length) break;
                    var lineParse = LogChatMessage(logArr, _filters, lenOffset, datLen);
                    if (lineParse != null)
                    {
                        ret.Add(lineParse);
                        Logger.LogChat(lineParse.ToString());
                    }

                    lenOffset += datLen;
                    _privLen = curLen;
                    _readLenLength += 4;
                }

                return new ChatlogResult(ret, (int)availableLines - linesToRead);
            }
        }

        private ChatlogLine LogChatMessage(byte[] message, int[] filters, int off, int len)
        {
            if (len < 8 || filters == null)
            {
                return null;
            }

            var ts = BitConverter.ToInt32(message, off + 0);
            var msgType = BitConverter.ToInt32(message, off + 4);

            if (!filters.Contains(msgType & 0xFF))
            {
                return null;
            }

            var text = Encoding.UTF8.GetString(message, off + 8, len - 8);
            if (text[0] != ':' || text.LastIndexOf(':') < 1)
            {
                return null;
            }

            var sender = text.Substring(1, text.IndexOf(':', 1) - 1);
            text = text.Substring(text.IndexOf(':', 1) + 1);
            return new ChatlogLine(ts, msgType, sender, text);
        }
    }
}
