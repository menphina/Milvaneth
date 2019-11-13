namespace Thaliak.Signatures
{
    public class PtrRecord
    {
        public PtrRecord(int[] offsets, int finalOffset, int length, int dtStep = 0, ThreadNr threadId = ThreadNr.NoThread, bool enableCache = false)
        {
            Offsets = offsets;
            FinalOffset = finalOffset;
            Length = length;
            DtStep = dtStep;
            EnableCache = enableCache;
            ThreadId = threadId;
        }

        public int[] Offsets { get; }
        public int FinalOffset { get; }
        public int Length { get; }
        public int DtStep { get; }
        public bool EnableCache { get; }
        public ThreadNr ThreadId { get; }
    }
}
