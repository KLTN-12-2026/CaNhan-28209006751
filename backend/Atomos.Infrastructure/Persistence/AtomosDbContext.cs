using Atomos.Application.Common;
using Atomos.Application.Auth;
using Atomos.Domain.Admin;
using Atomos.Domain.About;
using Atomos.Domain.Challenges;
using Atomos.Domain.Chemistry;
using Atomos.Domain.Learning;
using Atomos.Domain.Payments;
using Atomos.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Atomos.Infrastructure.Persistence;

public sealed class AtomosDbContext(DbContextOptions<AtomosDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<AuthSession> AuthSessions => Set<AuthSession>();
    public DbSet<Chemical> Chemicals => Set<Chemical>();
    public DbSet<Reaction> Reactions => Set<Reaction>();
    public DbSet<ReactionParticipant> ReactionParticipants => Set<ReactionParticipant>();
    public DbSet<ReactionRule> ReactionRules => Set<ReactionRule>();
    public DbSet<ReactionRuleSlot> ReactionRuleSlots => Set<ReactionRuleSlot>();
    public DbSet<ReactionRuleCondition> ReactionRuleConditions => Set<ReactionRuleCondition>();
    public DbSet<ReactionRuleProduct> ReactionRuleProducts => Set<ReactionRuleProduct>();
    public DbSet<ReactionRuleOutcome> ReactionRuleOutcomes => Set<ReactionRuleOutcome>();
    public DbSet<PeriodicElement> PeriodicElements => Set<PeriodicElement>();
    public DbSet<ChemicalComponent> ChemicalComponents => Set<ChemicalComponent>();
    public DbSet<ElementOxidationState> ElementOxidationStates => Set<ElementOxidationState>();
    public DbSet<ChemicalOxidationState> ChemicalOxidationStates => Set<ChemicalOxidationState>();
    public DbSet<ChemicalRedoxCapability> ChemicalRedoxCapabilities => Set<ChemicalRedoxCapability>();
    public DbSet<ChemicalWaterReactivity> ChemicalWaterReactivities => Set<ChemicalWaterReactivity>();
    public DbSet<ChemicalAmphotericBehavior> ChemicalAmphotericBehaviors => Set<ChemicalAmphotericBehavior>();
    public DbSet<ChemicalAcidBehavior> ChemicalAcidBehaviors => Set<ChemicalAcidBehavior>();
    public DbSet<ChemicalProductFamily> ChemicalProductFamilies => Set<ChemicalProductFamily>();
    public DbSet<UserChemical> UserChemicals => Set<UserChemical>();
    public DbSet<Chapter> Chapters => Set<Chapter>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<LessonProgress> LessonProgresses => Set<LessonProgress>();
    public DbSet<LearningProfile> LearningProfiles => Set<LearningProfile>();
    public DbSet<LearningNote> LearningNotes => Set<LearningNote>();
    public DbSet<Exercise> Exercises => Set<Exercise>();
    public DbSet<ExerciseSet> ExerciseSets => Set<ExerciseSet>();
    public DbSet<ExerciseSetQuestion> ExerciseSetQuestions => Set<ExerciseSetQuestion>();
    public DbSet<ExerciseChoice> ExerciseChoices => Set<ExerciseChoice>();
    public DbSet<Exam> Exams => Set<Exam>();
    public DbSet<ExamExercise> ExamExercises => Set<ExamExercise>();
    public DbSet<LearningAttempt> LearningAttempts => Set<LearningAttempt>();
    public DbSet<LearningAttemptDetail> LearningAttemptDetails => Set<LearningAttemptDetail>();
    public DbSet<AboutContent> AboutContents => Set<AboutContent>();
    public DbSet<UsageQuota> UsageQuotas => Set<UsageQuota>();
    public DbSet<AdminAuditLog> AdminAuditLogs => Set<AdminAuditLog>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
    public DbSet<KnowledgeProposal> KnowledgeProposals => Set<KnowledgeProposal>();
    public DbSet<KnowledgeProposalSource> KnowledgeProposalSources => Set<KnowledgeProposalSource>();
    public DbSet<KnowledgeProposalRevision> KnowledgeProposalRevisions => Set<KnowledgeProposalRevision>();
    public DbSet<KnowledgeProposalJob> KnowledgeProposalJobs => Set<KnowledgeProposalJob>();
    public DbSet<PaymentConfiguration> PaymentConfigurations => Set<PaymentConfiguration>();
    public DbSet<PaymentOrder> PaymentOrders => Set<PaymentOrder>();
    public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();
    public DbSet<WorkshopChallenge> WorkshopChallenges => Set<WorkshopChallenge>();
    public DbSet<WorkshopChallengeResource> WorkshopChallengeResources => Set<WorkshopChallengeResource>();
    public DbSet<WorkshopChallengeSession> WorkshopChallengeSessions => Set<WorkshopChallengeSession>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Role");
            entity.HasKey(role => role.IDRole);
            entity.Property(role => role.IDRole).HasColumnName("IDRole");
            entity.Property(role => role.TenRole)
                .HasColumnName("TenRole")
                .HasMaxLength(30)
                .IsRequired();
            entity.HasIndex(role => role.TenRole).IsUnique();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("NguoiDung");
            entity.HasKey(user => user.IDUser);
            entity.Property(user => user.IDUser).HasColumnName("IDUser");
            entity.Property(user => user.IDRole).HasColumnName("IDRole");
            entity.Property(user => user.Username)
                .HasColumnName("Username")
                .HasMaxLength(50)
                .IsRequired();
            entity.Property(user => user.Email)
                .HasColumnName("Email")
                .HasMaxLength(100)
                .IsRequired();
            entity.Property(user => user.MatKhau)
                .HasColumnName("MatKhau")
                .HasMaxLength(500);
            entity.Property(user => user.MatKhauTruocDo)
                .HasColumnName("MatKhauTruocDo")
                .HasMaxLength(500);
            entity.Property(user => user.IDGoogle)
                .HasColumnName("IDGoogle")
                .HasMaxLength(128);
            entity.Property(user => user.HoTen)
                .HasColumnName("HoTen")
                .HasMaxLength(100);
            entity.Property(user => user.Avatar).HasColumnName("Avatar");
            entity.Property(user => user.VipExpiresAt)
                .HasColumnName("VIPHetHan")
                .HasColumnType("datetime2");
            entity.Property(user => user.DaKhoa)
                .HasColumnName("DaKhoa")
                .HasDefaultValue(false);
            entity.Property(user => user.DaXoa)
                .HasColumnName("DaXoa")
                .HasDefaultValue(false);
            entity.Property(user => user.NgayTao)
                .HasColumnName("NgayTao")
                .HasColumnType("datetime2");
            entity.Property(user => user.NgayCapNhat)
                .HasColumnName("NgayCapNhat")
                .HasColumnType("datetime2");
            entity.HasIndex(user => user.Username).IsUnique();
            entity.HasIndex(user => user.Email).IsUnique();
            entity.HasIndex(user => user.IDGoogle).IsUnique();
            entity.HasIndex(user => user.IDRole);
            entity.HasOne(user => user.Role)
                .WithMany(role => role.Users)
                .HasForeignKey(user => user.IDRole)
                .HasConstraintName("FK_NguoiDung_Role");
        });

        modelBuilder.Entity<UsageQuota>(entity =>
        {
            entity.ToTable("GioiHanSuDung");
            entity.HasKey(quota => quota.IDUser);
            entity.Property(quota => quota.IDUser).HasColumnName("IDUser");
            entity.Property(quota => quota.SandboxWindowStartedAt)
                .HasColumnName("SandboxCuaSoBatDauLuc")
                .HasColumnType("datetime2");
            entity.Property(quota => quota.SandboxUsedSeconds)
                .HasColumnName("SandboxSoGiayDaDung")
                .HasDefaultValue(0);
            entity.Property(quota => quota.SandboxHeartbeatAt)
                .HasColumnName("SandboxHeartbeatLuc")
                .HasColumnType("datetime2");
            entity.Property(quota => quota.SandboxIsActive)
                .HasColumnName("SandboxDangHoatDong")
                .HasDefaultValue(false);
            entity.HasOne(quota => quota.User)
                .WithOne(user => user.UsageQuota)
                .HasForeignKey<UsageQuota>(quota => quota.IDUser)
                .HasConstraintName("FK_GioiHanSuDung_NguoiDung")
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ActivityLog>(entity =>
        {
            entity.ToTable("NhatKyHoatDong");
            entity.HasKey(log => log.IDActivityLog);
            entity.Property(log => log.IDActivityLog).HasColumnName("IDNhatKyHoatDong");
            entity.Property(log => log.IDUser).HasColumnName("IDUser");
            entity.Property(log => log.EventType).HasColumnName("LoaiSuKien").HasMaxLength(50).IsRequired();
            entity.Property(log => log.OccurredAt)
                .HasColumnName("XayRaLuc")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("SYSUTCDATETIME()");
            entity.Property(log => log.EntityType).HasColumnName("LoaiDoiTuong").HasMaxLength(50);
            entity.Property(log => log.EntityId).HasColumnName("IDDoiTuong");
            entity.Property(log => log.MetadataJson).HasColumnName("MetadataJson");
            entity.HasIndex(log => new { log.OccurredAt, log.EventType })
                .HasDatabaseName("IX_NhatKyHoatDong_XayRaLuc_LoaiSuKien");
            entity.HasIndex(log => new { log.IDUser, log.OccurredAt })
                .HasDatabaseName("IX_NhatKyHoatDong_IDUser_XayRaLuc");
            entity.HasIndex(log => new { log.EntityType, log.EntityId })
                .HasDatabaseName("IX_NhatKyHoatDong_DoiTuong");
            entity.HasOne(log => log.User)
                .WithMany()
                .HasForeignKey(log => log.IDUser)
                .HasConstraintName("FK_NhatKyHoatDong_NguoiDung")
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<PaymentConfiguration>(entity =>
        {
            entity.ToTable("CauHinhThanhToan");
            entity.HasKey(config => config.IDPaymentConfiguration);
            entity.Property(config => config.IDPaymentConfiguration).HasColumnName("IDCauHinhThanhToan");
            entity.Property(config => config.Provider).HasColumnName("NhaCungCap").HasMaxLength(50).IsRequired();
            entity.Property(config => config.VipPrice).HasColumnName("GiaVip").HasColumnType("decimal(18,2)");
            entity.Property(config => config.Currency).HasColumnName("TienTe").HasMaxLength(3).IsRequired();
            entity.Property(config => config.ReceiverName).HasColumnName("TenNguoiNhan").HasMaxLength(150);
            entity.Property(config => config.ReceiverAccount).HasColumnName("TaiKhoanNhan").HasMaxLength(100);
            entity.Property(config => config.ReceiverBank).HasColumnName("NganHangNhan").HasMaxLength(150);
            entity.Property(config => config.BankCode).HasColumnName("MaNganHang").HasMaxLength(20);
            entity.Property(config => config.ProviderMerchantId).HasColumnName("MaMerchant").HasMaxLength(100);
            entity.Property(config => config.IsActive).HasColumnName("DangHoatDong").HasDefaultValue(true);
            entity.Property(config => config.CreatedAt).HasColumnName("TaoLuc").HasColumnType("datetime2");
            entity.Property(config => config.UpdatedAt).HasColumnName("CapNhatLuc").HasColumnType("datetime2");
            entity.HasIndex(config => config.Provider).HasDatabaseName("IX_CauHinhThanhToan_NhaCungCap");
        });

        modelBuilder.Entity<PaymentOrder>(entity =>
        {
            entity.ToTable("DonThanhToan");
            entity.HasKey(order => order.IDPaymentOrder);
            entity.Property(order => order.IDPaymentOrder).HasColumnName("IDDonThanhToan");
            entity.Property(order => order.IDUser).HasColumnName("IDUser");
            entity.Property(order => order.OrderCode).HasColumnName("MaDonHang").HasMaxLength(40).IsRequired();
            entity.Property(order => order.Amount).HasColumnName("SoTien").HasColumnType("decimal(18,2)");
            entity.Property(order => order.Currency).HasColumnName("TienTe").HasMaxLength(3).IsRequired();
            entity.Property(order => order.Status).HasColumnName("TrangThai").HasMaxLength(20).IsRequired();
            entity.Property(order => order.Provider).HasColumnName("NhaCungCap").HasMaxLength(50).IsRequired();
            entity.Property(order => order.Description).HasColumnName("MoTa").HasMaxLength(250).IsRequired();
            entity.Property(order => order.ProviderOrderCode).HasColumnName("MaDonHangNhaCungCap").HasMaxLength(100);
            entity.Property(order => order.ProviderPaymentId).HasColumnName("MaThanhToanNhaCungCap").HasMaxLength(100);
            entity.Property(order => order.ProviderTransactionId).HasColumnName("MaGiaoDichNhaCungCap").HasMaxLength(100);
            entity.Property(order => order.CheckoutUrl).HasColumnName("CheckoutUrl").HasMaxLength(500);
            entity.Property(order => order.QrPayload).HasColumnName("QrPayload");
            entity.Property(order => order.PaymentContent).HasColumnName("NoiDungChuyenKhoan").HasMaxLength(250);
            entity.Property(order => order.CreatedAt).HasColumnName("TaoLuc").HasColumnType("datetime2");
            entity.Property(order => order.ExpiresAt).HasColumnName("HetHanLuc").HasColumnType("datetime2");
            entity.Property(order => order.PaidAt).HasColumnName("ThanhToanLuc").HasColumnType("datetime2");
            entity.Property(order => order.ConfirmedAt).HasColumnName("XacNhanLuc").HasColumnType("datetime2");
            entity.Property(order => order.MetadataJson).HasColumnName("MetadataJson");
            entity.HasIndex(order => order.OrderCode).IsUnique().HasDatabaseName("UX_DonThanhToan_MaDonHang");
            entity.HasIndex(order => new { order.IDUser, order.CreatedAt }).HasDatabaseName("IX_DonThanhToan_IDUser_TaoLuc");
            entity.HasIndex(order => new { order.Status, order.CreatedAt }).HasDatabaseName("IX_DonThanhToan_TrangThai_TaoLuc");
            entity.HasOne(order => order.User)
                .WithMany()
                .HasForeignKey(order => order.IDUser)
                .HasConstraintName("FK_DonThanhToan_NguoiDung")
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<PaymentTransaction>(entity =>
        {
            entity.ToTable("GiaoDichThanhToan");
            entity.HasKey(transaction => transaction.IDPaymentTransaction);
            entity.Property(transaction => transaction.IDPaymentTransaction).HasColumnName("IDGiaoDichThanhToan");
            entity.Property(transaction => transaction.IDPaymentOrder).HasColumnName("IDDonThanhToan");
            entity.Property(transaction => transaction.Provider).HasColumnName("NhaCungCap").HasMaxLength(50).IsRequired();
            entity.Property(transaction => transaction.ProviderTransactionId).HasColumnName("MaGiaoDichNhaCungCap").HasMaxLength(100).IsRequired();
            entity.Property(transaction => transaction.Amount).HasColumnName("SoTien").HasColumnType("decimal(18,2)");
            entity.Property(transaction => transaction.Currency).HasColumnName("TienTe").HasMaxLength(3).IsRequired();
            entity.Property(transaction => transaction.Status).HasColumnName("TrangThai").HasMaxLength(20).IsRequired();
            entity.Property(transaction => transaction.TransactionTime).HasColumnName("ThoiGianGiaoDich").HasColumnType("datetime2");
            entity.Property(transaction => transaction.RawReference).HasColumnName("ThamChieuRaw").HasMaxLength(500);
            entity.Property(transaction => transaction.MetadataJson).HasColumnName("MetadataJson");
            entity.Property(transaction => transaction.CreatedAt).HasColumnName("TaoLuc").HasColumnType("datetime2");
            entity.Property(transaction => transaction.ConfirmedAt).HasColumnName("XacNhanLuc").HasColumnType("datetime2");
            entity.Property(transaction => transaction.ConfirmedByAdminId).HasColumnName("IDXacNhanBoiAdmin");
            entity.HasIndex(transaction => new { transaction.Provider, transaction.ProviderTransactionId })
                .IsUnique()
                .HasDatabaseName("UX_GiaoDichThanhToan_NhaCungCap_MaGiaoDich");
            entity.HasIndex(transaction => transaction.IDPaymentOrder)
                .IsUnique()
                .HasFilter("[TrangThai] = N'Confirmed'")
                .HasDatabaseName("UX_GiaoDichThanhToan_ConfirmedOrder");
            entity.HasIndex(transaction => new { transaction.Status, transaction.TransactionTime })
                .HasDatabaseName("IX_GiaoDichThanhToan_TrangThai_ThoiGian");
            entity.HasIndex(transaction => new { transaction.IDPaymentOrder, transaction.CreatedAt })
                .HasDatabaseName("IX_GiaoDichThanhToan_IDDon_TaoLuc");
            entity.HasOne(transaction => transaction.PaymentOrder)
                .WithMany(order => order.Transactions)
                .HasForeignKey(transaction => transaction.IDPaymentOrder)
                .HasConstraintName("FK_GiaoDichThanhToan_DonThanhToan")
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(transaction => transaction.ConfirmedByAdmin)
                .WithMany()
                .HasForeignKey(transaction => transaction.ConfirmedByAdminId)
                .HasConstraintName("FK_GiaoDichThanhToan_Admin")
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<KnowledgeProposal>(entity =>
        {
            entity.ToTable("DeXuatTriThucAI");
            entity.HasKey(proposal => proposal.IDProposal);
            entity.Property(proposal => proposal.IDProposal).HasColumnName("IDDeXuat");
            entity.Property(proposal => proposal.EntityType).HasColumnName("LoaiThucThe").HasMaxLength(20).IsRequired();
            entity.Property(proposal => proposal.Status).HasColumnName("TrangThai").HasMaxLength(30).IsRequired();
            entity.Property(proposal => proposal.CandidateKey).HasColumnName("KhoaUngVien").HasMaxLength(200).IsRequired();
            entity.Property(proposal => proposal.Query).HasColumnName("TuKhoa").HasMaxLength(200).IsRequired();
            entity.Property(proposal => proposal.RequestedSourcesJson).HasColumnName("NguonYeuCauJson");
            entity.Property(proposal => proposal.PayloadJson).HasColumnName("PayloadJson");
            entity.Property(proposal => proposal.SchemaVersion).HasColumnName("PhienBanSchema");
            entity.Property(proposal => proposal.ValidationJson).HasColumnName("KetQuaKiemTraJson");
            entity.Property(proposal => proposal.Revision).HasColumnName("Revision");
            entity.Property(proposal => proposal.Origin).HasColumnName("NguonGoc").HasMaxLength(20).IsRequired();
            entity.Property(proposal => proposal.RequestedBy).HasColumnName("IDNguoiYeuCau");
            entity.Property(proposal => proposal.ReviewedBy).HasColumnName("IDNguoiDuyet");
            entity.Property(proposal => proposal.DecisionReason).HasColumnName("LyDoQuyetDinh").HasMaxLength(1000);
            entity.Property(proposal => proposal.TargetId).HasColumnName("IDBanGhiDich");
            entity.Property(proposal => proposal.CreatedAt).HasColumnName("NgayTao").HasColumnType("datetime2");
            entity.Property(proposal => proposal.UpdatedAt).HasColumnName("NgayCapNhat").HasColumnType("datetime2");
            entity.Property(proposal => proposal.DecidedAt).HasColumnName("NgayQuyetDinh").HasColumnType("datetime2");
            entity.Property(proposal => proposal.RowVersion).HasColumnName("RowVer").IsRowVersion();
            entity.HasMany(proposal => proposal.Sources).WithOne().HasForeignKey(source => source.IDProposal).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(proposal => proposal.Revisions).WithOne().HasForeignKey(revision => revision.IDProposal).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(proposal => proposal.Jobs).WithOne().HasForeignKey(job => job.IDProposal).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<KnowledgeProposalSource>(entity =>
        {
            entity.ToTable("NguonDeXuatTriThucAI");
            entity.HasKey(source => source.IDSource);
            entity.Property(source => source.IDSource).HasColumnName("IDNguon");
            entity.Property(source => source.IDProposal).HasColumnName("IDDeXuat");
            entity.Property(source => source.Organization).HasColumnName("CoQuan").HasMaxLength(100).IsRequired();
            entity.Property(source => source.Url).HasColumnName("Url").HasMaxLength(1000).IsRequired();
            entity.Property(source => source.Identifier).HasColumnName("DinhDanh").HasMaxLength(200);
            entity.Property(source => source.AccessedAt).HasColumnName("NgayTruyCap").HasColumnType("datetime2");
            entity.Property(source => source.Version).HasColumnName("PhienBan").HasMaxLength(100);
            entity.Property(source => source.ContentHash).HasColumnName("HashNoiDung").HasColumnType("char(64)").IsRequired();
            entity.Property(source => source.Snippet).HasColumnName("TrichDoan");
            entity.Property(source => source.FieldsJson).HasColumnName("TruongHoTroJson");
        });

        modelBuilder.Entity<KnowledgeProposalRevision>(entity =>
        {
            entity.ToTable("PhienBanDeXuatTriThucAI");
            entity.HasKey(revision => revision.IDRevision);
            entity.Property(revision => revision.IDRevision).HasColumnName("IDPhienBan");
            entity.Property(revision => revision.IDProposal).HasColumnName("IDDeXuat");
            entity.Property(revision => revision.Revision).HasColumnName("Revision");
            entity.Property(revision => revision.PayloadJson).HasColumnName("PayloadJson").IsRequired();
            entity.Property(revision => revision.Origin).HasColumnName("NguonGoc").HasMaxLength(20).IsRequired();
            entity.Property(revision => revision.EditedBy).HasColumnName("IDNguoiSua");
            entity.Property(revision => revision.CreatedAt).HasColumnName("NgayTao").HasColumnType("datetime2");
        });

        modelBuilder.Entity<KnowledgeProposalJob>(entity =>
        {
            entity.ToTable("CongViecDeXuatTriThucAI");
            entity.HasKey(job => job.IDJob);
            entity.Property(job => job.IDJob).HasColumnName("IDCongViec");
            entity.Property(job => job.IDProposal).HasColumnName("IDDeXuat");
            entity.Property(job => job.Status).HasColumnName("TrangThai").HasMaxLength(20).IsRequired();
            entity.Property(job => job.Attempts).HasColumnName("SoLanThu");
            entity.Property(job => job.LeaseUntil).HasColumnName("GiuDen").HasColumnType("datetime2");
            entity.Property(job => job.NotBefore).HasColumnName("ChayLaiSau").HasColumnType("datetime2");
            entity.Property(job => job.Error).HasColumnName("Loi").HasMaxLength(1000);
            entity.Property(job => job.Provider).HasColumnName("Provider").HasMaxLength(50);
            entity.Property(job => job.Model).HasColumnName("Model").HasMaxLength(100);
            entity.Property(job => job.PromptVersion).HasColumnName("PhienBanPrompt").HasMaxLength(20);
            entity.Property(job => job.DurationMs).HasColumnName("ThoiLuongMs");
            entity.Property(job => job.InputTokens).HasColumnName("TokenVao");
            entity.Property(job => job.OutputTokens).HasColumnName("TokenRa");
            entity.Property(job => job.CreatedAt).HasColumnName("NgayTao").HasColumnType("datetime2");
            entity.Property(job => job.StartedAt).HasColumnName("BatDauLuc").HasColumnType("datetime2");
            entity.Property(job => job.FinishedAt).HasColumnName("KetThucLuc").HasColumnType("datetime2");
        });

        modelBuilder.Entity<AdminAuditLog>(entity =>
        {
            entity.ToTable("NhatKyQuanTri");
            entity.HasKey(log => log.IDAuditLog);
            entity.Property(log => log.IDAuditLog).HasColumnName("IDAuditLog");
            entity.Property(log => log.IDAdmin).HasColumnName("IDAdmin");
            entity.Property(log => log.Action).HasColumnName("HanhDong").HasMaxLength(50).IsRequired();
            entity.Property(log => log.TargetType).HasColumnName("LoaiDoiTuong").HasMaxLength(50).IsRequired();
            entity.Property(log => log.TargetId).HasColumnName("IDDoiTuong");
            entity.Property(log => log.Detail).HasColumnName("ChiTiet").HasMaxLength(500);
            entity.Property(log => log.CreatedAt)
                .HasColumnName("NgayTao")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasIndex(log => new { log.CreatedAt, log.Action });
        });

        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.ToTable("PasswordResetTokens");
            entity.HasKey(resetToken => resetToken.IDReset);
            entity.Property(resetToken => resetToken.IDReset).HasColumnName("IDReset");
            entity.Property(resetToken => resetToken.IDUser).HasColumnName("IDUser");
            entity.Property(resetToken => resetToken.Token)
                .HasColumnName("Token")
                .HasMaxLength(64)
                .IsRequired();
            entity.Property(resetToken => resetToken.HetHan)
                .HasColumnName("HetHan")
                .HasColumnType("datetime2");
            entity.Property(resetToken => resetToken.DaSuDung)
                .HasColumnName("DaSuDung")
                .HasDefaultValue(false);
            entity.Property(resetToken => resetToken.FailedAttempts).HasDefaultValue(0);
            entity.Property(resetToken => resetToken.NgayTao)
                .HasColumnName("NgayTao")
                .HasColumnType("datetime2");
            entity.Property(resetToken => resetToken.IpAddress)
                .HasColumnName("IpAddress")
                .HasMaxLength(45);
            entity.Property(resetToken => resetToken.UserAgent)
                .HasColumnName("UserAgent")
                .HasMaxLength(512);

            entity.HasIndex(resetToken => resetToken.Token).IsUnique();
            entity.HasIndex(resetToken => resetToken.IDUser);
            entity.HasIndex(resetToken => resetToken.NgayTao);
            entity.HasOne(resetToken => resetToken.User)
                .WithMany(user => user.PasswordResetTokens)
                .HasForeignKey(resetToken => resetToken.IDUser)
                .HasConstraintName("FK_PasswordResetTokens_NguoiDung")
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AuthSession>(entity =>
        {
            entity.ToTable("AuthSessions", table =>
            {
                table.HasCheckConstraint(
                    "CK_AuthSessions_AuthMethod",
                    "AuthMethod COLLATE Latin1_General_100_BIN2 IN (N'Password', N'Google')");
                table.HasCheckConstraint(
                    "CK_AuthSessions_ExpiresAt",
                    "ExpiresAt > CreatedAt");
            });
            entity.HasKey(session => session.IDAuthSession);
            entity.Property(session => session.IDAuthSession).HasColumnName("IDAuthSession");
            entity.Property(session => session.IDUser).HasColumnName("IDUser");
            entity.Property(session => session.SessionSecretHash)
                .HasColumnName("SessionSecretHash")
                .HasColumnType("binary(32)")
                .IsRequired();
            entity.Property(session => session.AuthMethod)
                .HasColumnName("AuthMethod")
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();
            entity.Property(session => session.CreatedAt)
                .HasColumnName("CreatedAt")
                .HasColumnType("datetime2(7)");
            entity.Property(session => session.ExpiresAt)
                .HasColumnName("ExpiresAt")
                .HasColumnType("datetime2(7)");
            entity.Property(session => session.RevokedAt)
                .HasColumnName("RevokedAt")
                .HasColumnType("datetime2(7)");
            entity.Property(session => session.RevokeReason)
                .HasColumnName("RevokeReason")
                .HasMaxLength(AuthSessionService.MaxRevokeReasonLength);
            entity.HasIndex(session => session.SessionSecretHash)
                .IsUnique()
                .HasDatabaseName("UX_AuthSessions_SessionSecretHash");
            entity.HasIndex(session => new { session.IDUser, session.RevokedAt, session.ExpiresAt })
                .HasDatabaseName("IX_AuthSessions_IDUser_RevokedAt_ExpiresAt");
            entity.HasOne(session => session.User)
                .WithMany(user => user.AuthSessions)
                .HasForeignKey(session => session.IDUser)
                .HasConstraintName("FK_AuthSessions_NguoiDung")
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Chemical>(entity =>
        {
            entity.ToTable("HoaChat");
            entity.HasKey(chemical => chemical.IDChemical);
            entity.Property(chemical => chemical.IDChemical).HasColumnName("IDHoaChat");
            entity.Property(chemical => chemical.Name)
                .HasColumnName("TenChat")
                .HasMaxLength(100)
                .IsRequired();
            entity.Property(chemical => chemical.Formula)
                .HasColumnName("CongThuc")
                .HasMaxLength(50)
                .IsRequired();
            entity.Property(chemical => chemical.Type)
                .HasColumnName("LoaiChat")
                .HasMaxLength(50);
            entity.Property(chemical => chemical.MolarMass)
                .HasColumnName("KhoiLuongMol")
                .HasColumnType("numeric(10,4)");
            entity.Property(chemical => chemical.State)
                .HasColumnName("TrangThai")
                .HasMaxLength(20);
            entity.Property(chemical => chemical.Color)
                .HasColumnName("MauSac")
                .HasMaxLength(100);
            entity.Property(chemical => chemical.Properties).HasColumnName("TinhChat");
            entity.Property(chemical => chemical.Applications).HasColumnName("UngDung");
            entity.Property(chemical => chemical.Description).HasColumnName("MoTa");
            entity.Property(chemical => chemical.SandboxCategory)
                .HasColumnName("DanhMucSandbox")
                .HasMaxLength(50);
            entity.Property(chemical => chemical.AlwaysVisibleInSandbox)
                .HasColumnName("LuonHienThiSandbox")
                .HasDefaultValue(false);
            entity.Property(chemical => chemical.IsSandboxStarter)
                .HasColumnName("LaChatKhoiDau")
                .HasDefaultValue(false);
            entity.Property(chemical => chemical.CanDrag)
                .HasColumnName("CoTheKeoTha")
                .HasDefaultValue(true);
            entity.Property(chemical => chemical.DefaultPh)
                .HasColumnName("DoPHMacDinh")
                .HasColumnType("numeric(4,2)");
            entity.Property(chemical => chemical.DefaultConcentration)
                .HasColumnName("NongDoMacDinh")
                .HasColumnType("numeric(8,4)");
            entity.Property(chemical => chemical.RuleCation)
                .HasColumnName("QuyTac_Cation")
                .HasMaxLength(20);
            entity.Property(chemical => chemical.RuleCationCharge)
                .HasColumnName("QuyTac_DienTichCation");
            entity.Property(chemical => chemical.RuleAnion)
                .HasColumnName("QuyTac_Anion")
                .HasMaxLength(20);
            entity.Property(chemical => chemical.RuleAnionCharge)
                .HasColumnName("QuyTac_DienTichAnion");
            entity.Property(chemical => chemical.AcidHydrogenCount)
                .HasColumnName("QuyTac_SoHAcid");
            entity.Property(chemical => chemical.BaseHydroxideCount)
                .HasColumnName("QuyTac_SoOHBazo");
            entity.Property(chemical => chemical.MetalActivityRank)
                .HasColumnName("QuyTac_DoHoatDongKimLoai");
            entity.Property(chemical => chemical.Solubility)
                .HasColumnName("QuyTac_DoTan")
                .HasMaxLength(30);
            entity.Property(chemical => chemical.SandboxPhenomenon)
                .HasColumnName("QuyTac_HienTuong");
            entity.Property(chemical => chemical.AllowRuleEngine)
                .HasColumnName("QuyTac_ChoPhep")
                .HasDefaultValue(true);
            entity.HasIndex(chemical => chemical.Formula).IsUnique();
        });

        modelBuilder.Entity<Reaction>(entity =>
        {
            entity.ToTable("PhanUng");
            entity.HasKey(reaction => reaction.IDReaction);
            entity.Property(reaction => reaction.IDReaction).HasColumnName("IDPhanUng");
            entity.Property(reaction => reaction.Equation)
                .HasColumnName("PhuongTrinh")
                .HasMaxLength(500)
                .IsRequired();
            entity.Property(reaction => reaction.Phenomenon).HasColumnName("HienTuong");
            entity.Property(reaction => reaction.Explanation).HasColumnName("GiaiThich");
            entity.Property(reaction => reaction.HasPrecipitate).HasColumnName("CoKetTua");
            entity.Property(reaction => reaction.HasGas).HasColumnName("CoKhi");
            entity.Property(reaction => reaction.HasColorChange).HasColumnName("DoiMau");
            entity.Property(reaction => reaction.IsExothermic).HasColumnName("ToaNhiet");
            entity.Property(reaction => reaction.Note).HasColumnName("GhiChu");
            entity.Property(reaction => reaction.RuleCode)
                .HasColumnName("MaQuyTac")
                .HasMaxLength(50);
            entity.Property(reaction => reaction.IsSandboxOverride)
                .HasColumnName("LaNgoaiLeSandbox")
                .HasDefaultValue(false);
            entity.Property(reaction => reaction.SandboxPriority)
                .HasColumnName("DoUuTienSandbox")
                .HasDefaultValue(50);
            entity.HasIndex(reaction => reaction.Equation).IsUnique();
        });

        modelBuilder.Entity<ReactionRule>(entity =>
        {
            entity.ToTable("QuyLuatPhanUng");
            entity.HasKey(rule => rule.IDRule);
            entity.Property(rule => rule.IDRule).HasColumnName("IDQuyLuat");
            entity.Property(rule => rule.Code).HasColumnName("MaQuyLuat").HasMaxLength(80).IsRequired();
            entity.Property(rule => rule.Name).HasColumnName("TenQuyLuat").HasMaxLength(150).IsRequired();
            entity.Property(rule => rule.Description).HasColumnName("MoTa");
            entity.Property(rule => rule.IsActive).HasColumnName("DangHoatDong").HasDefaultValue(true);
            entity.Property(rule => rule.Priority).HasColumnName("DoUuTien").HasDefaultValue(100);
            entity.Property(rule => rule.RequiresHeat).HasColumnName("CanNhiet");
            entity.Property(rule => rule.OutcomeHasGas).HasColumnName("KetQua_CoKhi");
            entity.Property(rule => rule.OutcomeHasPrecipitate).HasColumnName("KetQua_CoKetTua");
            entity.Property(rule => rule.OutcomeHasColorChange).HasColumnName("KetQua_DoiMau");
            entity.Property(rule => rule.OutcomeIsExothermic).HasColumnName("KetQua_ToaNhiet");
            entity.Property(rule => rule.OutcomeTemperatureDeltaC).HasColumnName("KetQua_NhietDoTangC").HasColumnType("decimal(8,2)");
            entity.Property(rule => rule.PhenomenonTemplate).HasColumnName("HienTuongTemplate");
            entity.Property(rule => rule.ExplanationTemplate).HasColumnName("GiaiThichTemplate");
            entity.Property(rule => rule.CreatedAt).HasColumnName("NgayTao").HasColumnType("datetime2").HasDefaultValueSql("SYSUTCDATETIME()");
            entity.Property(rule => rule.UpdatedAt).HasColumnName("NgayCapNhat").HasColumnType("datetime2").HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasIndex(rule => rule.Code).IsUnique();
        });

        modelBuilder.Entity<ReactionRuleSlot>(entity =>
        {
            entity.ToTable("QuyLuatPhanUngSlot");
            entity.HasKey(slot => slot.IDSlot);
            entity.Property(slot => slot.IDSlot).HasColumnName("IDSlot");
            entity.Property(slot => slot.IDRule).HasColumnName("IDQuyLuat");
            entity.Property(slot => slot.SlotKey).HasColumnName("KyHieuSlot").HasMaxLength(20).IsRequired();
            entity.Property(slot => slot.Role).HasColumnName("VaiTro").HasMaxLength(20).IsRequired();
            entity.Property(slot => slot.Order).HasColumnName("ThuTu");
            entity.HasIndex(slot => new { slot.IDRule, slot.SlotKey }).IsUnique();
            entity.HasOne(slot => slot.Rule)
                .WithMany(rule => rule.Slots)
                .HasForeignKey(slot => slot.IDRule)
                .HasConstraintName("FK_QuyLuatPhanUngSlot_QuyLuatPhanUng")
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ReactionRuleCondition>(entity =>
        {
            entity.ToTable("QuyLuatPhanUngDieuKien");
            entity.HasKey(condition => condition.IDCondition);
            entity.Property(condition => condition.IDCondition).HasColumnName("IDDieuKien");
            entity.Property(condition => condition.IDRule).HasColumnName("IDQuyLuat");
            entity.Property(condition => condition.TargetKind).HasColumnName("TargetKind").HasMaxLength(30).IsRequired();
            entity.Property(condition => condition.TargetSlot).HasColumnName("TargetSlot").HasMaxLength(20);
            entity.Property(condition => condition.TargetProduct).HasColumnName("TargetProduct").HasMaxLength(20);
            entity.Property(condition => condition.Field).HasColumnName("Field").HasMaxLength(80).IsRequired();
            entity.Property(condition => condition.Operator).HasColumnName("Operator").HasMaxLength(30).IsRequired();
            entity.Property(condition => condition.ValueKind).HasColumnName("ValueKind").HasMaxLength(30).IsRequired();
            entity.Property(condition => condition.ValueText).HasColumnName("ValueText").HasMaxLength(200);
            entity.Property(condition => condition.CompareSlot).HasColumnName("CompareSlot").HasMaxLength(20);
            entity.Property(condition => condition.CompareField).HasColumnName("CompareField").HasMaxLength(80);
            entity.Property(condition => condition.GroupKey).HasColumnName("GroupKey").HasMaxLength(40);
            entity.Property(condition => condition.GroupOperator).HasColumnName("GroupOperator").HasMaxLength(10);
            entity.HasOne(condition => condition.Rule)
                .WithMany(rule => rule.Conditions)
                .HasForeignKey(condition => condition.IDRule)
                .HasConstraintName("FK_QuyLuatPhanUngDieuKien_QuyLuatPhanUng")
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ReactionRuleProduct>(entity =>
        {
            entity.ToTable("QuyLuatPhanUngSanPham");
            entity.HasKey(product => product.IDProduct);
            entity.Property(product => product.IDProduct).HasColumnName("IDSanPham");
            entity.Property(product => product.IDRule).HasColumnName("IDQuyLuat");
            entity.Property(product => product.Order).HasColumnName("ThuTu");
            entity.Property(product => product.ProductType).HasColumnName("KieuSanPham").HasMaxLength(40).IsRequired();
            entity.Property(product => product.FixedFormula).HasColumnName("FixedFormula").HasMaxLength(80);
            entity.Property(product => product.CationSourceSlot).HasColumnName("CationSourceSlot").HasMaxLength(20);
            entity.Property(product => product.AnionSourceSlot).HasColumnName("AnionSourceSlot").HasMaxLength(20);
            entity.Property(product => product.SourceField).HasColumnName("SourceField").HasMaxLength(80);
            entity.Property(product => product.CoefficientHint).HasColumnName("HeSoHint");
            entity.HasOne(product => product.Rule)
                .WithMany(rule => rule.Products)
                .HasForeignKey(product => product.IDRule)
                .HasConstraintName("FK_QuyLuatPhanUngSanPham_QuyLuatPhanUng")
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ReactionRuleOutcome>(entity =>
        {
            entity.ToTable("QuyLuatPhanUngKetQuaDieuKien");
            entity.HasKey(outcome => outcome.IDOutcome);
            entity.Property(outcome => outcome.IDOutcome).HasColumnName("IDKetQua");
            entity.Property(outcome => outcome.IDRule).HasColumnName("IDQuyLuat");
            entity.Property(outcome => outcome.Order).HasColumnName("ThuTu");
            entity.Property(outcome => outcome.OutputField).HasColumnName("OutputField").HasMaxLength(40).IsRequired();
            entity.Property(outcome => outcome.BoolValue).HasColumnName("BoolValue");
            entity.Property(outcome => outcome.DecimalValue).HasColumnName("DecimalValue").HasColumnType("decimal(8,2)");
            entity.Property(outcome => outcome.TextValue).HasColumnName("TextValue");
            entity.Property(outcome => outcome.TargetKind).HasColumnName("TargetKind").HasMaxLength(30).IsRequired();
            entity.Property(outcome => outcome.TargetSlot).HasColumnName("TargetSlot").HasMaxLength(20);
            entity.Property(outcome => outcome.TargetProduct).HasColumnName("TargetProduct").HasMaxLength(20);
            entity.Property(outcome => outcome.Field).HasColumnName("Field").HasMaxLength(80).IsRequired();
            entity.Property(outcome => outcome.Operator).HasColumnName("Operator").HasMaxLength(30).IsRequired();
            entity.Property(outcome => outcome.ValueKind).HasColumnName("ValueKind").HasMaxLength(30).IsRequired();
            entity.Property(outcome => outcome.ValueText).HasColumnName("ValueText").HasMaxLength(200);
            entity.Property(outcome => outcome.CompareSlot).HasColumnName("CompareSlot").HasMaxLength(20);
            entity.Property(outcome => outcome.CompareField).HasColumnName("CompareField").HasMaxLength(80);
            entity.HasOne(outcome => outcome.Rule)
                .WithMany(rule => rule.Outcomes)
                .HasForeignKey(outcome => outcome.IDRule)
                .HasConstraintName("FK_QuyLuatPhanUngKetQuaDieuKien_QuyLuatPhanUng")
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WorkshopChallenge>(entity =>
        {
            entity.ToTable("ThuThachXuong");
            entity.HasKey(challenge => challenge.IDChallenge);
            entity.Property(challenge => challenge.IDChallenge).HasColumnName("IDThuThach");
            entity.Property(challenge => challenge.Name).HasColumnName("TenThuThach").HasMaxLength(150).IsRequired();
            entity.Property(challenge => challenge.Description).HasColumnName("MoTa");
            entity.Property(challenge => challenge.IsActive).HasColumnName("DangHoatDong").HasDefaultValue(true);
            entity.Property(challenge => challenge.Difficulty).HasColumnName("DoKho").HasMaxLength(30).IsRequired();
            entity.Property(challenge => challenge.ChallengeDate).HasColumnName("NgayThuThach").HasColumnType("date");
            entity.Property(challenge => challenge.DailySlot).HasColumnName("SlotThuThach");
            entity.Property(challenge => challenge.Seed).HasColumnName("Seed").HasMaxLength(120);
            entity.Property(challenge => challenge.GeneratorVersion).HasColumnName("PhienBanGenerator").HasMaxLength(40);
            entity.Property(challenge => challenge.MetadataJson).HasColumnName("MetadataJson");
            entity.Property(challenge => challenge.Source).HasColumnName("NguonTao").HasMaxLength(40).HasDefaultValue("AdminLegacy");
            entity.Property(challenge => challenge.TimeLimitSeconds).HasColumnName("ThoiGianGioiHanGiay");
            entity.Property(challenge => challenge.TargetChemicalId).HasColumnName("IDHoaChatMucTieu");
            entity.Property(challenge => challenge.TargetAmount).HasColumnName("LuongMucTieu").HasColumnType("decimal(18,4)");
            entity.Property(challenge => challenge.TargetUnit).HasColumnName("DonViMucTieu").HasMaxLength(20).IsRequired();
            entity.Property(challenge => challenge.BaseScore).HasColumnName("DiemCoBan");
            entity.Property(challenge => challenge.EarlyBonusScore).HasColumnName("DiemThuongSom");
            entity.Property(challenge => challenge.EarlyBonusThresholdSeconds).HasColumnName("NguongThuongSomGiay");
            entity.Property(challenge => challenge.LatePenaltyScore).HasColumnName("DiemPhatTre");
            entity.Property(challenge => challenge.CreatedAt).HasColumnName("NgayTao").HasColumnType("datetime2");
            entity.Property(challenge => challenge.UpdatedAt).HasColumnName("NgayCapNhat").HasColumnType("datetime2");
            entity.HasIndex(challenge => new { challenge.IsActive, challenge.Difficulty }).HasDatabaseName("IX_ThuThachXuong_DangHoatDong_DoKho");
            entity.HasIndex(challenge => challenge.TargetChemicalId)
                .HasDatabaseName("IX_ThuThachXuong_IDHoaChatMucTieu");
            entity.HasIndex(challenge => new { challenge.ChallengeDate, challenge.Difficulty, challenge.DailySlot })
                .IsUnique()
                .HasFilter("[NgayThuThach] IS NOT NULL AND [SlotThuThach] IS NOT NULL")
                .HasDatabaseName("UX_ThuThachXuong_Ngay_DoKho_Slot");
            entity.HasOne(challenge => challenge.TargetChemical)
                .WithMany()
                .HasForeignKey(challenge => challenge.TargetChemicalId)
                .HasConstraintName("FK_ThuThachXuong_HoaChatMucTieu")
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<WorkshopChallengeResource>(entity =>
        {
            entity.ToTable("ThuThachXuongTaiNguyen");
            entity.HasKey(resource => resource.IDResource);
            entity.Property(resource => resource.IDResource).HasColumnName("IDTaiNguyen");
            entity.Property(resource => resource.IDChallenge).HasColumnName("IDThuThach");
            entity.Property(resource => resource.ChemicalId).HasColumnName("IDHoaChat");
            entity.Property(resource => resource.Amount).HasColumnName("Luong").HasColumnType("decimal(18,4)");
            entity.Property(resource => resource.Unit).HasColumnName("DonVi").HasMaxLength(20).IsRequired();
            entity.Property(resource => resource.Order).HasColumnName("ThuTu");
            entity.HasIndex(resource => new { resource.IDChallenge, resource.ChemicalId }).IsUnique().HasDatabaseName("UX_ThuThachXuongTaiNguyen_ThuThach_HoaChat");
            entity.HasIndex(resource => resource.ChemicalId)
                .HasDatabaseName("IX_ThuThachXuongTaiNguyen_IDHoaChat");
            entity.HasOne(resource => resource.Challenge)
                .WithMany(challenge => challenge.Resources)
                .HasForeignKey(resource => resource.IDChallenge)
                .HasConstraintName("FK_ThuThachXuongTaiNguyen_ThuThachXuong")
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(resource => resource.Chemical)
                .WithMany()
                .HasForeignKey(resource => resource.ChemicalId)
                .HasConstraintName("FK_ThuThachXuongTaiNguyen_HoaChat")
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<WorkshopChallengeSession>(entity =>
        {
            entity.ToTable("PhienThuThachXuong");
            entity.HasKey(session => session.IDSession);
            entity.Property(session => session.IDSession).HasColumnName("IDPhien");
            entity.Property(session => session.IDChallenge).HasColumnName("IDThuThach");
            entity.Property(session => session.IDUser).HasColumnName("IDUser");
            entity.Property(session => session.Status).HasColumnName("TrangThai").HasMaxLength(30).IsRequired().IsConcurrencyToken();
            entity.Property(session => session.StartedAt).HasColumnName("BatDauLuc").HasColumnType("datetime2");
            entity.Property(session => session.ExpiresAt).HasColumnName("HetHanLuc").HasColumnType("datetime2");
            entity.Property(session => session.CompletedAt).HasColumnName("HoanThanhLuc").HasColumnType("datetime2");
            entity.Property(session => session.InventoryJson).HasColumnName("InventoryJson").IsRequired().IsConcurrencyToken();
            entity.Property(session => session.TargetAmountAchieved).HasColumnName("LuongMucTieuDatDuoc").HasColumnType("decimal(18,4)");
            entity.Property(session => session.TargetAchieved).HasColumnName("DatMucTieu").HasDefaultValue(false);
            entity.Property(session => session.ReactionCount).HasColumnName("SoPhanUng").HasDefaultValue(0);
            entity.Property(session => session.MistakeCount).HasColumnName("SoLoi").HasDefaultValue(0);
            entity.Property(session => session.SuspicionStatus).HasColumnName("TrangThaiNghiVan").HasMaxLength(30).HasDefaultValue("Normal");
            entity.Property(session => session.EventHistoryJson).HasColumnName("LichSuSuKienJson").IsRequired().HasDefaultValue("[]");
            entity.Property(session => session.BaseScoreAwarded).HasColumnName("DiemCoBan");
            entity.Property(session => session.EarlyBonusAwarded).HasColumnName("DiemThuong");
            entity.Property(session => session.LatePenaltyApplied).HasColumnName("DiemPhat");
            entity.Property(session => session.FinalScore).HasColumnName("DiemCuoi");
            entity.Property(session => session.DurationSeconds).HasColumnName("ThoiLuongGiay");
            entity.Property(session => session.FailureReason).HasColumnName("LyDoKetThuc").HasMaxLength(200);
            entity.HasIndex(session => new { session.IDUser, session.IDChallenge }).IsUnique().HasDatabaseName("UX_PhienThuThachXuong_User_Challenge");
            entity.HasIndex(session => new { session.IDUser, session.IDChallenge, session.Status }).HasDatabaseName("IX_PhienThuThachXuong_User_Challenge_Status");
            entity.HasIndex(session => new { session.IDChallenge, session.Status, session.FinalScore }).HasDatabaseName("IX_PhienThuThachXuong_Leaderboard");
            entity.HasOne(session => session.Challenge)
                .WithMany(challenge => challenge.Sessions)
                .HasForeignKey(session => session.IDChallenge)
                .HasConstraintName("FK_PhienThuThachXuong_ThuThachXuong")
                .OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(session => session.User)
                .WithMany()
                .HasForeignKey(session => session.IDUser)
                .HasConstraintName("FK_PhienThuThachXuong_NguoiDung")
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LearningNote>(entity =>
        {
            entity.ToTable("GhiChu");
            entity.HasKey(note => note.IDNote);
            entity.Property(note => note.IDNote).HasColumnName("IDGhiChu");
            entity.Property(note => note.IDUser).HasColumnName("IDUser");
            entity.Property(note => note.IDLesson).HasColumnName("IDBaiHoc");
            entity.Property(note => note.Title)
                .HasColumnName("TieuDe")
                .HasMaxLength(150);
            entity.Property(note => note.Content)
                .HasColumnName("NoiDung")
                .HasDefaultValue(string.Empty);
            entity.Property(note => note.CreatedAt)
                .HasColumnName("NgayTao")
                .HasColumnType("datetime2");
            entity.Property(note => note.UpdatedAt)
                .HasColumnName("NgayCapNhat")
                .HasColumnType("datetime2");
            entity.HasIndex(note => new { note.IDUser, note.UpdatedAt });
            entity.HasOne(note => note.User)
                .WithMany()
                .HasForeignKey(note => note.IDUser)
                .HasConstraintName("FK_GhiChu_NguoiDung")
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Chapter>(entity =>
        {
            entity.ToTable("ChuongHoc");
            entity.HasKey(chapter => chapter.IDChapter);
            entity.Property(chapter => chapter.IDChapter).HasColumnName("IDChuong");
            entity.Property(chapter => chapter.Title).HasColumnName("TenChuong").HasMaxLength(150).IsRequired();
            entity.Property(chapter => chapter.Description).HasColumnName("MoTa");
            entity.Property(chapter => chapter.Order).HasColumnName("ThuTu");
            entity.HasIndex(chapter => chapter.Order).IsUnique();
        });

        modelBuilder.Entity<Lesson>(entity =>
        {
            entity.ToTable("BaiHoc");
            entity.HasKey(lesson => lesson.IDLesson);
            entity.Property(lesson => lesson.IDLesson).HasColumnName("IDBaiHoc");
            entity.Property(lesson => lesson.IDChapter).HasColumnName("IDChuong");
            entity.Property(lesson => lesson.Title).HasColumnName("TenBaiHoc").HasMaxLength(150).IsRequired();
            entity.Property(lesson => lesson.Content).HasColumnName("NoiDung");
            entity.Property(lesson => lesson.ReactionIllustrationsJson).HasColumnName("MinhHoaPhanUngJson");
            entity.Property(lesson => lesson.Order).HasColumnName("ThuTu");
            entity.HasIndex(lesson => new { lesson.IDChapter, lesson.Order }).IsUnique();
            entity.HasOne(lesson => lesson.Chapter)
                .WithMany(chapter => chapter.Lessons)
                .HasForeignKey(lesson => lesson.IDChapter)
                .HasConstraintName("FK_BaiHoc_ChuongHoc")
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LessonProgress>(entity =>
        {
            entity.ToTable("TienDoHocTap");
            entity.HasKey(progress => new { progress.IDUser, progress.IDLesson });
            entity.Property(progress => progress.IDUser).HasColumnName("IDUser");
            entity.Property(progress => progress.IDLesson).HasColumnName("IDBaiHoc");
            entity.Property(progress => progress.IsCompleted)
                .HasColumnName("DaHoanThanh")
                .HasDefaultValue(false);
            entity.Property(progress => progress.LastStudiedAt)
                .HasColumnName("LanHocGanNhat")
                .HasColumnType("datetime2");
            entity.Property(progress => progress.CompletionPercent)
                .HasColumnName("PhanTramHoanThanh")
                .HasDefaultValue((byte)0);
            entity.HasOne(progress => progress.User)
                .WithMany()
                .HasForeignKey(progress => progress.IDUser)
                .HasConstraintName("FK_TienDoHocTap_NguoiDung")
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(progress => progress.Lesson)
                .WithMany()
                .HasForeignKey(progress => progress.IDLesson)
                .HasConstraintName("FK_TienDoHocTap_BaiHoc")
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<LearningProfile>(entity =>
        {
            entity.ToTable("HoSoHocTap");
            entity.HasKey(profile => profile.IDUser);
            entity.Property(profile => profile.IDUser).HasColumnName("IDUser");
            entity.Property(profile => profile.CurrentStreak)
                .HasColumnName("ChuoiHienTai")
                .HasDefaultValue(0);
            entity.Property(profile => profile.BestStreak)
                .HasColumnName("ChuoiCaoNhat")
                .HasDefaultValue(0);
            entity.Property(profile => profile.LastDailyReviewDate).HasColumnName("NgayHoanThanhOnTapCuoi");
            entity.HasOne(profile => profile.User)
                .WithOne()
                .HasForeignKey<LearningProfile>(profile => profile.IDUser)
                .HasConstraintName("FK_HoSoHocTap_NguoiDung")
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ExerciseSet>(entity =>
        {
            entity.ToTable("DeBaiTap");
            entity.HasKey(set => set.IDExerciseSet);
            entity.Property(set => set.IDExerciseSet).HasColumnName("IDDeBaiTap");
            entity.Property(set => set.IDLesson).HasColumnName("IDBaiHoc");
            entity.Property(set => set.Title).HasColumnName("TieuDe").HasMaxLength(200).IsRequired();
            entity.Property(set => set.Description).HasColumnName("MoTa");
            entity.Property(set => set.Order).HasColumnName("ThuTu").HasDefaultValue(1);
            entity.Property(set => set.IsActive)
                .HasColumnName("DangHoatDong")
                .HasDefaultValue(true);
            entity.Property(set => set.UpdatedAt)
                .HasColumnName("NgayCapNhat")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasIndex(set => new { set.IDLesson, set.Order }).IsUnique().HasFilter("[DangHoatDong] = 1");
            entity.HasOne(set => set.Lesson)
                .WithMany()
                .HasForeignKey(set => set.IDLesson)
                .HasConstraintName("FK_DeBaiTap_BaiHoc")
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ExerciseSetQuestion>(entity =>
        {
            entity.ToTable("ChiTietDeBaiTap");
            entity.HasKey(item => new { item.IDExerciseSet, item.IDExercise });
            entity.Property(item => item.IDExerciseSet).HasColumnName("IDDeBaiTap");
            entity.Property(item => item.IDExercise).HasColumnName("IDBaiTap");
            entity.Property(item => item.Order).HasColumnName("ThuTu");
            entity.Property(item => item.MaxScore)
                .HasColumnName("DiemToiDa")
                .HasColumnType("decimal(5,2)")
                .HasDefaultValue(1);
            entity.HasIndex(item => new { item.IDExerciseSet, item.Order }).IsUnique();
            entity.HasIndex(item => item.IDExercise)
                .HasDatabaseName("IX_ChiTietDeBaiTap_IDBaiTap");
            entity.HasOne(item => item.ExerciseSet)
                .WithMany(set => set.Questions)
                .HasForeignKey(item => item.IDExerciseSet)
                .HasConstraintName("FK_ChiTietDeBaiTap_DeBaiTap")
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(item => item.Exercise)
                .WithMany(exercise => exercise.ExerciseSetQuestions)
                .HasForeignKey(item => item.IDExercise)
                .HasConstraintName("FK_ChiTietDeBaiTap_BaiTap")
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Exercise>(entity =>
        {
            entity.ToTable("BaiTap");
            entity.HasKey(exercise => exercise.IDExercise);
            entity.Property(exercise => exercise.IDExercise).HasColumnName("IDBaiTap");
            entity.Property(exercise => exercise.IDLesson).HasColumnName("IDBaiHoc");
            entity.Property(exercise => exercise.Type).HasColumnName("LoaiBaiTap").HasMaxLength(50).IsRequired();
            entity.Property(exercise => exercise.Question).HasColumnName("CauHoi").IsRequired();
            entity.Property(exercise => exercise.Answer).HasColumnName("DapAn");
            entity.Property(exercise => exercise.Solution).HasColumnName("LoiGiai");
            entity.Property(exercise => exercise.ReviewHint).HasColumnName("GoiYHocLai");
            entity.Property(exercise => exercise.AnswerUnit).HasColumnName("DonViDapAn").HasMaxLength(30);
            entity.Property(exercise => exercise.AllowedError)
                .HasColumnName("SaiSoChoPhep")
                .HasColumnType("decimal(12,4)");
            entity.Property(exercise => exercise.IsActive)
                .HasColumnName("DangHoatDong")
                .HasDefaultValue(true);
            entity.Property(exercise => exercise.UpdatedAt)
                .HasColumnName("NgayCapNhat")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasIndex(exercise => exercise.IDLesson);
            entity.HasOne(exercise => exercise.Lesson)
                .WithMany()
                .HasForeignKey(exercise => exercise.IDLesson)
                .HasConstraintName("FK_BaiTap_BaiHoc")
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<ExerciseChoice>(entity =>
        {
            entity.ToTable("LuaChon");
            entity.HasKey(choice => choice.IDChoice);
            entity.Property(choice => choice.IDChoice).HasColumnName("IDLuaChon");
            entity.Property(choice => choice.IDExercise).HasColumnName("IDBaiTap");
            entity.Property(choice => choice.Content).HasColumnName("NoiDung").IsRequired();
            entity.Property(choice => choice.IsAnswer)
                .HasColumnName("LaDapAn")
                .HasDefaultValue(false);
            entity.Property(choice => choice.Order).HasColumnName("ThuTu");
            entity.HasIndex(choice => new { choice.IDExercise, choice.Order }).IsUnique();
            entity.HasOne(choice => choice.Exercise)
                .WithMany(exercise => exercise.Choices)
                .HasForeignKey(choice => choice.IDExercise)
                .HasConstraintName("FK_LuaChon_BaiTap")
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Exam>(entity =>
        {
            entity.ToTable("BaiKiemTra");
            entity.HasKey(exam => exam.IDExam);
            entity.Property(exam => exam.IDExam).HasColumnName("IDBaiKiemTra");
            entity.Property(exam => exam.IDChapter).HasColumnName("IDChuong");
            entity.Property(exam => exam.IDLesson).HasColumnName("IDBaiHoc");
            entity.Property(exam => exam.Title).HasColumnName("TenBaiKiemTra").HasMaxLength(150).IsRequired();
            entity.Property(exam => exam.Description).HasColumnName("MoTa");
            entity.Property(exam => exam.DurationMinutes)
                .HasColumnName("ThoiLuongPhut")
                .HasDefaultValue(30);
            entity.Property(exam => exam.IsActive)
                .HasColumnName("DangHoatDong")
                .HasDefaultValue(true);
            entity.Property(exam => exam.CreatedAt)
                .HasColumnName("NgayTao")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("SYSUTCDATETIME()");
            entity.Property(exam => exam.QuestionSelectionMode)
                .HasColumnName("KieuChonCauHoi")
                .HasMaxLength(20)
                .HasDefaultValue("Fixed");
            entity.Property(exam => exam.QuestionCount).HasColumnName("SoCauHoi");
            entity.HasOne(exam => exam.Chapter)
                .WithMany()
                .HasForeignKey(exam => exam.IDChapter)
                .HasConstraintName("FK_BaiKiemTra_ChuongHoc")
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(exam => exam.Lesson)
                .WithMany()
                .HasForeignKey(exam => exam.IDLesson)
                .HasConstraintName("FK_BaiKiemTra_BaiHoc")
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<ExamExercise>(entity =>
        {
            entity.ToTable("ChiTietBaiKiemTra");
            entity.HasKey(item => new { item.IDExam, item.IDExercise });
            entity.Property(item => item.IDExam).HasColumnName("IDBaiKiemTra");
            entity.Property(item => item.IDExercise).HasColumnName("IDBaiTap");
            entity.Property(item => item.Order).HasColumnName("ThuTu");
            entity.Property(item => item.MaxScore)
                .HasColumnName("DiemToiDa")
                .HasColumnType("decimal(5,2)")
                .HasDefaultValue(1);
            entity.HasIndex(item => new { item.IDExam, item.Order }).IsUnique();
            entity.HasIndex(item => item.IDExercise)
                .HasDatabaseName("IX_ChiTietBaiKiemTra_IDBaiTap");
            entity.HasOne(item => item.Exam)
                .WithMany()
                .HasForeignKey(item => item.IDExam)
                .HasConstraintName("FK_ChiTietBaiKiemTra_BaiKiemTra")
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(item => item.Exercise)
                .WithMany(exercise => exercise.ExamExercises)
                .HasForeignKey(item => item.IDExercise)
                .HasConstraintName("FK_ChiTietBaiKiemTra_BaiTap");
        });

        modelBuilder.Entity<LearningAttempt>(entity =>
        {
            entity.ToTable("LanLamBai");
            entity.HasKey(attempt => attempt.IDAttempt);
            entity.Property(attempt => attempt.IDAttempt).HasColumnName("IDLanLam");
            entity.Property(attempt => attempt.IDUser).HasColumnName("IDUser");
            entity.Property(attempt => attempt.IDExam).HasColumnName("IDBaiKiemTra");
            entity.Property(attempt => attempt.IDLesson).HasColumnName("IDBaiHoc");
            entity.Property(attempt => attempt.IDExerciseSet).HasColumnName("IDDeBaiTap");
            entity.Property(attempt => attempt.AttemptType).HasColumnName("LoaiLanLam").HasMaxLength(20).IsRequired();
            entity.Property(attempt => attempt.Status)
                .HasColumnName("TrangThai")
                .HasMaxLength(20)
                .HasDefaultValue("Draft");
            entity.Property(attempt => attempt.StartedAt)
                .HasColumnName("BatDauLuc")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("SYSUTCDATETIME()");
            entity.Property(attempt => attempt.SubmittedAt)
                .HasColumnName("NopLuc")
                .HasColumnType("datetime2");
            entity.Property(attempt => attempt.DurationSeconds).HasColumnName("ThoiLuongGiay");
            entity.Property(attempt => attempt.UpdatedAt)
                .HasColumnName("CapNhatLuc")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("SYSUTCDATETIME()");
            entity.Property(attempt => attempt.DueAt)
                .HasColumnName("HanNopLuc")
                .HasColumnType("datetime2");
            entity.Property(attempt => attempt.TotalScore)
                .HasColumnName("TongDiem")
                .HasColumnType("decimal(6,2)");
            entity.Property(attempt => attempt.CorrectCount)
                .HasColumnName("SoCauDung")
                .HasDefaultValue(0);
            entity.Property(attempt => attempt.WrongCount)
                .HasColumnName("SoCauSai")
                .HasDefaultValue(0);
            entity.Property(attempt => attempt.PercentCorrect)
                .HasColumnName("PhanTramDung")
                .HasColumnType("decimal(5,2)");
            entity.HasIndex(attempt => attempt.IDUser);
            entity.HasIndex(attempt => attempt.IDExerciseSet);
            entity.HasIndex(attempt => new { attempt.Status, attempt.AttemptType, attempt.SubmittedAt })
                .HasDatabaseName("IX_LanLamBai_Dashboard");
            entity.HasOne(attempt => attempt.User)
                .WithMany()
                .HasForeignKey(attempt => attempt.IDUser)
                .HasConstraintName("FK_LanLamBai_NguoiDung")
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(attempt => attempt.Exam)
                .WithMany()
                .HasForeignKey(attempt => attempt.IDExam)
                .HasConstraintName("FK_LanLamBai_BaiKiemTra")
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(attempt => attempt.Lesson)
                .WithMany()
                .HasForeignKey(attempt => attempt.IDLesson)
                .HasConstraintName("FK_LanLamBai_BaiHoc");
            entity.HasOne(attempt => attempt.ExerciseSet)
                .WithMany()
                .HasForeignKey(attempt => attempt.IDExerciseSet)
                .HasConstraintName("FK_LanLamBai_DeBaiTap")
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<LearningAttemptDetail>(entity =>
        {
            entity.ToTable("ChiTietLanLamBai");
            entity.HasKey(detail => new { detail.IDAttempt, detail.IDExercise });
            entity.Property(detail => detail.IDAttempt).HasColumnName("IDLanLam");
            entity.Property(detail => detail.IDExercise).HasColumnName("IDBaiTap");
            entity.Property(detail => detail.Answer).HasColumnName("CauTraLoi");
            entity.Property(detail => detail.Score)
                .HasColumnName("Diem")
                .HasColumnType("decimal(5,2)")
                .HasDefaultValue(0);
            entity.Property(detail => detail.Order).HasColumnName("ThuTu");
            entity.Property(detail => detail.IsCorrect).HasColumnName("LaDung");
            entity.Property(detail => detail.MaxScore)
                .HasColumnName("DiemToiDa")
                .HasColumnType("decimal(5,2)")
                .HasDefaultValue(1);
            entity.Property(detail => detail.HintViewed)
                .HasColumnName("DaXemGoiY")
                .HasDefaultValue(false);
            entity.Property(detail => detail.SolutionViewed)
                .HasColumnName("DaXemLoiGiai")
                .HasDefaultValue(false);
            entity.Property(detail => detail.QuestionSnapshot).HasColumnName("CauHoiSnapshot");
            entity.Property(detail => detail.CorrectAnswerSnapshot).HasColumnName("DapAnDungSnapshot");
            entity.Property(detail => detail.SolutionSnapshot).HasColumnName("LoiGiaiSnapshot");
            entity.Property(detail => detail.ChoiceSnapshot).HasColumnName("LuaChonSnapshot");
            entity.Property(detail => detail.ExerciseTypeSnapshot)
                .HasColumnName("LoaiBaiTapSnapshot")
                .HasMaxLength(50);
            entity.Property(detail => detail.AllowedErrorSnapshot)
                .HasColumnName("SaiSoChoPhepSnapshot")
                .HasColumnType("decimal(18,6)");
            entity.HasIndex(detail => detail.IDExercise)
                .HasDatabaseName("IX_ChiTietLanLamBai_IDBaiTap");
            entity.HasOne(detail => detail.Attempt)
                .WithMany()
                .HasForeignKey(detail => detail.IDAttempt)
                .HasConstraintName("FK_ChiTietLanLamBai_LanLamBai")
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(detail => detail.Exercise)
                .WithMany()
                .HasForeignKey(detail => detail.IDExercise)
                .HasConstraintName("FK_ChiTietLanLamBai_BaiTap");
        });

        modelBuilder.Entity<UserChemical>(entity =>
        {
            entity.ToTable("KhoHoaChatNguoiDung");
            entity.HasKey(item => new { item.IDUser, item.IDChemical });
            entity.Property(item => item.IDUser).HasColumnName("IDUser");
            entity.Property(item => item.IDChemical).HasColumnName("IDHoaChat");
            entity.Property(item => item.UnlockedAt)
                .HasColumnName("NgayMoKhoa")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasIndex(item => item.IDChemical);
            entity.HasOne(item => item.User)
                .WithMany()
                .HasForeignKey(item => item.IDUser)
                .HasConstraintName("FK_KhoHoaChatNguoiDung_NguoiDung")
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(item => item.Chemical)
                .WithMany()
                .HasForeignKey(item => item.IDChemical)
                .HasConstraintName("FK_KhoHoaChatNguoiDung_HoaChat")
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ReactionParticipant>(entity =>
        {
            entity.ToTable("ChiTietPhanUng");
            entity.HasKey(participant => new
            {
                participant.IDReaction,
                participant.IDChemical,
                participant.Role
            });
            entity.Property(participant => participant.IDReaction).HasColumnName("IDPhanUng");
            entity.Property(participant => participant.IDChemical).HasColumnName("IDHoaChat");
            entity.Property(participant => participant.Role)
                .HasColumnName("VaiTro")
                .HasMaxLength(20)
                .IsRequired();
            entity.Property(participant => participant.Coefficient).HasColumnName("HeSo");
            entity.HasOne(participant => participant.Reaction)
                .WithMany(reaction => reaction.Participants)
                .HasForeignKey(participant => participant.IDReaction)
                .HasConstraintName("FK_ChiTietPhanUng_PhanUng")
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(participant => participant.Chemical)
                .WithMany(chemical => chemical.ReactionParticipants)
                .HasForeignKey(participant => participant.IDChemical)
                .HasConstraintName("FK_ChiTietPhanUng_HoaChat")
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<PeriodicElement>(entity =>
        {
            entity.ToTable("BangTuanHoan");
            entity.HasKey(element => element.IDElement);
            entity.Property(element => element.IDElement).HasColumnName("IDNguyenTo");
            entity.Property(element => element.AtomicNumber).HasColumnName("SoHieuNguyenTu");
            entity.Property(element => element.Symbol)
                .HasColumnName("KyHieu")
                .HasMaxLength(3)
                .IsRequired();
            entity.Property(element => element.Name)
                .HasColumnName("TenNguyenTo")
                .HasMaxLength(100)
                .IsRequired();
            entity.Property(element => element.AtomicMass)
                .HasColumnName("KhoiLuongNguyenTu")
                .HasColumnType("numeric(10,4)");
            entity.Property(element => element.EnglishName)
                .HasColumnName("TenTiengAnh")
                .HasMaxLength(100);
            entity.Property(element => element.Period).HasColumnName("ChuKy");
            entity.Property(element => element.Group).HasColumnName("Nhom");
            entity.Property(element => element.Block).HasColumnName("Khoi").HasMaxLength(5);
            entity.Property(element => element.Type).HasColumnName("LoaiNguyenTo").HasMaxLength(50);
            entity.Property(element => element.State).HasColumnName("TrangThaiODieuKienThuong").HasMaxLength(20);
            entity.Property(element => element.Color).HasColumnName("MauSac").HasMaxLength(100);
            entity.Property(element => element.Electronegativity)
                .HasColumnName("DoAmDien")
                .HasColumnType("numeric(4,2)");
            entity.Property(element => element.ElectronConfiguration).HasColumnName("CauHinhElectron");
            entity.Property(element => element.Description).HasColumnName("MoTa");
            entity.Property(element => element.Applications).HasColumnName("UngDung");
            entity.Property(element => element.ImageUrl).HasColumnName("ImageUrl");
            entity.HasIndex(element => element.AtomicNumber).IsUnique();
            entity.HasIndex(element => element.Symbol).IsUnique();
        });

        modelBuilder.Entity<ChemicalComponent>(entity =>
        {
            entity.ToTable("ThanhPhanHoaChat");
            entity.HasKey(component => new { component.IDChemical, component.IDElement });
            entity.Property(component => component.IDChemical).HasColumnName("IDHoaChat");
            entity.Property(component => component.IDElement).HasColumnName("IDNguyenTo");
            entity.Property(component => component.AtomCount).HasColumnName("SoNguyenTu");
            entity.HasIndex(component => component.IDElement)
                .HasDatabaseName("IX_ThanhPhanHoaChat_IDNguyenTo");
            entity.HasOne(component => component.Chemical)
                .WithMany()
                .HasForeignKey(component => component.IDChemical)
                .HasConstraintName("FK_ThanhPhanHoaChat_HoaChat")
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(component => component.Element)
                .WithMany()
                .HasForeignKey(component => component.IDElement)
                .HasConstraintName("FK_ThanhPhanHoaChat_BangTuanHoan")
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ElementOxidationState>(entity =>
        {
            entity.ToTable("NguyenToTrangThaiOxiHoa");
            entity.HasKey(item => item.IDState);
            entity.Property(item => item.IDState).HasColumnName("IDTrangThaiOxiHoa");
            entity.Property(item => item.IDElement).HasColumnName("IDNguyenTo");
            entity.Property(item => item.OxidationState).HasColumnName("SoOxiHoa");
            entity.ToTable(table => table.HasCheckConstraint("CK_NguyenToTrangThaiOxiHoa_SoOxiHoa", "SoOxiHoa BETWEEN -8 AND 8"));
            entity.HasIndex(item => new { item.IDElement, item.OxidationState }).IsUnique();
            entity.HasOne(item => item.Element)
                .WithMany(element => element.OxidationStates)
                .HasForeignKey(item => item.IDElement)
                .HasConstraintName("FK_NguyenToTrangThaiOxiHoa_BangTuanHoan")
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ChemicalOxidationState>(entity =>
        {
            entity.ToTable("HoaChatTrangThaiOxiHoa");
            entity.HasKey(item => item.IDState);
            entity.Property(item => item.IDState).HasColumnName("IDTrangThaiOxiHoa");
            entity.Property(item => item.IDChemical).HasColumnName("IDHoaChat");
            entity.Property(item => item.IDElement).HasColumnName("IDNguyenTo");
            entity.Property(item => item.OxidationState).HasColumnName("SoOxiHoa");
            entity.ToTable(table => table.HasCheckConstraint("CK_HoaChatTrangThaiOxiHoa_SoOxiHoa", "SoOxiHoa BETWEEN -8 AND 8"));
            entity.HasIndex(item => new { item.IDChemical, item.IDElement, item.OxidationState }).IsUnique();
            entity.HasIndex(item => item.IDElement);
            entity.HasOne(item => item.Chemical)
                .WithMany(chemical => chemical.OxidationStates)
                .HasForeignKey(item => item.IDChemical)
                .HasConstraintName("FK_HoaChatTrangThaiOxiHoa_HoaChat")
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(item => item.Element)
                .WithMany(element => element.ChemicalOxidationStates)
                .HasForeignKey(item => item.IDElement)
                .HasConstraintName("FK_HoaChatTrangThaiOxiHoa_BangTuanHoan")
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ChemicalRedoxCapability>(entity =>
        {
            entity.ToTable("HoaChatKhaNangRedox");
            entity.HasKey(item => item.IDCapability);
            entity.Property(item => item.IDCapability).HasColumnName("IDKhaNangRedox");
            entity.Property(item => item.IDChemical).HasColumnName("IDHoaChat");
            entity.Property(item => item.Role).HasColumnName("VaiTro").HasMaxLength(30).IsRequired();
            entity.Property(item => item.Condition).HasColumnName("DieuKien").HasMaxLength(50);
            entity.ToTable(table => table.HasCheckConstraint("CK_HoaChatKhaNangRedox_VaiTro", "VaiTro IN (N'OxidizingAgent', N'ReducingAgent', N'Both', N'Neither')"));
            entity.HasIndex(item => new { item.IDChemical, item.Role, item.Condition }).IsUnique();
            entity.HasIndex(item => item.Role);
            entity.HasOne(item => item.Chemical)
                .WithMany(chemical => chemical.RedoxCapabilities)
                .HasForeignKey(item => item.IDChemical)
                .HasConstraintName("FK_HoaChatKhaNangRedox_HoaChat")
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ChemicalWaterReactivity>(entity =>
        {
            entity.ToTable("HoaChatPhanUngVoiNuoc");
            entity.HasKey(item => item.IDReactivity);
            entity.Property(item => item.IDReactivity).HasColumnName("IDPhanUngVoiNuoc");
            entity.Property(item => item.IDChemical).HasColumnName("IDHoaChat");
            entity.Property(item => item.WaterPhase).HasColumnName("PhaNuoc").HasMaxLength(20).IsRequired();
            entity.Property(item => item.MinTemperatureC).HasColumnName("NhietDoToiThieuC").HasColumnType("decimal(6,2)");
            entity.Property(item => item.MaxTemperatureC).HasColumnName("NhietDoToiDaC").HasColumnType("decimal(6,2)");
            entity.Property(item => item.ProductFamily).HasColumnName("HoSanPham").HasMaxLength(40).IsRequired();
            entity.Property(item => item.IsActive).HasColumnName("DangHoatDong").HasDefaultValue(true);
            entity.ToTable(table =>
            {
                table.HasCheckConstraint("CK_HoaChatPhanUngVoiNuoc_PhaNuoc", "PhaNuoc IN (N'ColdWater', N'HotWater', N'Steam')");
                table.HasCheckConstraint("CK_HoaChatPhanUngVoiNuoc_NhietDo", "NhietDoToiThieuC IS NULL OR NhietDoToiDaC IS NULL OR NhietDoToiThieuC <= NhietDoToiDaC");
            });
            entity.HasIndex(item => new { item.IDChemical, item.WaterPhase, item.ProductFamily }).IsUnique();
            entity.HasIndex(item => item.WaterPhase);
            entity.HasOne(item => item.Chemical)
                .WithMany(chemical => chemical.WaterReactivities)
                .HasForeignKey(item => item.IDChemical)
                .HasConstraintName("FK_HoaChatPhanUngVoiNuoc_HoaChat")
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ChemicalAmphotericBehavior>(entity =>
        {
            entity.ToTable("HoaChatTinhLuongTinh");
            entity.HasKey(item => item.IDBehavior);
            entity.Property(item => item.IDBehavior).HasColumnName("IDTinhLuongTinh");
            entity.Property(item => item.IDChemical).HasColumnName("IDHoaChat");
            entity.Property(item => item.ChemicalForm).HasColumnName("DangChat").HasMaxLength(30).IsRequired();
            entity.Property(item => item.Medium).HasColumnName("MoiTruong").HasMaxLength(30).IsRequired();
            entity.Property(item => item.RequiresWater).HasColumnName("CanNuoc").HasDefaultValue(false);
            entity.Property(item => item.ProductFamily).HasColumnName("HoSanPham").HasMaxLength(40).IsRequired();
            entity.Property(item => item.IsActive).HasColumnName("DangHoatDong").HasDefaultValue(true);
            entity.ToTable(table =>
            {
                table.HasCheckConstraint("CK_HoaChatTinhLuongTinh_DangChat", "DangChat IN (N'Metal', N'Oxide', N'Hydroxide')");
                table.HasCheckConstraint("CK_HoaChatTinhLuongTinh_MoiTruong", "MoiTruong IN (N'Acid', N'StrongBase')");
            });
            entity.HasIndex(item => new { item.IDChemical, item.ChemicalForm, item.Medium, item.ProductFamily }).IsUnique();
            entity.HasIndex(item => item.Medium);
            entity.HasOne(item => item.Chemical)
                .WithMany(chemical => chemical.AmphotericBehaviors)
                .HasForeignKey(item => item.IDChemical)
                .HasConstraintName("FK_HoaChatTinhLuongTinh_HoaChat")
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ChemicalAcidBehavior>(entity =>
        {
            entity.ToTable("HoaChatTinhAcid");
            entity.HasKey(item => item.IDBehavior);
            entity.Property(item => item.IDBehavior).HasColumnName("IDTinhAcid");
            entity.Property(item => item.IDChemical).HasColumnName("IDHoaChat");
            entity.Property(item => item.AcidKind).HasColumnName("KieuAcid").HasMaxLength(30).IsRequired();
            entity.Property(item => item.Condition).HasColumnName("DieuKien").HasMaxLength(50);
            entity.Property(item => item.ReductionProduct).HasColumnName("SanPhamKhu").HasMaxLength(80);
            entity.ToTable(table => table.HasCheckConstraint("CK_HoaChatTinhAcid_KieuAcid", "KieuAcid IN (N'NonOxidizing', N'Oxidizing', N'ConditionDependent')"));
            entity.HasIndex(item => new { item.IDChemical, item.AcidKind, item.Condition, item.ReductionProduct }).IsUnique();
            entity.HasIndex(item => item.AcidKind);
            entity.HasOne(item => item.Chemical)
                .WithMany(chemical => chemical.AcidBehaviors)
                .HasForeignKey(item => item.IDChemical)
                .HasConstraintName("FK_HoaChatTinhAcid_HoaChat")
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ChemicalProductFamily>(entity =>
        {
            entity.ToTable("HoaChatHoSanPham");
            entity.HasKey(item => item.IDProductFamily);
            entity.Property(item => item.IDProductFamily).HasColumnName("IDHoSanPham");
            entity.Property(item => item.IDChemical).HasColumnName("IDHoaChat");
            entity.Property(item => item.ProductFamily).HasColumnName("HoSanPham").HasMaxLength(40).IsRequired();
            entity.HasIndex(item => new { item.IDChemical, item.ProductFamily }).IsUnique();
            entity.HasIndex(item => item.ProductFamily);
            entity.HasOne(item => item.Chemical)
                .WithMany(chemical => chemical.ProductFamilies)
                .HasForeignKey(item => item.IDChemical)
                .HasConstraintName("FK_HoaChatHoSanPham_HoaChat")
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AboutContent>(entity =>
        {
            entity.ToTable("VeChungToi");
            entity.HasKey(content => content.IDAboutContent);
            entity.Property(content => content.IDAboutContent).HasColumnName("IDVeChungToi");
            entity.Property(content => content.Title).HasColumnName("TieuDe").HasMaxLength(150).IsRequired();
            entity.Property(content => content.Content).HasColumnName("NoiDung").IsRequired();
            entity.Property(content => content.ImageUrl).HasColumnName("ImageUrl");
            entity.Property(content => content.Order).HasColumnName("ThuTu");
            entity.Property(content => content.IsVisible)
                .HasColumnName("DangHienThi")
                .HasDefaultValue(true);
            entity.Property(content => content.UpdatedAt)
                .HasColumnName("NgayCapNhat")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasIndex(content => new { content.IsVisible, content.Order });
        });
    }
}
