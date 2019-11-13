using MessagePack;

namespace Milvaneth.Common
{
    [MessagePackObject]
    public class ApiConfig
    {
        [Key(0)]
        public string[] Endpoints;
        [Key(1)]
        public string Prefix;
        [Key(2)]
        public string Mime;
    }
}
