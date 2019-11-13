namespace Thaliak.Network.Sniffer
{
    public interface ISnifferOutput
    {
        void Output(TimestampedData timestampedData);
    }
}