using MessagePack;

namespace Milvaneth.Common
{
    [MessagePackObject]
    public class CurrentWorldResult : IResult
    {
        public CurrentWorldResult(int world)
        {
            WorldId = world;
        }

        [Key(0)]
        public int WorldId { get; }
    }
}
