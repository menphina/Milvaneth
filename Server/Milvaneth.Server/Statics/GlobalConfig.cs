// ReSharper disable InconsistentNaming

using Microsoft.Extensions.Configuration;
using Milvaneth.Common;
using System.Linq;

namespace Milvaneth.Server.Statics
{
    public class GlobalConfig
    {
        public static IConfigurationSection Section;

        public static readonly int TOKEN_LENGTH = Section?.GetValue("TOKEN_LENGTH", 48) ?? 48;
        public static readonly int TOKEN_ACCOUNT_LIFE_TIME = Section?.GetValue("TOKEN_ACCOUNT_LIFE_TIME", 5 * 60) ?? 5 * 60;
        public static readonly int TOKEN_DATA_LIFE_TIME = Section?.GetValue("TOKEN_DATA_LIFE_TIME", 24 * 60 * 60) ?? 24 * 60 * 60;
        public static readonly int TOKEN_RENEW_LIFE_TIME = Section?.GetValue("TOKEN_RENEW_LIFE_TIME", 30 * 24 * 60 * 60) ?? 30 * 24 * 60 * 60;

        public static readonly int SALT_MIN_LENGTH = Section?.GetValue("SALT_MIN_LENGTH", 16) ?? 16;
        public static readonly int SALT_MAX_LENGTH = Section?.GetValue("SALT_MAX_LENGTH", 64) ?? 64;

        public static readonly bool POW_ENABLE = Section?.GetValue("POW_ENABLE", true) ?? true;
        public static readonly int POW_LIFE_TIME = Section?.GetValue("POW_LIFE_TIME", 5 * 60) ?? 5 * 60;
        public static readonly int POW_HEADER_LENGTH = Section?.GetValue("POW_HEADER_LENGTH", 1 + 4 + 8) ?? 1 + 4 + 8;
        public static readonly int POW_LENGTH = POW_HEADER_LENGTH + 32;
        public static readonly int POW_REQUIRED_THRESHOLD = Section?.GetValue("POW_REQUIRED_THRESHOLD", 18) ?? 18;
        public static readonly int POW_SENSITIVE_OPERATION = Section?.GetValue("POW_SENSITIVE_OPERATION", 20) ?? 20;

        public static readonly int SRP6_SESSION_LIFE_TIME = Section?.GetValue("SRP6_SESSION_LIFE_TIME", 5 * 60) ?? 5 * 60;
        public static readonly int[] SRP6_ALLOWED_PARAM_GROUP = Section?.GetValue("SRP6_ALLOWED_PARAM_GROUP", new[] {2048}) ?? new[] {2048};

        public static readonly int ACCOUNT_BLOCKED_LEVEL = Section?.GetValue("ACCOUNT_BLOCKED_LEVEL", 2) ?? 2;
        public static readonly int ACCOUNT_VERIFY_CODE_LIFE_TIME = Section?.GetValue("ACCOUNT_VERIFY_CODE_LIFE_TIME", 10 * 60) ?? 10 * 60;
        public static readonly int ACCOUNT_VERIFY_CODE_COOLDOWN = Section?.GetValue("ACCOUNT_VERIFY_CODE_COOLDOWN", 1 * 60) ?? 1 * 60;
        public static readonly int ACCOUNT_PASSWORD_RETRY_TOLERANCE = Section?.GetValue("ACCOUNT_PASSWORD_RETRY_TOLERANCE", 5) ?? 5;
        public static readonly int ACCOUNT_PASSWORD_RETRY_COOLDOWN = Section?.GetValue("ACCOUNT_PASSWORD_RETRY_COOLDOWN", 5 * 60) ?? 5 * 60;

        public static readonly int INVOKE_DELAY_TOLERANCE = Section?.GetValue("INVOKE_DELAY_TOLERANCE", 2) ?? 2;

        public static readonly int USER_INITIAL_KARMA = Section?.GetValue("USER_INITIAL_KARMA", 1000) ?? 1000;
        public static readonly bool USER_ENABLE_KARMA = Section?.GetValue("USER_ENABLE_KARMA", true) ?? true;

        public static readonly int DATA_OVERVIEW_QUERY_LIMIT = Section?.GetValue("DATA_OVERVIEW_QUERY_LIMIT", 100) ?? 100;

        public static readonly string SERVICE_PROVIDER_ID = Section?.GetValue("SERVICE_PROVIDER_ID", "SHANDA") ?? "SHANDA";

        private static readonly int[] _SG1 = {1042, 1043, 1044, 1045, 1060, 1081, 1106, 1167, 1169};
        private static readonly int[] _SG2 = { 1076, 1113, 1170, 1171, 1172 };
        private static readonly byte[] _RL = {0, 1, 2, 3, 4, 7, 10};

        public static readonly int[] CHARA_SERVER_GROUP1 = Section?.GetValue("CHARA_SERVER_GROUP1", _SG1) ?? _SG1;
        public static readonly int[] CHARA_SERVER_GROUP2 = Section?.GetValue("CHARA_SERVER_GROUP2", _SG2) ?? _SG2;
        public static readonly int[] CHARA_SERVER_AVAILABLE = CHARA_SERVER_GROUP1.Union(CHARA_SERVER_GROUP2).ToArray();
        public static readonly int CHARA_SERVER_GENESIS = Section?.GetValue("CHARA_SERVER_GENESIS", 1405958400) ?? 1405958400;

        public static readonly byte[] MARKET_RETAINER_LOCALTION = Section?.GetValue("MARKET_RETAINER_LOCALTION", _RL) ?? _RL;
        public static readonly InventoryContainerId MARKET_RETAINER_CONTAINER = (InventoryContainerId) (Section?.GetValue("MARKET_RETAINER_CONTAINER", 12002) ?? 12002);
        public static readonly int MARKET_PRICE_THRESHOLD = Section?.GetValue("MARKET_PRICE_THRESHOLD", 999_999_999) ?? 999_999_999;
        public static readonly int MARKET_COUNT_THRESHOLD = Section?.GetValue("MARKET_COUNT_THRESHOLD", 9999) ?? 9999;

    }
}
