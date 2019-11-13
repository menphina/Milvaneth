using System;

namespace Thaliak.Signatures
{
    public class SigRecord
    {
        public SigRecord(string pattern, SignatureType selfType, int offset = 0, bool asmSignature = true)
        {
            AsmSignature = asmSignature;
            Pattern = pattern;
            Offset = offset;
            Signature = ConvertPattern(pattern, out var mask);
            Mask = mask;
            Length = Signature.Length;
            SelfType = selfType;
        }

        public string Pattern { get; }
        public byte[] Signature { get; }
        public bool[] Mask { get; }
        public int Length { get; }
        public int Offset { get; }
        public bool AsmSignature { get; }
        public SignatureType SelfType { get; }

        internal bool Finished = false;

        private static readonly byte[] FromHexTable = {
                255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
                255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
                255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
                255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
                255, 255, 255, 255, 255, 255, 255, 255, 0, 1,
                2, 3, 4, 5, 6, 7, 8, 9, 255, 255,
                255, 255, 255, 255, 255, 10, 11, 12, 13, 14,
                15, 255, 255, 255, 255, 255, 255, 255, 255, 255,
                255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
                255, 255, 255, 255, 255, 255, 255, 10, 11, 12,
                13, 14, 15
            };

        private static readonly byte[] FromHexTable16 = {
                255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
                255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
                255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
                255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
                255, 255, 255, 255, 255, 255, 255, 255, 0, 16,
                32, 48, 64, 80, 96, 112, 128, 144, 255, 255,
                255, 255, 255, 255, 255, 160, 176, 192, 208, 224,
                240, 255, 255, 255, 255, 255, 255, 255, 255, 255,
                255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
                255, 255, 255, 255, 255, 255, 255, 160, 176, 192,
                208, 224, 240
            };

        private static unsafe byte[] ConvertPattern(string source, out bool[] mask)
        {
            if (string.IsNullOrEmpty(source) || source.Length % 2 == 1)
                throw new ArgumentException("Bad signature length");

            var len = source.Length >> 1;
            var result = new byte[len];
            mask = new bool[len];

            fixed (char* sourceRef = source)
            fixed (byte* hiRef = FromHexTable16)
            fixed (byte* lowRef = FromHexTable)
            fixed (byte* resultRef = result)
            fixed (bool* maskRef = mask)
            {
                var s = &sourceRef[0];
                var r = &resultRef[0];
                var m = &maskRef[0];

                while (*s != 0)
                {
                    byte add;
                    *m++ = (*r = hiRef[*s++]) != 255 & (add = lowRef[*s++]) != 255;
                    *r++ += add;
                }
                return result;
            }
        }
    }
}
