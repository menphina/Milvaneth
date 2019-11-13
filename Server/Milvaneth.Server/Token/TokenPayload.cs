using MessagePack;
using System;

namespace Milvaneth.Server.Token
{
    [MessagePackObject]
    public class TokenPayload
    {
        [Key(0)]
        public long TokenId { get; set; }
        [Key(1)]
        public int TimeSign { get; set; }
        [Key(2)]
        public long AccountId { get; set; }
        [Key(3)]
        public TokenPurpose Purpose { get; set; }
        [Key(4)]
        public long RelatedKey { get; set; }

        [IgnoreMember]
        public DateTime ValidTo
        {
            // Minute level precision
            get => new DateTime(600000000L * TimeSign, DateTimeKind.Utc);
            set => TimeSign = (int) (value.ToUniversalTime().Ticks / 600000000L);
        }

        [SerializationConstructor]
        public TokenPayload(long tokenId, int timeSign, long accountId, TokenPurpose purpose, long relatedKey)
        {
            TokenId = tokenId;
            TimeSign = timeSign;
            AccountId = accountId;
            Purpose = purpose;
            RelatedKey = relatedKey;
        }

        public TokenPayload(DateTime validToUtc, long accountId, TokenPurpose purpose, long relatedKey)
        {
            ValidTo = validToUtc;
            AccountId = accountId;
            Purpose = purpose;
            RelatedKey = relatedKey;
        }

        public byte[] ToPayloadBytes()
        {
            return MessagePackSerializer.Serialize(this);
        }

        public static byte[] ToPayloadBytes(TokenPayload payload)
        {
            return payload.ToPayloadBytes();
        }

        public static TokenPayload FromPayloadBytes(byte[] payload)
        {
            return MessagePackSerializer.Deserialize<TokenPayload>(payload);
        }
    }

    public static class TokenPayloadHelper
    {
        public static TokenPayload FromPayloadBytes(this byte[] payload)
        {
            return MessagePackSerializer.Deserialize<TokenPayload>(payload);
        }
    }
}
