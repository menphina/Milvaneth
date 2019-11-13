using Milvaneth.Common;
using Milvaneth.Communication.Foundation;

namespace Milvaneth.Communication.Api
{
    internal static class ApiCall
    {
        public static VerbMethod Get
        {
            get => ApiDefinition<MilvanethProtocol>.Get;
            set => ApiDefinition<MilvanethProtocol>.Get = value;
        }
        public static VerbMethod Delete
        {
            get => ApiDefinition<MilvanethProtocol>.Delete;
            set => ApiDefinition<MilvanethProtocol>.Delete = value;
        }

        public static VerbMethod Post
        {
            get => ApiDefinition<MilvanethProtocol>.Post;
            set => ApiDefinition<MilvanethProtocol>.Post = value;
        }

        public static VerbMethod Put
        {
            get => ApiDefinition<MilvanethProtocol>.Put;
            set => ApiDefinition<MilvanethProtocol>.Put = value;
        }

        // version 1: not rest
        // auth server
        public static ApiDefinition<MilvanethProtocol> AccountCreate = new ApiDefinition<MilvanethProtocol>(HttpVerb.Post, "account/create", false);
        public static ApiDefinition<MilvanethProtocol> AccountRecoveryEmail = new ApiDefinition<MilvanethProtocol>(HttpVerb.Post, "account/recoveryemail", false);
        public static ApiDefinition<MilvanethProtocol> AccountRecoveryGame = new ApiDefinition<MilvanethProtocol>(HttpVerb.Post, "account/recoverygame", false);
        public static ApiDefinition<MilvanethProtocol> AuthStart = new ApiDefinition<MilvanethProtocol>(HttpVerb.Post, "auth/start", false);
        public static ApiDefinition<MilvanethProtocol> AuthFinish = new ApiDefinition<MilvanethProtocol>(HttpVerb.Post, "auth/finish", false);
        public static ApiDefinition<MilvanethProtocol> AuthReset = new ApiDefinition<MilvanethProtocol>(HttpVerb.Post, "auth/reset", false);
        public static ApiDefinition<MilvanethProtocol> SessionCreate = new ApiDefinition<MilvanethProtocol>(HttpVerb.Post, "session/create", false);
        public static ApiDefinition<MilvanethProtocol> SessionChange = new ApiDefinition<MilvanethProtocol>(HttpVerb.Post, "session/change", false);
        public static ApiDefinition<MilvanethProtocol> SessionRenew = new ApiDefinition<MilvanethProtocol>(HttpVerb.Post, "session/renew", false);
        public static ApiDefinition<MilvanethProtocol> SessionLogout = new ApiDefinition<MilvanethProtocol>(HttpVerb.Get, "session/logout", false);

        // data server
        public static ApiDefinition<MilvanethProtocol> DataUpload = new ApiDefinition<MilvanethProtocol>(HttpVerb.Post, "data/upload", false);
        public static ApiDefinition<MilvanethProtocol> DataOverview = new ApiDefinition<MilvanethProtocol>(HttpVerb.Post, "data/overview", true);
        public static ApiDefinition<MilvanethProtocol> DataItem = new ApiDefinition<MilvanethProtocol>(HttpVerb.Get, "data/item", true);
        public static ApiDefinition<MilvanethProtocol> AccountStatus = new ApiDefinition<MilvanethProtocol>(HttpVerb.Get, "account/status", false);
        public static ApiDefinition<MilvanethProtocol> AccountUpdate = new ApiDefinition<MilvanethProtocol>(HttpVerb.Post, "account/update", false);

        // software server
        public static ApiDefinition<MilvanethProtocol> VersionCurrent = new ApiDefinition<MilvanethProtocol>(HttpVerb.Get, "version/current", true);
        public static ApiDefinition<MilvanethProtocol> VersionDownload = new ApiDefinition<MilvanethProtocol>(HttpVerb.Get, "version/download", true);
        public static ApiDefinition<MilvanethProtocol> UsageReport = new ApiDefinition<MilvanethProtocol>(HttpVerb.Post, "usage/report", false);
    }
}
