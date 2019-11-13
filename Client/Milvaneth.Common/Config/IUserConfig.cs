using MessagePack;

namespace Milvaneth.Common
{
    [Union(0, typeof(UserConfig))]
    [Union(1, typeof(UserInternalConfig))]
    public interface IUserConfig
    {
    }
}