// ReSharper disable InconsistentNaming

namespace Milvaneth.Server.Statics
{
    public class GlobalMessage
    {
        public const int OK_SUCCESS = 00_0000;
        public const int OK_NEXT_STEP = 00_0001;
        public const int OK_FINISHED = 00_0002;
        public const int OK_VERIFY_REQUEST_ACCEPTED = 00_0003;
        public const int OK_UPDATE_AVAILABLE = 00_0004;

        public const int RATE_PLEASE_RETRY = 01_0000;
        public const int RATE_LIMIT = 01_0001;
        public const int RATE_OVERLOADED = 01_0002;
        public const int RATE_POW_REQUIRED = 01_0003;

        public const int ERR_DATA_ERROR = 02_0007;
        public const int ERR_NO_PRIVILEGE = 02_0008;

        public const int DATA_PLEASE_RETRY = 03_0000;
        public const int DATA_INVALID_USERNAME = 03_0001;
        public const int DATA_USERNAME_OCCUPIED = 03_0002;
        public const int DATA_INVALID_NICKNAME = 03_0003;
        public const int DATA_NICKNAME_ILLEGAL = 03_0004;
        public const int DATA_INVALID_EMAIL = 03_0005;
        public const int DATA_EMAIL_BLACKLISTED = 03_0006;
        public const int DATA_NOT_ENOUGH = 03_0008;
        public const int DATA_INVALID_INPUT = 03_0010;
        public const int DATA_INVALID_CAPTCHA = 03_0011;
        public const int DATA_RECORD_MISMATCH = 03_0012;
        public const int DATA_NO_SUCH_USER = 03_0013;
        public const int DATA_NO_EMAIL_RECORDED = 03_0014;
        public const int DATA_TRACE_MISMATCH = 03_0015;

        public const int OP_TOKEN_NOT_FOUND = 04_0000;
        public const int OP_TOKEN_INVALID = 04_0001;
        public const int OP_DATA_ERROR = 04_0002;
        public const int OP_INVALID = 04_0003;
        public const int OP_NOT_ENOUGH_KARMA = 04_0004;
        public const int OP_PASSWORD_RETRY_TOO_MUCH = 04_0005;
        public const int OP_ACCOUNT_SUSPENDED = 04_0006;
        public const int OP_TOO_MANY_REQUEST = 04_0007;
        public const int OP_TOKEN_RENEW_REQUIRED = 02_0511;
        public const int OP_SANCTION = 04_4625;
    }
}
