using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessagePack;

namespace Milvaneth.Common
{
    [MessagePackObject]
    public class LobbyServiceResult : IResult
    {
        [Key(0)]
        public uint ServiceId;
        [Key(1)]
        public string ServiceProvider;
    }
}
