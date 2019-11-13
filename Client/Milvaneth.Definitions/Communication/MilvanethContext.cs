using MessagePack;

namespace Milvaneth.Common
{
    [MessagePackObject]
    public class MilvanethContext
    {
        [Key(0)]
        public long AccountId;
        [Key(1)]
        public uint ServiceId;
        [Key(2)]
        public long CharacterId;
        [Key(3)]
        public int World;
        [Key(4)]
        public long ConnectionNumber;

        public MilvanethContext Copy()
        {
            return new MilvanethContext
            {
                AccountId = AccountId,
                ServiceId = ServiceId,
                CharacterId = CharacterId,
                World = World,
                ConnectionNumber = ConnectionNumber
            };
        }

        public MilvanethContext CopyAccount()
        {
            return new MilvanethContext
            {
                AccountId = AccountId,
                ServiceId = ServiceId,
                ConnectionNumber = ConnectionNumber
            };
        }
    }
}
