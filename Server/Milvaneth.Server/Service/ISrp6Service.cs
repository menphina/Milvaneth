namespace Milvaneth.Server.Service
{
    public interface ISrp6Service
    {
        long DoServerResponse(long accountId, int mode, byte[] verifier, out byte[] serverToken);
        bool DoServerValidate(long sessionId, byte[] clientToken, byte[] clientEvidence, out long accountId);
    }
}
