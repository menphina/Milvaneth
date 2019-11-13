using MessagePack;
using System;
using Milvaneth.Common.Communication.Auth;
using Milvaneth.Common.Communication.Data;
using Milvaneth.Common.Communication.Login;
using Milvaneth.Common.Communication.Recovery;
using Milvaneth.Common.Communication.Register;
using Milvaneth.Common.Communication.Version;
using Milvaneth.Definitions.Communication.Data;

namespace Milvaneth.Common
{
    [Union(0, typeof(PackedResult))]
    [Union(1, typeof(RegisterForm))]
    [Union(2, typeof(ClientChallenge))]
    [Union(3, typeof(ServerChallenge))]
    [Union(4, typeof(ClientResponse))]
    [Union(5, typeof(ServerResponse))]
    [Union(6, typeof(AuthRequest))]
    [Union(7, typeof(AuthResponse))]
    [Union(8, typeof(AuthRenew))]
    [Union(9, typeof(RecoveryEmail))]
    [Union(10, typeof(RecoveryGame))]
    [Union(11, typeof(RecoveryRequest))]
    [Union(12, typeof(PackedResultBundle))]
    [Union(13, typeof(OverviewRequest))]
    [Union(14, typeof(OverviewResponse))]
    [Union(15, typeof(AccountUpdate))]
    [Union(16, typeof(AccountStatus))]
    [Union(17, typeof(VersionInfo))]
    [Union(18, typeof(VersionDownload))]
    public interface IMilvanethData
    {
        SafeDateTime ReportTime { get; }
    }
}
