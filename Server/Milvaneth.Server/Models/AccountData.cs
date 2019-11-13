using System;
using System.Collections.Generic;

namespace Milvaneth.Server.Models
{
    public partial class AccountData
    {
        public AccountData()
        {
            AccountLog = new HashSet<AccountLog>();
            ApiLog = new HashSet<ApiLog>();
            CharacterData = new HashSet<CharacterData>();
            DataLog = new HashSet<DataLog>();
            EmailVerifyCode = new HashSet<EmailVerifyCode>();
            KarmaLog = new HashSet<KarmaLog>();
            KeyStore = new HashSet<KeyStore>();
            TokenIssueList = new HashSet<TokenIssueList>();
        }

        public long AccountId { get; set; }
        public string AccountName { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public byte[] Salt { get; set; }
        public byte[] Verifier { get; set; }
        public short GroupParam { get; set; }
        public long RegisterService { get; set; }
        public long[] RelatedService { get; set; }
        public long[] PlayedCharacter { get; set; }
        public long[] Trace { get; set; }
        public long Karma { get; set; }
        public int PrivilegeLevel { get; set; }
        public DateTime? SuspendUntil { get; set; }
        public short? PasswordRetry { get; set; }
        public DateTime? LastRetry { get; set; }

        public virtual PrivilegeConfig PrivilegeLevelNavigation { get; set; }
        public virtual ICollection<AccountLog> AccountLog { get; set; }
        public virtual ICollection<ApiLog> ApiLog { get; set; }
        public virtual ICollection<CharacterData> CharacterData { get; set; }
        public virtual ICollection<DataLog> DataLog { get; set; }
        public virtual ICollection<EmailVerifyCode> EmailVerifyCode { get; set; }
        public virtual ICollection<KarmaLog> KarmaLog { get; set; }
        public virtual ICollection<KeyStore> KeyStore { get; set; }
        public virtual ICollection<TokenIssueList> TokenIssueList { get; set; }
    }
}
