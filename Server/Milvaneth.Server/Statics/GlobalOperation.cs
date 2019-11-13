// ReSharper disable InconsistentNaming

namespace Milvaneth.Server.Statics
{
    public static class GlobalOperation
    {
        public const int GENERATE_APIKEY = 00_0001;
        public const int AUTH_START = 01_0001;
        public const int AUTH_FINISH = 01_0002;
        public const int AUTH_RESET = 01_0003;
        public const int ACCOUNT_CREATE = 01_0004;
        public const int ACCOUNT_STATUS = 01_0005;
        public const int ACCOUNT_RECOVERY_EMAIL = 01_0006;
        public const int ACCOUNT_RECOVERY_GAME = 01_0007;
        public const int SESSION_CREATE = 01_0008;
        public const int SESSION_CHANGE = 01_0009;
        public const int SESSION_RENEW = 01_0010;
        public const int SESSION_LOGOUT = 01_0011;
        public const int DATA_OVERVIEW = 01_0012;
        public const int DATA_ITEM = 01_0013;
        public const int DATA_UPLOAD = 03_0000;

        public const short COLUMN_CHARACTER_NEW = 02_0000;
        public const short COLUMN_CHARACTER_NAME = 02_0001;
        public const short COLUMN_CHARACTER_ACCOUNT = 02_0002;
        public const short COLUMN_CHARACTER_SERVICE = 02_0003;
        public const short COLUMN_CHARACTER_WORLD = 02_0004;
        public const short COLUMN_RETAINER_NEW = 02_0005;
        public const short COLUMN_RETAINER_NAME = 02_0006;
        public const short COLUMN_RETAINER_CHARA = 02_0007;
        public const short COLUMN_RETAINER_WORLD = 02_0008;
        public const short COLUMN_RETAINER_LOCATION = 02_0009;
    }
}
