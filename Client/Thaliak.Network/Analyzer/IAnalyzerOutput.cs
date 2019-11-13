namespace Thaliak.Network.Analyzer
{
    public interface IAnalyzerOutput
    {
        void Output(AnalyzedPacket analyzedPacket);
    }
}
