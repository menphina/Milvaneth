using Microsoft.EntityFrameworkCore;
using System;

namespace Milvaneth.Server.Models
{
    public partial class MilvanethDbContext : DbContext
    {
        public MilvanethDbContext(DbContextOptions<MilvanethDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<AccountData> AccountData { get; set; }
        public virtual DbSet<AccountLog> AccountLog { get; set; }
        public virtual DbSet<ApiLog> ApiLog { get; set; }
        public virtual DbSet<CharacterData> CharacterData { get; set; }
        public virtual DbSet<DataLog> DataLog { get; set; }
        public virtual DbSet<EmailVerifyCode> EmailVerifyCode { get; set; }
        public virtual DbSet<HistoryData> HistoryData { get; set; }
        public virtual DbSet<KarmaLog> KarmaLog { get; set; }
        public virtual DbSet<KeyStore> KeyStore { get; set; }
        public virtual DbSet<KeyUsage> KeyUsage { get; set; }
        public virtual DbSet<ListingData> ListingData { get; set; }
        public virtual DbSet<OverviewData> OverviewData { get; set; }
        public virtual DbSet<PrivilegeConfig> PrivilegeConfig { get; set; }
        public virtual DbSet<RetainerData> RetainerData { get; set; }
        public virtual DbSet<TokenIssueList> TokenIssueList { get; set; }
        public virtual DbSet<TokenRevocationList> TokenRevocationList { get; set; }
        public virtual DbSet<VersionData> VersionData { get; set; }
        public virtual DbSet<VersionDownload> VersionDownload { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                throw new NotImplementedException();
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresExtension("timescaledb")
                .HasAnnotation("ProductVersion", "2.2.4-servicing-10062");

            modelBuilder.Entity<AccountData>(entity =>
            {
                entity.HasKey(e => e.AccountId)
                    .HasName("account_data_pkey");

                entity.ToTable("account_data");

                entity.HasIndex(e => e.AccountName)
                    .HasName("account_data_account_name_key")
                    .IsUnique();

                entity.Property(e => e.AccountId).HasColumnName("account_id");

                entity.Property(e => e.AccountName)
                    .IsRequired()
                    .HasColumnName("account_name")
                    .HasMaxLength(20);

                entity.Property(e => e.DisplayName)
                    .HasColumnName("display_name")
                    .HasMaxLength(20);

                entity.Property(e => e.Email)
                    .HasColumnName("email")
                    .HasMaxLength(40);

                entity.Property(e => e.EmailConfirmed).HasColumnName("email_confirmed");

                entity.Property(e => e.GroupParam).HasColumnName("group_param");

                entity.Property(e => e.Karma).HasColumnName("karma");

                entity.Property(e => e.LastRetry).HasColumnName("last_retry");

                entity.Property(e => e.PasswordRetry).HasColumnName("password_retry");

                entity.Property(e => e.PlayedCharacter).HasColumnName("played_character");

                entity.Property(e => e.PrivilegeLevel).HasColumnName("privilege_level");

                entity.Property(e => e.RegisterService).HasColumnName("register_service");

                entity.Property(e => e.RelatedService).HasColumnName("related_service");

                entity.Property(e => e.Salt)
                    .IsRequired()
                    .HasColumnName("salt");

                entity.Property(e => e.SuspendUntil).HasColumnName("suspend_until");

                entity.Property(e => e.Trace)
                    .IsRequired()
                    .HasColumnName("trace");

                entity.Property(e => e.Verifier)
                    .IsRequired()
                    .HasColumnName("verifier");

                entity.HasOne(d => d.PrivilegeLevelNavigation)
                    .WithMany(p => p.AccountData)
                    .HasForeignKey(d => d.PrivilegeLevel)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("account_data_privilege_level_fkey");
            });

            modelBuilder.Entity<AccountLog>(entity =>
            {
                entity.HasKey(e => new { e.RecordId, e.ReportTime })
                    .HasName("account_log_pkey");

                entity.ToTable("account_log");

                entity.HasIndex(e => e.ReportTime)
                    .HasName("account_log_report_time_idx");

                entity.Property(e => e.RecordId)
                    .HasColumnName("record_id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.ReportTime).HasColumnName("report_time");

                entity.Property(e => e.AccountId).HasColumnName("account_id");

                entity.Property(e => e.Detail)
                    .IsRequired()
                    .HasColumnName("detail")
                    .HasMaxLength(256);

                entity.Property(e => e.IpAddress)
                    .IsRequired()
                    .HasColumnName("ip_address")
                    .HasMaxLength(40);

                entity.Property(e => e.Message).HasColumnName("message");

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.AccountLog)
                    .HasForeignKey(d => d.AccountId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("account_log_account_id_fkey");
            });

            modelBuilder.Entity<ApiLog>(entity =>
            {
                entity.HasKey(e => new { e.RecordId, e.ReportTime })
                    .HasName("api_log_pkey");

                entity.ToTable("api_log");

                entity.HasIndex(e => e.ReportTime)
                    .HasName("api_log_report_time_idx");

                entity.Property(e => e.RecordId)
                    .HasColumnName("record_id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.ReportTime).HasColumnName("report_time");

                entity.Property(e => e.AccountId).HasColumnName("account_id");

                entity.Property(e => e.Detail)
                    .IsRequired()
                    .HasColumnName("detail")
                    .HasMaxLength(256);

                entity.Property(e => e.IpAddress)
                    .IsRequired()
                    .HasColumnName("ip_address")
                    .HasMaxLength(40);

                entity.Property(e => e.KeyId).HasColumnName("key_id");

                entity.Property(e => e.Operation).HasColumnName("operation");

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.ApiLog)
                    .HasForeignKey(d => d.AccountId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("api_log_account_id_fkey");

                entity.HasOne(d => d.Key)
                    .WithMany(p => p.ApiLog)
                    .HasForeignKey(d => d.KeyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("api_log_key_id_fkey");
            });

            modelBuilder.Entity<CharacterData>(entity =>
            {
                entity.HasKey(e => e.CharacterId)
                    .HasName("character_data_pkey");

                entity.ToTable("character_data");

                entity.HasIndex(e => e.AccountId)
                    .HasName("chara_account");

                entity.HasIndex(e => e.CharacterName)
                    .HasName("chara_name");

                entity.HasIndex(e => e.HomeWorld)
                    .HasName("chara_world");

                entity.HasIndex(e => e.ServiceId)
                    .HasName("chara_service");

                entity.Property(e => e.CharacterId)
                    .HasColumnName("character_id")
                    .ValueGeneratedNever();

                entity.Property(e => e.AccountId).HasColumnName("account_id");

                entity.Property(e => e.CharacterName)
                    .HasColumnName("character_name")
                    .HasMaxLength(20);

                entity.Property(e => e.GilHold).HasColumnName("gil_hold");

                entity.Property(e => e.HomeWorld).HasColumnName("home_world");

                entity.Property(e => e.Inventory).HasColumnName("inventory");

                entity.Property(e => e.JobLevels).HasColumnName("job_levels");

                entity.Property(e => e.RetainerList).HasColumnName("retainer_list");

                entity.Property(e => e.ServiceId).HasColumnName("service_id");

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.CharacterData)
                    .HasForeignKey(d => d.AccountId)
                    .HasConstraintName("character_data_account_id_fkey");
            });

            modelBuilder.Entity<DataLog>(entity =>
            {
                entity.HasKey(e => new { e.RecordId, e.ReportTime })
                    .HasName("data_log_pkey");

                entity.ToTable("data_log");

                entity.HasIndex(e => e.ReportTime)
                    .HasName("data_log_report_time_idx");

                entity.Property(e => e.RecordId)
                    .HasColumnName("record_id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.ReportTime).HasColumnName("report_time");

                entity.Property(e => e.FromValue)
                    .IsRequired()
                    .HasColumnName("from_value")
                    .HasMaxLength(256);

                entity.Property(e => e.Key).HasColumnName("key");

                entity.Property(e => e.Operator).HasColumnName("operator");

                entity.Property(e => e.TableColumn).HasColumnName("table_column");

                entity.Property(e => e.ToValue)
                    .IsRequired()
                    .HasColumnName("to_value")
                    .HasMaxLength(256);

                entity.HasOne(d => d.OperatorNavigation)
                    .WithMany(p => p.DataLog)
                    .HasForeignKey(d => d.Operator)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("data_log_operator_fkey");
            });

            modelBuilder.Entity<EmailVerifyCode>(entity =>
            {
                entity.HasKey(e => e.EventId)
                    .HasName("email_verify_code_pkey");

                entity.ToTable("email_verify_code");

                entity.Property(e => e.EventId).HasColumnName("event_id");

                entity.Property(e => e.AccountId).HasColumnName("account_id");

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasColumnName("code")
                    .HasMaxLength(40);

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasColumnName("email")
                    .HasMaxLength(40);

                entity.Property(e => e.FailedRetry).HasColumnName("failed_retry");

                entity.Property(e => e.SendTime).HasColumnName("send_time");

                entity.Property(e => e.ValidTo).HasColumnName("valid_to");

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.EmailVerifyCode)
                    .HasForeignKey(d => d.AccountId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("email_verify_code_account_id_fkey");
            });

            modelBuilder.Entity<HistoryData>(entity =>
            {
                entity.HasKey(e => new { e.RecordId, e.ReportTime })
                    .HasName("history_data_pkey");

                entity.ToTable("history_data");

                entity.HasIndex(e => e.PurchaseTime)
                    .HasName("history_time");

                entity.HasIndex(e => e.Quantity)
                    .HasName("hist_quantity");

                entity.HasIndex(e => e.ReportTime)
                    .HasName("history_data_report_time_idx");

                entity.HasIndex(e => e.UnitPrice)
                    .HasName("hist_price");

                entity.HasIndex(e => e.World)
                    .HasName("hist_world");

                entity.Property(e => e.RecordId)
                    .HasColumnName("record_id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.ReportTime).HasColumnName("report_time");

                entity.Property(e => e.BucketId).HasColumnName("bucket_id");

                entity.Property(e => e.BuyerName)
                    .IsRequired()
                    .HasColumnName("buyer_name")
                    .HasMaxLength(40);

                entity.Property(e => e.IsHq).HasColumnName("is_hq");

                entity.Property(e => e.ItemId).HasColumnName("item_id");

                entity.Property(e => e.OnMannequin).HasColumnName("on_mannequin");

                entity.Property(e => e.Padding).HasColumnName("padding");

                entity.Property(e => e.PurchaseTime).HasColumnName("purchase_time");

                entity.Property(e => e.Quantity).HasColumnName("quantity");

                entity.Property(e => e.ReporterId).HasColumnName("reporter_id");

                entity.Property(e => e.UnitPrice).HasColumnName("unit_price");

                entity.Property(e => e.World).HasColumnName("world");

                entity.HasOne(d => d.Reporter)
                    .WithMany(p => p.HistoryData)
                    .HasForeignKey(d => d.ReporterId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("history_data_reporter_id_fkey");
            });

            modelBuilder.Entity<KarmaLog>(entity =>
            {
                entity.HasKey(e => new { e.RecordId, e.ReportTime })
                    .HasName("karma_log_pkey");

                entity.ToTable("karma_log");

                entity.HasIndex(e => e.ReportTime)
                    .HasName("karma_log_report_time_idx");

                entity.Property(e => e.RecordId)
                    .HasColumnName("record_id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.ReportTime).HasColumnName("report_time");

                entity.Property(e => e.AccountId).HasColumnName("account_id");

                entity.Property(e => e.After).HasColumnName("after");

                entity.Property(e => e.Before).HasColumnName("before");

                entity.Property(e => e.Reason).HasColumnName("reason");

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.KarmaLog)
                    .HasForeignKey(d => d.AccountId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("karma_log_account_id_fkey");
            });

            modelBuilder.Entity<KeyStore>(entity =>
            {
                entity.HasKey(e => e.KeyId)
                    .HasName("key_store_pkey");

                entity.ToTable("key_store");

                entity.HasIndex(e => e.HoldingAccount)
                    .HasName("key_account");

                entity.HasIndex(e => e.Key)
                    .HasName("key_key");

                entity.Property(e => e.KeyId).HasColumnName("key_id");

                entity.Property(e => e.HoldingAccount).HasColumnName("holding_account");

                entity.Property(e => e.Key)
                    .IsRequired()
                    .HasColumnName("key");

                entity.Property(e => e.LastActive).HasColumnName("last_active");

                entity.Property(e => e.Quota).HasColumnName("quota");

                entity.Property(e => e.ReuseCounter).HasColumnName("reuse_counter");

                entity.Property(e => e.Usage).HasColumnName("usage");

                entity.Property(e => e.ValidFrom).HasColumnName("valid_from");

                entity.Property(e => e.ValidUntil).HasColumnName("valid_until");

                entity.HasOne(d => d.HoldingAccountNavigation)
                    .WithMany(p => p.KeyStore)
                    .HasForeignKey(d => d.HoldingAccount)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("key_store_holding_account_fkey");

                entity.HasOne(d => d.UsageNavigation)
                    .WithMany(p => p.KeyStore)
                    .HasForeignKey(d => d.Usage)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("key_store_usage_fkey");
            });

            modelBuilder.Entity<KeyUsage>(entity =>
            {
                entity.HasKey(e => e.Usage)
                    .HasName("key_usage_pkey");

                entity.ToTable("key_usage");

                entity.Property(e => e.Usage).HasColumnName("usage");

                entity.Property(e => e.AccessData).HasColumnName("access_data");

                entity.Property(e => e.BatchRead).HasColumnName("batch_read");

                entity.Property(e => e.BatchWrite).HasColumnName("batch_write");

                entity.Property(e => e.ChangePassword).HasColumnName("change_password");

                entity.Property(e => e.CreateSession).HasColumnName("create_session");

                entity.Property(e => e.GetChangeToken).HasColumnName("get_change_token");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(40);

                entity.Property(e => e.ProveIdentity).HasColumnName("prove_identity");

                entity.Property(e => e.RenewSession).HasColumnName("renew_session");
            });

            modelBuilder.Entity<ListingData>(entity =>
            {
                entity.HasKey(e => new { e.RecordId, e.ReportTime })
                    .HasName("listing_data_pkey");

                entity.ToTable("listing_data");

                entity.HasIndex(e => e.ItemId)
                    .HasName("list_item");

                entity.HasIndex(e => e.Quantity)
                    .HasName("list_quantity");

                entity.HasIndex(e => e.ReportTime)
                    .HasName("listing_data_report_time_idx");

                entity.HasIndex(e => e.UnitPrice)
                    .HasName("list_price");

                entity.HasIndex(e => e.World)
                    .HasName("list_world");

                entity.Property(e => e.RecordId)
                    .HasColumnName("record_id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.ReportTime).HasColumnName("report_time");

                entity.Property(e => e.ArtisanId).HasColumnName("artisan_id");

                entity.Property(e => e.BucketId).HasColumnName("bucket_id");

                entity.Property(e => e.Condition).HasColumnName("condition");

                entity.Property(e => e.ContainerId).HasColumnName("container_id");

                entity.Property(e => e.DyeId).HasColumnName("dye_id");

                entity.Property(e => e.IsHq).HasColumnName("is_hq");

                entity.Property(e => e.ItemId).HasColumnName("item_id");

                entity.Property(e => e.ListingId).HasColumnName("listing_id");

                entity.Property(e => e.Materia1).HasColumnName("materia1");

                entity.Property(e => e.Materia2).HasColumnName("materia2");

                entity.Property(e => e.Materia3).HasColumnName("materia3");

                entity.Property(e => e.Materia4).HasColumnName("materia4");

                entity.Property(e => e.Materia5).HasColumnName("materia5");

                entity.Property(e => e.MateriaCount).HasColumnName("materia_count");

                entity.Property(e => e.OnMannequin).HasColumnName("on_mannequin");

                entity.Property(e => e.OwnerId).HasColumnName("owner_id");

                entity.Property(e => e.PlayerName)
                    .HasColumnName("player_name")
                    .HasMaxLength(40);

                entity.Property(e => e.Quantity).HasColumnName("quantity");

                entity.Property(e => e.ReporterId).HasColumnName("reporter_id");

                entity.Property(e => e.RetainerId).HasColumnName("retainer_id");

                entity.Property(e => e.RetainerLoc).HasColumnName("retainer_loc");

                entity.Property(e => e.RetainerName)
                    .HasColumnName("retainer_name")
                    .HasMaxLength(40);

                entity.Property(e => e.SlotId).HasColumnName("slot_id");

                entity.Property(e => e.SpiritBond).HasColumnName("spirit_bond");

                entity.Property(e => e.TotalTax).HasColumnName("total_tax");

                entity.Property(e => e.UnitPrice).HasColumnName("unit_price");

                entity.Property(e => e.UpdateTime).HasColumnName("update_time");

                entity.Property(e => e.World).HasColumnName("world");

                entity.HasOne(d => d.Artisan)
                    .WithMany(p => p.ListingDataArtisan)
                    .HasForeignKey(d => d.ArtisanId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("listing_data_artisan_id_fkey");

                entity.HasOne(d => d.Owner)
                    .WithMany(p => p.ListingDataOwner)
                    .HasForeignKey(d => d.OwnerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("listing_data_owner_id_fkey");

                entity.HasOne(d => d.Reporter)
                    .WithMany(p => p.ListingDataReporter)
                    .HasForeignKey(d => d.ReporterId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("listing_data_reporter_id_fkey");

                entity.HasOne(d => d.Retainer)
                    .WithMany(p => p.ListingData)
                    .HasForeignKey(d => d.RetainerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("listing_data_retainer_id_fkey");
            });

            modelBuilder.Entity<OverviewData>(entity =>
            {
                entity.HasKey(e => new { e.RecordId, e.ReportTime })
                    .HasName("overview_data_pkey");

                entity.ToTable("overview_data");

                entity.HasIndex(e => e.ReportTime)
                    .HasName("overview_data_report_time_idx");

                entity.Property(e => e.RecordId)
                    .HasColumnName("record_id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.ReportTime).HasColumnName("report_time");

                entity.Property(e => e.BucketId).HasColumnName("bucket_id");

                entity.Property(e => e.Demand).HasColumnName("demand");

                entity.Property(e => e.ItemId).HasColumnName("item_id");

                entity.Property(e => e.OpenListing).HasColumnName("open_listing");

                entity.Property(e => e.ReporterId).HasColumnName("reporter_id");

                entity.Property(e => e.World).HasColumnName("world");

                entity.HasOne(d => d.Reporter)
                    .WithMany(p => p.OverviewData)
                    .HasForeignKey(d => d.ReporterId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("overview_data_reporter_id_fkey");
            });

            modelBuilder.Entity<PrivilegeConfig>(entity =>
            {
                entity.HasKey(e => e.PrivilegeLevel)
                    .HasName("privilege_config_pkey");

                entity.ToTable("privilege_config");

                entity.Property(e => e.PrivilegeLevel).HasColumnName("privilege_level");

                entity.Property(e => e.AccessData).HasColumnName("access_data");

                entity.Property(e => e.AccessStatics).HasColumnName("access_statics");

                entity.Property(e => e.AccountManagement).HasColumnName("account_management");

                entity.Property(e => e.AccountOperation).HasColumnName("account_operation");

                entity.Property(e => e.BatchRead).HasColumnName("batch_read");

                entity.Property(e => e.BatchWrite).HasColumnName("batch_write");

                entity.Property(e => e.Debug).HasColumnName("debug");

                entity.Property(e => e.DeleteRecord).HasColumnName("delete_record");

                entity.Property(e => e.IgnoreKarma).HasColumnName("ignore_karma");

                entity.Property(e => e.Login).HasColumnName("login");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(40);

                entity.Property(e => e.ReleaseUpdate).HasColumnName("release_update");
            });

            modelBuilder.Entity<RetainerData>(entity =>
            {
                entity.HasKey(e => e.RetainerId)
                    .HasName("retainer_data_pkey");

                entity.ToTable("retainer_data");

                entity.HasIndex(e => e.Character)
                    .HasName("retain_chara");

                entity.HasIndex(e => e.RetainerName)
                    .HasName("retain_name");

                entity.Property(e => e.RetainerId)
                    .HasColumnName("retainer_id")
                    .ValueGeneratedNever();

                entity.Property(e => e.Character).HasColumnName("character");

                entity.Property(e => e.Inventory).HasColumnName("inventory");

                entity.Property(e => e.Location).HasColumnName("location");

                entity.Property(e => e.RetainerName)
                    .IsRequired()
                    .HasColumnName("retainer_name")
                    .HasMaxLength(20);

                entity.Property(e => e.World).HasColumnName("world");

                entity.HasOne(d => d.CharacterNavigation)
                    .WithMany(p => p.RetainerData)
                    .HasForeignKey(d => d.Character)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("retainer_data_character_fkey");
            });

            modelBuilder.Entity<TokenIssueList>(entity =>
            {
                entity.HasKey(e => e.TokenSerial)
                    .HasName("token_issue_list_pkey");

                entity.ToTable("token_issue_list");

                entity.HasIndex(e => e.HoldingAccount)
                    .HasName("token_account");

                entity.Property(e => e.TokenSerial).HasColumnName("token_serial");

                entity.Property(e => e.HoldingAccount).HasColumnName("holding_account");

                entity.Property(e => e.IssueTime).HasColumnName("issue_time");

                entity.Property(e => e.Reason).HasColumnName("reason");

                entity.Property(e => e.ValidUntil).HasColumnName("valid_until");

                entity.HasOne(d => d.HoldingAccountNavigation)
                    .WithMany(p => p.TokenIssueList)
                    .HasForeignKey(d => d.HoldingAccount)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("token_issue_list_holding_account_fkey");
            });

            modelBuilder.Entity<TokenRevocationList>(entity =>
            {
                entity.HasKey(e => e.TokenSerial)
                    .HasName("token_revocation_list_pkey");

                entity.ToTable("token_revocation_list");

                entity.Property(e => e.TokenSerial)
                    .HasColumnName("token_serial")
                    .ValueGeneratedNever();

                entity.Property(e => e.Reason).HasColumnName("reason");

                entity.Property(e => e.RevokeSince).HasColumnName("revoke_since");

                entity.HasOne(d => d.TokenSerialNavigation)
                    .WithOne(p => p.TokenRevocationList)
                    .HasForeignKey<TokenRevocationList>(d => d.TokenSerial)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("token_revocation_list_token_serial_fkey");
            });

            modelBuilder.Entity<VersionData>(entity =>
            {
                entity.HasKey(e => e.VersionId)
                    .HasName("version_data_pkey");

                entity.ToTable("version_data");

                entity.HasIndex(e => e.DataVersion)
                    .HasName("version_dataver");

                entity.HasIndex(e => e.MilVersion)
                    .HasName("version_mil");

                entity.Property(e => e.VersionId).HasColumnName("version_id");

                entity.Property(e => e.BundleKey)
                    .IsRequired()
                    .HasColumnName("bundle_key")
                    .HasMaxLength(40);

                entity.Property(e => e.CustomMessage)
                    .HasColumnName("custom_message")
                    .HasMaxLength(256);

                entity.Property(e => e.DataVersion).HasColumnName("data_version");

                entity.Property(e => e.ForceUpdate).HasColumnName("force_update");

                entity.Property(e => e.GameVersion).HasColumnName("game_version");

                entity.Property(e => e.MilVersion).HasColumnName("mil_version");

                entity.Property(e => e.UpdateTo).HasColumnName("update_to");

                entity.HasOne(d => d.BundleKeyNavigation)
                    .WithMany(p => p.VersionData)
                    .HasForeignKey(d => d.BundleKey)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("version_data_bundle_key_fkey");
            });

            modelBuilder.Entity<VersionDownload>(entity =>
            {
                entity.HasKey(e => e.BundleKey)
                    .HasName("version_download_pkey");

                entity.ToTable("version_download");

                entity.Property(e => e.BundleKey)
                    .HasColumnName("bundle_key")
                    .HasMaxLength(40)
                    .ValueGeneratedNever();

                entity.Property(e => e.Argument)
                    .IsRequired()
                    .HasColumnName("argument")
                    .HasMaxLength(128);

                entity.Property(e => e.BinaryUpdate).HasColumnName("binary_update");

                entity.Property(e => e.DataUpdate).HasColumnName("data_update");

                entity.Property(e => e.FileServer)
                    .IsRequired()
                    .HasColumnName("file_server")
                    .HasMaxLength(64);

                entity.Property(e => e.Files)
                    .IsRequired()
                    .HasColumnName("files");

                entity.Property(e => e.UpdaterUpdate).HasColumnName("updater_update");
            });
        }
    }
}
