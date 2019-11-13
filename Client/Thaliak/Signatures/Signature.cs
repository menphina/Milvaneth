using System;
using System.Collections.Generic;
using Milvaneth.Common;

namespace Thaliak.Signatures
{

    internal static class Signature
    {
        internal static Dictionary<SignatureType, SigRecord> SignatureLib { get; private set; }
        internal static Dictionary<PointerType, PtrRecord> PointerLib { get; private set; }
        internal static int DataVersion { get; private set; }

        private const string encKey = "";

        internal static void LoadSearchPack()
        {
            var path = Helper.GetMilFilePathRaw("search.pack");
            var ser = new Serializer<Tuple<int, Dictionary<SignatureType, SigRecord>, Dictionary<PointerType, PtrRecord>>>(path, encKey);
            var data = ser.Load();
            DataVersion = data.Item1;
            SignatureLib = data.Item2;
            PointerLib = data.Item3;
        }
    }
}
