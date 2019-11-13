using Milvaneth.Common;
using System;
using Thaliak.Signatures;

namespace Thaliak.Readers
{
    internal partial class DataReader
    {
        internal DateTime GetServerTime()
        {
            var ptr = _gs.GetPointer(PointerType.ServerTime);
            var len = Signature.PointerLib[PointerType.ServerTime].Length;
            var arr = _gs.Reader.Read(ptr, len);
            return Helper.UnixTimeStampToDateTime(ConvInt(arr, len));
        }

        private int GetMapInfo()
        {
            var ptr = _gs.GetPointer(PointerType.MapInfo);
            var len = Signature.PointerLib[PointerType.MapInfo].Length;
            var arr = _gs.Reader.Read(ptr, len);
            return (int)ConvInt(arr, len);
        }

        internal int GetSessionUpTime()
        {
            var ptr = _gs.GetPointer(PointerType.SessionUpTime);
            var len = Signature.PointerLib[PointerType.SessionUpTime].Length;
            var arr = _gs.Reader.Read(ptr, len);
            return (int)ConvInt(arr, len);
        }

        internal int GetCurrentGil()
        {
            var ptr = _gs.GetPointer(PointerType.CurrentGil);
            var len = Signature.PointerLib[PointerType.CurrentGil].Length;
            var arr = _gs.Reader.Read(ptr, len);
            return (int)ConvInt(arr, len);
        }

        private string GetLocalWorld()
        {
            var ptr = _gs.GetPointer(PointerType.LocalWorldName);
            var len = Signature.PointerLib[PointerType.LocalWorldName].Length;
            var arr = _gs.Reader.Read(ptr, len);
            var str = Helper.ToUtf8String(arr, 0, 0, len);
            return str;
        }

        internal string GetPlayerName()
        {
            var ptr = _gs.GetPointer(PointerType.CharacterMap);
            var len = 40;
            var arr = _gs.Reader.Read(ptr + 48, len);
            var str = Helper.ToUtf8String(arr, 0, 0, len);
            return str;
        }

        private static long ConvInt(byte[] arr, int len, int offset = 0)
        {
            if (len != 4 && len != 8) throw new InvalidOperationException("Invalid ConvInt length");
            return len == 8 ? BitConverter.ToInt64(arr, offset) : BitConverter.ToInt32(arr, offset);
        }
    }
}
