namespace Thaliak.Network
{
    public interface INetworkMessageBase { }
    public interface INetworkMessageBase<T> : INetworkMessageBase
    {
        T Spawn(byte[] data, int offset);
    }
}
