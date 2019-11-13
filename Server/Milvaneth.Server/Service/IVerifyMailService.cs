namespace Milvaneth.Server.Service
{
    public interface IVerifyMailService
    {
        void SendCode(string email, string nickname, string code);
    }
}
