// Generated from RasepiDbContext.cs by the smoower transform - token-measurement
// artifact, NOT compiled. Relational-only calls (HasDefaultValue/HasFilter/
// HasDatabaseName) and the few one-to-one WithOne chains keep their long form.
﻿
public class SampleDbContext : DbContext
{
    private readonly TenantContext _tenantContext;
    public SampleDbContext(DbContextOptions<SampleDbContext> options, TenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<TenantMembership> TenantMemberships => Set<TenantMembership>();
    public DbSet<TenantInvitation> TenantInvitations => Set<TenantInvitation>();
    public DbSet<Hub> Hubs => Set<Hub>();
    public DbSet<Entry> Entries => Set<Entry>();
    public DbSet<EntryTranslation> EntryTranslations => Set<EntryTranslation>();
    public DbSet<EntryBlock> EntryBlocks => Set<EntryBlock>();
    public DbSet<TranslationBlock> TranslationBlocks => Set<TranslationBlock>();
    public DbSet<User> Users => Set<User>();
    public DbSet<EditSession> EditSessions => Set<EditSession>();
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<GroupMembership> GroupMemberships => Set<GroupMembership>();
    public DbSet<HubMembership> HubMemberships => Set<HubMembership>();
    public DbSet<EntryPermission> EntryPermissions => Set<EntryPermission>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<BlockTypeHeuristic> BlockTypeHeuristics => Set<BlockTypeHeuristic>();
    public DbSet<HubTag> HubTags => Set<HubTag>();
    public DbSet<EntryTag> EntryTags => Set<EntryTag>();
    public DbSet<EntryActivity> DocumentActivities => Set<EntryActivity>();
    public DbSet<TenantLanguageConfig> TenantLanguageConfigs => Set<TenantLanguageConfig>();
    public DbSet<TenantGlossary> TenantGlossaries => Set<TenantGlossary>();
    public DbSet<TenantGlossaryEntry> TenantGlossaryEntries => Set<TenantGlossaryEntry>();
    public DbSet<TenantStyleRuleList> TenantStyleRuleLists => Set<TenantStyleRuleList>();
    public DbSet<TenantCustomInstruction> TenantCustomInstructions => Set<TenantCustomInstruction>();
    public DbSet<TenantPluginInstallation> TenantPluginInstallations => Set<TenantPluginInstallation>();
    public DbSet<ConnectorInstallation> ConnectorInstallations => Set<ConnectorInstallation>();
    public DbSet<EntryTemplate> EntryTemplates => Set<EntryTemplate>();
    public DbSet<EntryTemplateBlock> EntryTemplateBlocks => Set<EntryTemplateBlock>();
    public DbSet<TemplateRating> TemplateRatings => Set<TemplateRating>();
    public DbSet<TemplateUsage> TemplateUsages => Set<TemplateUsage>();
    public DbSet<TranslationHistory> TranslationHistories => Set<TranslationHistory>();
    public DbSet<EntryStarred> EntryStarreds => Set<EntryStarred>();
    public DbSet<EntryPin> EntryPins => Set<EntryPin>();
    public DbSet<EntryWatcher> EntryWatchers => Set<EntryWatcher>();
    public DbSet<EntryVersion> EntryVersions => Set<EntryVersion>();
    public DbSet<EntryVersionBlock> EntryVersionBlocks => Set<EntryVersionBlock>();
    public DbSet<ExpiryTemplate> ExpiryTemplates => Set<ExpiryTemplate>();
    public DbSet<EntryReviewHistory> DocumentReviewHistories => Set<EntryReviewHistory>();
    public DbSet<ExpiryNotification> ExpiryNotifications => Set<ExpiryNotification>();
    public DbSet<NotificationChannel> NotificationChannels => Set<NotificationChannel>();
    public DbSet<ReviewChecklist> ReviewChecklists => Set<ReviewChecklist>();
    public DbSet<ExpiryConfigAuditLog> ExpiryConfigAuditLogs => Set<ExpiryConfigAuditLog>();
    public DbSet<FreshnessSignal> FreshnessSignals => Set<FreshnessSignal>();
    public DbSet<FreshnessScore> FreshnessScores => Set<FreshnessScore>();
    public DbSet<ExternalSourceLink> ExternalSourceLinks => Set<ExternalSourceLink>();
    public DbSet<ExternalSourceChange> ExternalSourceChanges => Set<ExternalSourceChange>();
    public DbSet<BlockEmbedding> BlockEmbeddings => Set<BlockEmbedding>();
    public DbSet<EntryEmbedding> EntryEmbeddings => Set<EntryEmbedding>();
    public DbSet<AgentConversation> AgentConversations => Set<AgentConversation>();
    public DbSet<AgentMessage> AgentMessages => Set<AgentMessage>();
    public DbSet<TenantAIUsage> TenantAIUsages => Set<TenantAIUsage>();
    public DbSet<AIPromptLog> AIPromptLogs => Set<AIPromptLog>();
    public DbSet<PersonalSpace> PersonalSpaces => Set<PersonalSpace>();
    public DbSet<PersonalNote> PersonalNotes => Set<PersonalNote>();
    public DbSet<PersonalNoteBlock> PersonalNoteBlocks => Set<PersonalNoteBlock>();
    public DbSet<PersonalNoteShare> PersonalNoteShares => Set<PersonalNoteShare>();
    public DbSet<PersonalNoteTag> PersonalNoteTags => Set<PersonalNoteTag>();
    public DbSet<PersonalNoteEmbedding> PersonalNoteEmbeddings => Set<PersonalNoteEmbedding>();
    public DbSet<UserAIUsage> UserAIUsages => Set<UserAIUsage>();
    public DbSet<TenantSubscription> TenantSubscriptions => Set<TenantSubscription>();
    public DbSet<ShareLink> ShareLinks => Set<ShareLink>();
    public DbSet<AuditLogEntry> AuditLogEntries => Set<AuditLogEntry>();
    public DbSet<ApiKey> ApiKeys => Set<ApiKey>();
    public DbSet<AiApplication> AiApplications => Set<AiApplication>();
    public DbSet<AiApplicationSource> AiApplicationSources => Set<AiApplicationSource>();
    public DbSet<AiUsePolicy> AiUsePolicies => Set<AiUsePolicy>();
    public DbSet<EntryAiPolicyOverride> EntryAiPolicyOverrides => Set<EntryAiPolicyOverride>();
    public DbSet<SensitiveDetection> SensitiveDetections => Set<SensitiveDetection>();
    public DbSet<TrustDecisionRecord> TrustDecisionRecords => Set<TrustDecisionRecord>();
    public DbSet<TrustDecisionSource> TrustDecisionSources => Set<TrustDecisionSource>();
    public DbSet<TrustUsageLog> TrustUsageLogs => Set<TrustUsageLog>();
    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);
        if (Database.IsNpgsql())
            mb.HasPostgresExtension("vector");
        mb.UseOpenIddict();
        mb.ApplyConfigurationsFromAssembly(GetType().Assembly);
        ApplyTenantFilters(mb);
        mb.qf<HubMembership>(e => e.Hub.TenantId == _tenantContext.TenantId);
        mb.qf<HubTag>(e => e.Hub.TenantId == _tenantContext.TenantId);
        mb.qf<GroupMembership>(e => e.Group.TenantId == _tenantContext.TenantId);
        mb.qf<EntryBlock>(e => e.Entry.TenantId == _tenantContext.TenantId);
        mb.qf<TranslationBlock>(e => e.Entry.TenantId == _tenantContext.TenantId);
        mb.qf<EntryTranslation>(e => e.Entry.TenantId == _tenantContext.TenantId);
        mb.qf<EditSession>(e => e.Entry.TenantId == _tenantContext.TenantId);
        mb.qf<EntryPermission>(e => e.Entry.TenantId == _tenantContext.TenantId);
        mb.qf<EntryTag>(e => e.Entry.TenantId == _tenantContext.TenantId);
        mb.qf<EntryActivity>(e => e.Entry.TenantId == _tenantContext.TenantId);
        mb.qf<EntryReviewHistory>(e => e.Entry.TenantId == _tenantContext.TenantId);
        mb.qf<ExpiryNotification>(e => e.Entry.TenantId == _tenantContext.TenantId);
        mb.qf<ExpiryConfigAuditLog>(e => e.Entry.TenantId == _tenantContext.TenantId);
        mb.qf<FreshnessSignal>(e => e.Entry.TenantId == _tenantContext.TenantId);
        mb.qf<FreshnessScore>(e => e.Entry.TenantId == _tenantContext.TenantId);
        mb.qf<ExternalSourceChange>(c => c.SourceLink.TenantId == _tenantContext.TenantId);
        mb.qf<BlockEmbedding>(e => e.Entry.TenantId == _tenantContext.TenantId);
        mb.qf<EntryEmbedding>(e => e.Entry.TenantId == _tenantContext.TenantId);
        mb.qf<AgentMessage>(e => e.Conversation.TenantId == _tenantContext.TenantId);
        mb.qf<PersonalSpace>(e => e.TenantId == _tenantContext.TenantId
                && e.OwnerUserId == _tenantContext.UserId);
        mb.qf<PersonalNote>(e => e.TenantId == _tenantContext.TenantId
                && e.OwnerUserId == _tenantContext.UserId
                && e.DeletedAt == null);
        mb.qf<PersonalNoteBlock>(e => e.Note.TenantId == _tenantContext.TenantId
                && e.Note.OwnerUserId == _tenantContext.UserId);
        mb.qf<PersonalNoteShare>(e => e.Note.TenantId == _tenantContext.TenantId
                && e.Note.OwnerUserId == _tenantContext.UserId);
        mb.qf<PersonalNoteTag>(e => e.Note.TenantId == _tenantContext.TenantId
                && e.Note.OwnerUserId == _tenantContext.UserId);
        mb.qf<PersonalNoteEmbedding>(e => e.TenantId == _tenantContext.TenantId
                && e.OwnerUserId == _tenantContext.UserId);
        mb.qf<UserAIUsage>(e => e.TenantId == _tenantContext.TenantId
                && e.UserId == _tenantContext.UserId);
        mb.qf<TenantGlossaryEntry>(e => e.Glossary.TenantId == _tenantContext.TenantId);
        mb.qf<TenantCustomInstruction>(e => e.StyleRuleList.TenantId == _tenantContext.TenantId);
        mb.qf<EntryStarred>(e => e.Entry.TenantId == _tenantContext.TenantId);
        mb.qf<EntryWatcher>(e => e.Entry.TenantId == _tenantContext.TenantId);
        mb.qf<EntryVersion>(e => e.Entry.TenantId == _tenantContext.TenantId);
        mb.qf<EntryVersionBlock>(b => b.Version.Entry.TenantId == _tenantContext.TenantId);
        mb.qf<ShareLink>(l => l.Entry!.TenantId == _tenantContext.TenantId);
        mb.qf<TemplateRating>(r => r.Template.TenantId == _tenantContext.TenantId);
        mb.qf<EntryTemplateBlock>(b => b.Template.TenantId == _tenantContext.TenantId);
        mb.qf<TemplateUsage>(u => u.TenantId == _tenantContext.TenantId);
        mb.Entity<Tenant>(entity =>
        {
            entity.key(e => e.Id);
            entity.idx(e => e.Slug).uniq();
            entity.p(e => e.Slug).req().max(100);
            entity.p(e => e.Name).req().max(200);
            entity.hasM(e => e.TenantMemberships)
                .wOne(m => m.Tenant)
                .fk(m => m.TenantId)
                .onDel(DeleteBehavior.Cascade);
            entity.hasM(e => e.Hubs)
                .wOne(s => s.Tenant)
                .fk(s => s.TenantId)
                .onDel(DeleteBehavior.Cascade);
            entity.hasM(e => e.Groups)
                .wOne()
                .fk(g => g.TenantId)
                .onDel(DeleteBehavior.Cascade);
            entity.one(e => e.Subscription)
                .wOne(s => s.Tenant)
                .HasForeignKey<TenantSubscription>(s => s.TenantId)
                .onDel(DeleteBehavior.Cascade);
        });
        mb.Entity<TenantSubscription>(entity =>
        {
            entity.key(e => e.Id);
            entity.idx(e => e.TenantId).uniq();
            entity.idx(e => e.StripeCustomerId).uniq().HasFilter("\"StripeCustomerId\" IS NOT NULL");
            entity.idx(e => e.StripeSubscriptionId).uniq().HasFilter("\"StripeSubscriptionId\" IS NOT NULL");
            entity.p(e => e.StripeCustomerId).max(255);
            entity.p(e => e.StripeSubscriptionId).max(255);
            entity.p(e => e.StripePriceId).max(255);
        });
        mb.Entity<TenantMembership>(entity =>
        {
            entity.key(e => e.Id);
            entity.idx(e => new { e.TenantId, e.UserId }).uniq();
            entity.idx(e => e.UserId)
                .uniq()
                .HasFilter($"\"Role\" = {(int)Rasepi.Shared.Models.Tenants.TenantRole.Owner}")
                .HasDatabaseName("IX_TenantMembership_OneOwnerPerUser");
            entity.one(e => e.Tenant)
                .many(t => t.TenantMemberships)
                .fk(e => e.TenantId)
                .onDel(DeleteBehavior.Cascade);
            entity.one(e => e.User)
                .many(u => u.TenantMemberships)
                .fk(e => e.UserId)
                .onDel(DeleteBehavior.Restrict); // SQL Server: cannot have two cascade paths to TenantMemberships
            entity.one(e => e.InvitedBy)
                .many()
                .fk(e => e.InvitedByUserId)
                .onDel(DeleteBehavior.ClientSetNull); // SQL Server: SET NULL not allowed when multiple FKs from Users→TenantMemberships; EF handles null in-memory
        });
        mb.Entity<TenantInvitation>(entity =>
        {
            entity.key(e => e.Id);
            entity.idx(e => new { e.TenantId, e.Email });
            entity.p(e => e.Email).req().max(256);
            entity.one(e => e.Tenant)
                .many()
                .fk(e => e.TenantId)
                .onDel(DeleteBehavior.Cascade);
            entity.one(e => e.InvitedBy)
                .many()
                .fk(e => e.InvitedByUserId)
                .onDel(DeleteBehavior.Restrict);
            entity.one(e => e.AcceptedBy)
                .many()
                .fk(e => e.AcceptedByUserId)
                .onDel(DeleteBehavior.ClientSetNull);
        });
        mb.Entity<Hub>(entity =>
        {
            entity.key(e => e.Id);
            entity.idx(e => new { e.TenantId, e.Key }).uniq();
            entity.p(e => e.Name).req().max(200);
            entity.p(e => e.Key).req().max(100);
            entity.p(e => e.Icon).max(100);
            entity.p(e => e.IconColor).max(20);
            entity.p(e => e.Origin)
                .conv<string>()
                .max(20)
                .HasDefaultValue(HubOrigin.Native);
            entity.idx(e => new { e.TenantId, e.Origin });
            entity.one(e => e.ConnectorInstallation)
                .many()
                .fk(e => e.ConnectorInstallationId)
                .onDel(DeleteBehavior.SetNull);
            entity.one(e => e.Owner)
                .many()
                .fk(e => e.OwnerId)
                .onDel(DeleteBehavior.Restrict);
            entity.one(e => e.CreatedBy)
                .many()
                .fk(e => e.CreatedById)
                .onDel(DeleteBehavior.Restrict);
            entity.one(e => e.UpdatedBy)
                .many()
                .fk(e => e.UpdatedById)
                .onDel(DeleteBehavior.Restrict);
            entity.one(e => e.DeletedBy)
                .many()
                .fk(e => e.DeletedById)
                .onDel(DeleteBehavior.Restrict);
        });
        mb.Entity<Entry>(entity =>
        {
            entity.key(e => e.Id);
            entity.idx(e => new { e.HubId, e.Key }).uniq();
            entity.p(e => e.OriginalLanguage).req().max(10);
            entity.p(e => e.Origin)
                .conv<string>()
                .max(20)
                .HasDefaultValue(EntryOrigin.Native);
            entity.p(e => e.ConnectorExternalId).max(500);
            entity.p(e => e.ConnectorExternalUrl).max(2000);
            entity.p(e => e.ConnectorExternalVersion).max(200);
            entity.p(e => e.ConnectorTitle).max(500);
            entity.idx(e => new { e.ConnectorInstallationId, e.ConnectorExternalId });
            entity.one(e => e.ConnectorInstallation)
                .many()
                .fk(e => e.ConnectorInstallationId)
                .onDel(DeleteBehavior.SetNull);
            entity.one(e => e.Hub)
                .many(s => s.Entries)
                .fk(e => e.HubId)
                .onDel(DeleteBehavior.Cascade);
            entity.one(e => e.Owner)
                .many()
                .fk(e => e.OwnerId)
                .onDel(DeleteBehavior.Restrict);
            entity.one(e => e.CreatedBy)
                .many()
                .fk(e => e.CreatedById)
                .onDel(DeleteBehavior.Restrict);
            entity.one(e => e.UpdatedBy)
                .many()
                .fk(e => e.UpdatedById)
                .onDel(DeleteBehavior.Restrict);
            entity.one(e => e.DeletedBy)
                .many()
                .fk(e => e.DeletedById)
                .onDel(DeleteBehavior.Restrict);
            entity.p(e => e.SortOrder).HasDefaultValue(0);
            entity.idx(e => new { e.HubId, e.SortOrder });
        });
        mb.Entity<EntryBlock>(entity =>
        {
            entity.key(e => e.Id);
            entity.idx(e => new { e.EntryId, e.Language, e.Position });
            entity.p(e => e.Language).req().max(10);
            entity.p(e => e.BlockType).req().max(50);
            entity.p(e => e.ContentHash).req().max(64);
            entity.p(e => e.ConnectorBlockKey).max(1000);
            entity.one(e => e.Entry)
                .many()
                .fk(e => e.EntryId)
                .onDel(DeleteBehavior.Cascade);
            entity.one(e => e.CreatedBy)
                .many()
                .fk(e => e.CreatedById)
                .onDel(DeleteBehavior.Restrict);
            entity.one(e => e.UpdatedBy)
                .many()
                .fk(e => e.UpdatedById)
                .onDel(DeleteBehavior.Restrict);
            entity.one(e => e.DeletedBy)
                .many()
                .fk(e => e.DeletedById)
                .onDel(DeleteBehavior.Restrict);
            entity.one(e => e.BlockTypeHeuristic)
                .many()
                .fk(e => e.BlockTypeHeuristicId)
                .onDel(DeleteBehavior.SetNull);
        });
        mb.Entity<TranslationBlock>(entity =>
        {
            entity.key(e => e.Id);
            entity.idx(e => new { e.SourceBlockId, e.Language }).uniq();
            entity.idx(e => new { e.EntryId, e.Language, e.Position });
            entity.p(e => e.Language).req().max(10);
            entity.p(e => e.BlockType).req().max(50);
            entity.p(e => e.SourceBlockType).req().max(50);
            entity.p(e => e.SourceContentHash).req().max(64);
            entity.one(e => e.SourceBlock)
                .many(b => b.Translations)
                .fk(e => e.SourceBlockId)
                .onDel(DeleteBehavior.Cascade);
            entity.one(e => e.Entry)
                .many()
                .fk(e => e.EntryId)
                .onDel(DeleteBehavior.Restrict);
            entity.one(e => e.TranslatedBy)
                .many()
                .fk(e => e.TranslatedById)
                .onDel(DeleteBehavior.Restrict);
            entity.one(e => e.CreatedBy)
                .many()
                .fk(e => e.CreatedById)
                .onDel(DeleteBehavior.Restrict);
            entity.one(e => e.UpdatedBy)
                .many()
                .fk(e => e.UpdatedById)
                .onDel(DeleteBehavior.Restrict);
            entity.one(e => e.DeletedBy)
                .many()
                .fk(e => e.DeletedById)
                .onDel(DeleteBehavior.Restrict);
        });
        mb.Entity<EntryTranslation>(entity =>
        {
            entity.key(e => e.Id);
            entity.idx(e => new { e.EntryId, e.Language }).uniq();
            entity.p(e => e.Language).req().max(10);
            entity.p(e => e.Title).req().max(500);
            entity.one(e => e.Entry)
                .many(d => d.Translations)
                .fk(e => e.EntryId)
                .onDel(DeleteBehavior.Cascade);
            entity.one(e => e.CreatedBy)
                .many()
                .fk(e => e.CreatedById)
                .onDel(DeleteBehavior.Restrict);
            entity.one(e => e.UpdatedBy)
                .many()
                .fk(e => e.UpdatedById)
                .onDel(DeleteBehavior.Restrict);
            entity.one(e => e.DeletedBy)
                .many()
                .fk(e => e.DeletedById)
                .onDel(DeleteBehavior.Restrict);
        });
        mb.Entity<User>(entity =>
        {
            entity.key(e => e.Id);
            entity.idx(e => e.Email).uniq();
            entity.p(e => e.Email).req().max(256);
            entity.p(e => e.Name).req().max(200);
        });
        mb.Entity<EditSession>(entity =>
        {
            entity.key(e => e.Id);
            entity.idx(e => new { e.EntryId, e.Language, e.UserId });
            entity.one(e => e.Entry)
                .many(d => d.EditSessions)
                .fk(e => e.EntryId)
                .onDel(DeleteBehavior.Cascade);
            entity.one(e => e.User)
                .many()
                .fk(e => e.UserId)
                .onDel(DeleteBehavior.Cascade);
        });
        mb.Entity<Group>(entity =>
        {
            entity.key(e => e.Id);
            entity.idx(e => new { e.TenantId, e.Name }).uniq();
            entity.p(e => e.Name).req().max(200);
            entity.one(e => e.CreatedBy)
                .many()
                .fk(e => e.CreatedById)
                .onDel(DeleteBehavior.Restrict);
            entity.one(e => e.UpdatedBy)
                .many()
                .fk(e => e.UpdatedById)
                .onDel(DeleteBehavior.Restrict);
            entity.one(e => e.DeletedBy)
                .many()
                .fk(e => e.DeletedById)
                .onDel(DeleteBehavior.Restrict);
        });
        mb.Entity<GroupMembership>(entity =>
        {
            entity.key(e => e.Id);
            entity.idx(e => new { e.GroupId, e.UserId }).uniq();
            entity.one(e => e.Group)
                .many(g => g.Members)
                .fk(e => e.GroupId)
                .onDel(DeleteBehavior.Cascade);
            entity.one(e => e.User)
                .many(u => u.GroupMemberships)
                .fk(e => e.UserId)
                .onDel(DeleteBehavior.Cascade);
        });
        mb.Entity<HubMembership>(entity =>
        {
            entity.key(e => e.Id);
            entity.idx(e => new { e.HubId, e.UserId, e.GroupId });
            entity.one(e => e.Hub)
                .many(s => s.Memberships)
                .fk(e => e.HubId)
                .onDel(DeleteBehavior.Cascade);
            entity.one(e => e.User)
                .many(u => u.HubMemberships)
                .fk(e => e.UserId)
                .onDel(DeleteBehavior.Cascade);
            entity.one(e => e.Group)
                .many(g => g.HubMemberships)
                .fk(e => e.GroupId)
                .onDel(DeleteBehavior.ClientCascade); // SQL Server: GroupId + HubId both cascade from Tenant; EF handles group-driven deletions in-memory
        });
        mb.Entity<EntryPermission>(entity =>
        {
            entity.key(e => e.Id);
            entity.idx(e => new { e.EntryId, e.UserId, e.GroupId });
            entity.one(e => e.Entry)
                .many(d => d.Permissions)
                .fk(e => e.EntryId)
                .onDel(DeleteBehavior.Cascade);
            entity.one(e => e.User)
                .many(u => u.EntryPermissions)
                .fk(e => e.UserId)
                .onDel(DeleteBehavior.Cascade);
            entity.one(e => e.Group)
                .many(g => g.EntryPermissions)
                .fk(e => e.GroupId)
                .onDel(DeleteBehavior.ClientCascade); // SQL Server: GroupId + EntryId both cascade from Tenant; EF handles group-driven deletions in-memory
        });
        mb.Entity<Category>(entity =>
        {
            entity.key(e => e.Id);
            entity.idx(e => new { e.TenantId, e.Name }).uniq();
            entity.p(e => e.Name).req().max(100);
            entity.p(e => e.Description).max(500);
            entity.p(e => e.Color).max(7);
            entity.p(e => e.Icon).max(50);
        });
        mb.Entity<BlockTypeHeuristic>(entity =>
        {
            entity.key(e => e.Id);
            entity.idx(e => new { e.TenantId, e.Name }).uniq();
            entity.p(e => e.Name).req().max(100);
            entity.p(e => e.Description).max(500);
            entity.p(e => e.Color).max(7);
            entity.p(e => e.Icon).max(50);
        });
        mb.Entity<Tag>(entity =>
        {
            entity.key(e => e.Id);
            entity.idx(e => new { e.TenantId, e.Name }).uniq();
            entity.p(e => e.Name).req().max(50);
            entity.p(e => e.Color).max(7);
        });
        mb.Entity<HubTag>(entity =>
        {
            entity.key(e => new { e.HubId, e.TagId });
            entity.one(e => e.Hub)
                .many(s => s.HubTags)
                .fk(e => e.HubId)
                .onDel(DeleteBehavior.Cascade);
            entity.one(e => e.Tag)
                .many(t => t.HubTags)
                .fk(e => e.TagId)
                .onDel(DeleteBehavior.Cascade);
        });
        mb.Entity<EntryActivity>(entity =>
        {
            entity.key(e => e.Id);
            entity.idx(e => new { e.EntryId, e.OccurredAt });
            entity.idx(e => new { e.EntryId, e.EventType });
            entity.p(e => e.Language).max(10);
            entity.p(e => e.Metadata).max(2000);
            entity.one(e => e.Entry)
                .many()
                .fk(e => e.EntryId)
                .onDel(DeleteBehavior.Cascade);
            entity.one(e => e.User)
                .many()
                .fk(e => e.UserId)
                .onDel(DeleteBehavior.SetNull);
        });
        mb.Entity<TranslationHistory>(entity =>
        {
            entity.key(e => e.Id);
            entity.idx(e => new { e.TenantId, e.OccurredAt });
            entity.idx(e => new { e.TenantId, e.TargetLanguage });
            entity.p(e => e.SourceLanguage).req().max(20);
            entity.p(e => e.TargetLanguage).req().max(20);
            entity.p(e => e.Provider).max(100);
            entity.p(e => e.ErrorMessage).max(1000);
        });
        mb.Entity<ExpiryTemplate>(entity =>
        {
            entity.key(e => e.Id);
            entity.idx(e => new { e.TenantId, e.IsDefault });
            entity.p(e => e.Name).req().max(200);
            entity.p(e => e.WarningThresholds).max(100);
            entity.p(e => e.Color).max(20);
            entity.p(e => e.Icon).max(50);
            entity.hasM(e => e.Entries)
                .wOne(d => d.ExpiryTemplate)
                .fk(d => d.ExpiryTemplateId)
                .onDel(DeleteBehavior.SetNull);
            entity.hasM(e => e.HubsUsingAsDefault)
                .wOne(s => s.DefaultExpiryTemplate)
                .fk(s => s.DefaultExpiryTemplateId)
                .onDel(DeleteBehavior.SetNull);
            entity.one(e => e.CreatedBy)
                .many()
                .fk(e => e.CreatedById)
                .onDel(DeleteBehavior.Restrict);
            entity.one(e => e.UpdatedBy)
                .many()
                .fk(e => e.UpdatedById)
                .onDel(DeleteBehavior.Restrict);
            entity.one(e => e.DeletedBy)
                .many()
                .fk(e => e.DeletedById)
                .onDel(DeleteBehavior.Restrict);
        });
        mb.Entity<EntryReviewHistory>(entity =>
        {
            entity.key(e => e.Id);
            entity.idx(e => new { e.EntryId, e.ReviewedAt });
            entity.idx(e => new { e.EntryId, e.ReviewerId });
            entity.one(e => e.Entry)
                .many(d => d.ReviewHistory)
                .fk(e => e.EntryId)
                .onDel(DeleteBehavior.Cascade);
            entity.one(e => e.Reviewer)
                .many()
                .fk(e => e.ReviewerId)
                .onDel(DeleteBehavior.Restrict);
        });
        mb.Entity<ExpiryNotification>(entity =>
        {
            entity.key(e => e.Id);
            entity.idx(e => new { e.EntryId, e.RecipientUserId, e.DaysBeforeExpiry }).uniq();
            entity.idx(e => new { e.RecipientUserId, e.Acknowledged });
            entity.one(e => e.Entry)
                .many(d => d.ExpiryNotifications)
                .fk(e => e.EntryId)
                .onDel(DeleteBehavior.Cascade);
            entity.one(e => e.Recipient)
                .many()
                .fk(e => e.RecipientUserId)
                .onDel(DeleteBehavior.Restrict);
            entity.one(e => e.NotificationChannel)
                .many(c => c.Notifications)
                .fk(e => e.NotificationChannelId)
                .onDel(DeleteBehavior.ClientSetNull);
        });
        mb.Entity<NotificationChannel>(entity =>
        {
            entity.key(e => e.Id);
            entity.idx(e => new { e.TenantId, e.IsActive });
            entity.p(e => e.Name).req().max(200);
            entity.p(e => e.EventTypes).max(500);
            entity.one(e => e.Hub)
                .many(s => s.NotificationChannels)
                .fk(e => e.HubId)
                .onDel(DeleteBehavior.Cascade);
            entity.one(e => e.CreatedBy)
                .many()
                .fk(e => e.CreatedById)
                .onDel(DeleteBehavior.Restrict);
            entity.one(e => e.UpdatedBy)
                .many()
                .fk(e => e.UpdatedById)
                .onDel(DeleteBehavior.Restrict);
            entity.one(e => e.DeletedBy)
                .many()
                .fk(e => e.DeletedById)
                .onDel(DeleteBehavior.Restrict);
        });
        mb.Entity<ReviewChecklist>(entity =>
        {
            entity.key(e => e.Id);
            entity.idx(e => new { e.TenantId, e.HubId });
            entity.p(e => e.Name).req().max(200);
            entity.one(e => e.Hub)
                .many(s => s.ReviewChecklists)
                .fk(e => e.HubId)
                .onDel(DeleteBehavior.Cascade);
            entity.one(e => e.CreatedBy)
                .many()
                .fk(e => e.CreatedById)
                .onDel(DeleteBehavior.Restrict);
            entity.one(e => e.UpdatedBy)
                .many()
                .fk(e => e.UpdatedById)
                .onDel(DeleteBehavior.Restrict);
            entity.one(e => e.DeletedBy)
                .many()
                .fk(e => e.DeletedById)
                .onDel(DeleteBehavior.Restrict);
        });
        mb.Entity<ExpiryConfigAuditLog>(entity =>
        {
            entity.key(e => e.Id);
            entity.idx(e => new { e.EntryId, e.ChangedAt });
            entity.one(e => e.Entry)
                .many(d => d.ExpiryConfigAuditLogs)
                .fk(e => e.EntryId)
                .onDel(DeleteBehavior.Cascade);
            entity.one(e => e.ChangedBy)
                .many()
                .fk(e => e.ChangedByUserId)
                .onDel(DeleteBehavior.Restrict);
            entity.one(e => e.OldTemplate)
                .many()
                .fk(e => e.OldTemplateId)
                .onDel(DeleteBehavior.ClientSetNull);
            entity.one(e => e.NewTemplate)
                .many()
                .fk(e => e.NewTemplateId)
                .onDel(DeleteBehavior.ClientSetNull);
        });
        mb.Entity<ApiKey>(entity =>
        {
            entity.key(e => e.Id);
            entity.idx(e => e.KeyPrefix).uniq();
            entity.idx(e => new { e.TenantId, e.RevokedAt });
            entity.p(e => e.Name).req().max(200);
            entity.p(e => e.KeyPrefix).req().max(50);
            entity.p(e => e.SecretHash).req().max(100);
            entity.p(e => e.Scopes).req().max(500);
            entity.p(e => e.OpenIddictClientId).max(100);
            entity.one(e => e.CreatedBy)
                .many()
                .fk(e => e.CreatedById)
                .onDel(DeleteBehavior.Restrict);
            entity.one(e => e.RevokedBy)
                .many()
                .fk(e => e.RevokedById)
                .onDel(DeleteBehavior.Restrict);
            entity.one(e => e.AiApplication)
                .many(a => a.ApiKeys)
                .fk(e => e.AiApplicationId)
                .onDel(DeleteBehavior.SetNull);
        });
        mb.Entity<AiApplication>(entity =>
        {
            entity.key(e => e.Id);
            entity.idx(e => new { e.TenantId, e.Status });
            entity.p(e => e.Name).req().max(200);
            entity.p(e => e.Description).max(2000);
            entity.p(e => e.Purpose).max(2000);
            entity.one(e => e.Owner)
                .many()
                .fk(e => e.OwnerUserId)
                .onDel(DeleteBehavior.Restrict);
            entity.one(e => e.DefaultPolicy)
                .many()
                .fk(e => e.DefaultPolicyId)
                .onDel(DeleteBehavior.SetNull);
            entity.one(e => e.CreatedBy)
                .many()
                .fk(e => e.CreatedById)
                .onDel(DeleteBehavior.Restrict);
            entity.one(e => e.UpdatedBy)
                .many()
                .fk(e => e.UpdatedById)
                .onDel(DeleteBehavior.Restrict);
        });
        mb.Entity<AiApplicationSource>(entity =>
        {
            entity.key(e => e.Id);
            entity.idx(e => new { e.TenantId, e.AiApplicationId, e.SourceRef }).uniq();
            entity.p(e => e.SourceRef).req().max(2000);
            entity.p(e => e.Label).max(200);
            entity.one(e => e.Application)
                .many(a => a.Sources)
                .fk(e => e.AiApplicationId)
                .onDel(DeleteBehavior.Cascade);
        });
        mb.Entity<AiUsePolicy>(entity =>
        {
            entity.key(e => e.Id);
            entity.idx(e => new { e.TenantId, e.IsDefault });
            entity.p(e => e.Name).req().max(200);
            entity.p(e => e.Description).max(1000);
            entity.one(e => e.CreatedBy)
                .many()
                .fk(e => e.CreatedById)
                .onDel(DeleteBehavior.Restrict);
            entity.one(e => e.UpdatedBy)
                .many()
                .fk(e => e.UpdatedById)
                .onDel(DeleteBehavior.Restrict);
            entity.one(e => e.DeletedBy)
                .many()
                .fk(e => e.DeletedById)
                .onDel(DeleteBehavior.Restrict);
        });
        mb.Entity<Hub>()
            .one(h => h.AiUsePolicy)
            .many()
            .fk(h => h.AiUsePolicyId)
            .onDel(DeleteBehavior.SetNull);
        mb.Entity<Entry>()
            .one(e => e.AiUsePolicy)
            .many()
            .fk(e => e.AiUsePolicyId)
            .onDel(DeleteBehavior.SetNull);
        mb.Entity<EntryAiPolicyOverride>(entity =>
        {
            entity.key(e => e.Id);
            entity.idx(e => new { e.TenantId, e.EntryId }).uniq();
            entity.one(e => e.Entry)
                .wOne(d => d.AiPolicyOverride)
                .HasForeignKey<EntryAiPolicyOverride>(e => e.EntryId)
                .onDel(DeleteBehavior.Cascade);
            entity.one(e => e.LegalApprovedBy)
                .many()
                .fk(e => e.LegalApprovedById)
                .onDel(DeleteBehavior.Restrict);
        });
        mb.Entity<SensitiveDetection>(entity =>
        {
            entity.key(e => e.Id);
            entity.idx(e => new { e.TenantId, e.EntryId });
            entity.p(e => e.Detector).req().max(100);
        });
        mb.Entity<TrustDecisionRecord>(entity =>
        {
            entity.key(e => e.Id);
            entity.idx(e => new { e.TenantId, e.EvaluatedAt });
            entity.idx(e => new { e.TenantId, e.AiApplicationId, e.EvaluatedAt });
            entity.p(e => e.ActorLabel).max(200);
            entity.p(e => e.Action).max(100);
            entity.p(e => e.Channel).max(100);
        });
        mb.Entity<TrustDecisionSource>(entity =>
        {
            entity.key(e => e.Id);
            entity.idx(e => new { e.TenantId, e.DecisionRecordId });
            entity.idx(e => new { e.TenantId, e.EntryId, e.CreatedAt });
            entity.p(e => e.RequestedSourceId).req().max(2000);
            entity.p(e => e.PolicySource).max(300);
            entity.one(e => e.DecisionRecord)
                .many(r => r.Sources)
                .fk(e => e.DecisionRecordId)
                .onDel(DeleteBehavior.Cascade);
        });
        mb.Entity<TrustUsageLog>(entity =>
        {
            entity.key(e => e.Id);
            entity.idx(e => new { e.TenantId, e.LoggedAt });
            entity.p(e => e.ActorLabel).max(200);
            entity.p(e => e.Action).max(100);
            entity.p(e => e.Outcome).max(1000);
            entity.one(e => e.DecisionRecord)
                .many()
                .fk(e => e.DecisionRecordId)
                .onDel(DeleteBehavior.SetNull);
        });
        mb.Entity<FreshnessSignal>(entity =>
        {
            entity.key(e => e.Id);
            entity.idx(e => new { e.EntryId, e.SignalType }).uniq();
            entity.one(e => e.Entry)
                .many(d => d.FreshnessSignals)
                .fk(e => e.EntryId)
                .onDel(DeleteBehavior.Cascade);
            entity.p(e => e.SignalType)
                .conv<string>()
                .max(50);
        });
        mb.Entity<FreshnessScore>(entity =>
        {
            entity.key(e => e.Id);
            entity.idx(e => e.EntryId).uniq();
            entity.one(e => e.Entry)
                .wOne(d => d.FreshnessScore)
                .HasForeignKey<FreshnessScore>(e => e.EntryId)
                .onDel(DeleteBehavior.Cascade);
        });
        mb.Entity<ExternalSourceLink>(entity =>
        {
            entity.key(e => e.Id);
            entity.idx(e => new { e.TenantId, e.EntryId });
            entity.idx(e => new { e.TenantId, e.SourceType, e.LastCheckedAt });
            entity.idx(e => new { e.EntryId, e.ManagedByConnectorInstallationId });
            entity.p(e => e.Url).req().max(2000);
            entity.p(e => e.Label).max(200);
            entity.p(e => e.Selector).max(500);
            entity.p(e => e.LastObservedSignature).max(200);
            entity.p(e => e.LastChangeSummary).max(500);
            entity.p(e => e.SourceType)
                .conv<string>()
                .max(50);
            entity.p(e => e.Importance)
                .conv<string>()
                .max(30);
            entity.one(e => e.Entry)
                .many()
                .fk(e => e.EntryId)
                .onDel(DeleteBehavior.Cascade);
            entity.one(e => e.CreatedBy)
                .many()
                .fk(e => e.CreatedByUserId)
                .onDel(DeleteBehavior.Restrict);
            entity.hasM(e => e.Changes)
                .wOne(c => c.SourceLink)
                .fk(c => c.SourceLinkId)
                .onDel(DeleteBehavior.Cascade);
        });
        mb.Entity<ExternalSourceChange>(entity =>
        {
            entity.key(e => e.Id);
            entity.idx(e => new { e.SourceLinkId, e.DetectedAt });
            entity.p(e => e.ExternalRef).max(200);
            entity.p(e => e.Summary).max(500);
            entity.one(e => e.AcknowledgedBy)
                .many()
                .fk(e => e.AcknowledgedByUserId)
                .onDel(DeleteBehavior.Restrict);
        });
        mb.Entity<Entry>(entity =>
        {
            entity.one(e => e.ExpiryConfigChangedBy)
                .many()
                .fk(e => e.ExpiryConfigChangedByUserId)
                .onDel(DeleteBehavior.Restrict);
        });
        mb.Entity<TenantLanguageConfig>(entity =>
        {
            entity.key(e => e.Id);
            entity.idx(e => new { e.TenantId, e.LanguageCode }).uniq();
            entity.p(e => e.LanguageCode).req().max(10);
            entity.p(e => e.DisplayName).req().max(100);
            entity.p(e => e.TranslationProvider).max(100);
            entity.one(e => e.Tenant)
                .many(t => t.LanguageConfigs)
                .fk(e => e.TenantId)
                .onDel(DeleteBehavior.Cascade);
        });
        mb.Entity<TenantGlossary>(entity =>
        {
            entity.key(g => g.Id);
            entity.idx(g => new { g.TenantId, g.SourceLanguage, g.TargetLanguage }).uniq();
            entity.p(g => g.Name).max(256);
            entity.p(g => g.SourceLanguage).req().max(10);
            entity.p(g => g.TargetLanguage).req().max(10);
            entity.p(g => g.DeepLGlossaryId).max(64);
            entity.hasM(g => g.Entries)
                .wOne(e => e.Glossary)
                .fk(e => e.GlossaryId)
                .onDel(DeleteBehavior.Cascade);
            entity.one(g => g.Tenant)
                .many()
                .fk(g => g.TenantId)
                .onDel(DeleteBehavior.Cascade);
        });
        mb.Entity<TenantGlossaryEntry>(entity =>
        {
            entity.key(e => e.Id);
            entity.idx(e => new { e.GlossaryId, e.SourceTerm }).uniq();
            entity.p(e => e.SourceTerm).req().max(500);
            entity.p(e => e.TargetTerm).req().max(500);
        });
        mb.Entity<TenantStyleRuleList>(entity =>
        {
            entity.key(s => s.Id);
            entity.idx(s => new { s.TenantId, s.TargetLanguage }).uniq();
            entity.p(s => s.Name).max(256);
            entity.p(s => s.TargetLanguage).req().max(10);
            entity.p(s => s.DeepLStyleId).max(64);
            entity.hasM(s => s.CustomInstructions)
                .wOne(ci => ci.StyleRuleList)
                .fk(ci => ci.StyleRuleListId)
                .onDel(DeleteBehavior.Cascade);
            entity.one(s => s.Tenant)
                .many()
                .fk(s => s.TenantId)
                .onDel(DeleteBehavior.Cascade);
        });
        mb.Entity<TenantCustomInstruction>(entity =>
        {
            entity.key(ci => ci.Id);
            entity.p(ci => ci.Label).req().max(200);
            entity.p(ci => ci.Prompt).req().max(300);
            entity.p(ci => ci.SourceLanguage).max(10);
            entity.p(ci => ci.DeepLInstructionId).max(64);
        });
        mb.Entity<TenantPluginInstallation>(entity =>
        {
            entity.key(e => e.Id);
            entity.idx(e => new { e.TenantId, e.PluginId }).uniq();
            entity.p(e => e.PluginId).req().max(100);
            entity.p(e => e.Version).req().max(50);
        });
        mb.Entity<ConnectorInstallation>(entity =>
        {
            entity.key(e => e.Id);
            entity.idx(e => new { e.TenantId, e.ConnectorKey });
            entity.p(e => e.ConnectorKey).req().max(50);
            entity.p(e => e.DisplayName).max(200);
            entity.p(e => e.LastError).max(1000);
            entity.p(e => e.Status)
                .conv<string>()
                .max(30);
        });
        mb.Entity<EntryTemplate>(entity =>
        {
            entity.key(e => e.Id);
            entity.idx(e => e.ExternalId).uniq();
            entity.p(e => e.Name).req().max(200);
            entity.p(e => e.Description).max(1000);
            entity.p(e => e.Category).max(100);
            entity.p(e => e.IconName).max(50);
            entity.p(e => e.ExternalId).max(100);
            entity.p(e => e.SourceUrl).max(500);
            entity.p(e => e.TagsJson).max(1000);
            entity.hasM(e => e.Blocks)
                .wOne(b => b.Template)
                .fk(b => b.TemplateId)
                .onDel(DeleteBehavior.Cascade);
            entity.hasM(e => e.Ratings)
                .wOne(r => r.Template)
                .fk(r => r.TemplateId)
                .onDel(DeleteBehavior.Cascade);
            entity.one(e => e.CreatedBy)
                .many()
                .fk(e => e.CreatedById)
                .onDel(DeleteBehavior.Restrict);
            entity.one(e => e.UpdatedBy)
                .many()
                .fk(e => e.UpdatedById)
                .onDel(DeleteBehavior.Restrict);
        });
        mb.Entity<EntryTemplateBlock>(entity =>
        {
            entity.key(e => e.Id);
            entity.idx(e => new { e.TemplateId, e.Position });
            entity.p(e => e.BlockType).req().max(100);
        });
        mb.Entity<TemplateRating>(entity =>
        {
            entity.key(e => e.Id);
            entity.idx(e => new { e.TemplateId, e.UserId }).uniq();
            entity.one(e => e.User)
                .many()
                .fk(e => e.UserId)
                .onDel(DeleteBehavior.Cascade);
        });
        mb.Entity<TemplateUsage>(entity =>
        {
            entity.key(e => e.Id);
            entity.idx(e => new { e.TemplateId, e.UsedAt });
            entity.idx(e => new { e.TenantId, e.UsedAt });
            entity.one(e => e.Template)
                .many()
                .fk(e => e.TemplateId)
                .onDel(DeleteBehavior.Cascade);
        });
        mb.Entity<EntryTag>(entity =>
        {
            entity.key(e => new { e.EntryId, e.TagId });
            entity.one(e => e.Entry)
                .many(d => d.EntryTags)
                .fk(e => e.EntryId)
                .onDel(DeleteBehavior.Cascade);
            entity.one(e => e.Tag)
                .many(t => t.EntryTags)
                .fk(e => e.TagId)
                .onDel(DeleteBehavior.Cascade);
        });
        mb.Entity<EntryStarred>(entity =>
        {
            entity.key(e => e.Id);
            entity.idx(e => new { e.EntryId, e.UserId }).uniq();
            entity.one(e => e.Entry)
                .many()
                .fk(e => e.EntryId)
                .onDel(DeleteBehavior.Cascade);
            entity.one(e => e.User)
                .many()
                .fk(e => e.UserId)
                .onDel(DeleteBehavior.Cascade);
        });
        mb.Entity<EntryPin>(entity =>
        {
            entity.key(e => e.Id);
            entity.idx(e => e.EntryId).uniq();
            entity.one(e => e.Entry)
                .many()
                .fk(e => e.EntryId)
                .onDel(DeleteBehavior.Cascade);
            entity.one(e => e.User)
                .many()
                .fk(e => e.PinnedByUserId)
                .onDel(DeleteBehavior.Cascade);
        });
        mb.Entity<EntryWatcher>(entity =>
        {
            entity.key(e => e.Id);
            entity.idx(e => new { e.EntryId, e.UserId }).uniq();
            entity.p(e => e.WatchLevel).req().max(50);
            entity.one(e => e.Entry)
                .many()
                .fk(e => e.EntryId)
                .onDel(DeleteBehavior.Cascade);
            entity.one(e => e.User)
                .many()
                .fk(e => e.UserId)
                .onDel(DeleteBehavior.Cascade);
        });
        mb.Entity<EntryVersion>(entity =>
        {
            entity.key(e => e.Id);
            entity.idx(e => new { e.EntryId, e.VersionNumber });
            entity.one(e => e.Entry)
                .many()
                .fk(e => e.EntryId)
                .onDel(DeleteBehavior.Cascade);
            entity.one(e => e.SavedBy)
                .many()
                .fk(e => e.SavedById)
                .onDel(DeleteBehavior.Restrict);
            entity.hasM(e => e.ChangedBlocks)
                .wOne(b => b.Version)
                .fk(b => b.EntryVersionId)
                .onDel(DeleteBehavior.Cascade);
        });
        mb.Entity<EntryVersionBlock>(entity =>
        {
            entity.key(e => e.Id);
            entity.idx(e => e.EntryVersionId);
            entity.p(e => e.ChangeType).req().max(20);
        });
        mb.Entity<ShareLink>(entity =>
        {
            entity.key(e => e.Id);
            entity.idx(e => e.Token).uniq();
            entity.idx(e => e.EntryId);
            entity.p(e => e.Token).req().max(64);
            entity.one(e => e.Entry)
                .many()
                .fk(e => e.EntryId)
                .onDel(DeleteBehavior.Cascade);
            entity.one(e => e.CreatedBy)
                .many()
                .fk(e => e.CreatedById)
                .onDel(DeleteBehavior.Restrict);
        });
        mb.Entity<BlockEmbedding>(entity =>
        {
            entity.key(e => e.Id);
            entity.p(e => e.Id).ValueGeneratedOnAdd();
            entity.idx(e => new { e.EntryBlockId, e.Language }).uniq();
            entity.idx(e => new { e.EntryId, e.Language });
            entity.idx(e => e.TenantId);
            entity.p(e => e.Language).req().max(10);
            entity.p(e => e.EmbeddingModel).req().max(100);
            entity.p(e => e.ContentHash).req().max(64);
            entity.p(e => e.EmbeddingVector)
                .HasColumnType("vector(1536)");
            entity.idx(e => e.EmbeddingVector)
                .HasMethod("hnsw")
                .HasOperators("vector_cosine_ops");
            entity.one(e => e.EntryBlock)
                .many()
                .fk(e => e.EntryBlockId)
                .onDel(DeleteBehavior.Cascade);
            entity.one(e => e.Entry)
                .many()
                .fk(e => e.EntryId)
                .onDel(DeleteBehavior.Restrict);
        });
        mb.Entity<EntryEmbedding>(entity =>
        {
            entity.key(e => e.Id);
            entity.p(e => e.Id).ValueGeneratedOnAdd();
            entity.idx(e => new { e.EntryId, e.Language }).uniq();
            entity.idx(e => e.TenantId);
            entity.p(e => e.Language).req().max(10);
            entity.p(e => e.EmbeddingModel).req().max(100);
            entity.p(e => e.EmbeddingVector)
                .HasColumnType("vector(1536)");
            entity.idx(e => e.EmbeddingVector)
                .HasMethod("hnsw")
                .HasOperators("vector_cosine_ops");
            entity.one(e => e.Entry)
                .many()
                .fk(e => e.EntryId)
                .onDel(DeleteBehavior.Cascade);
        });
        mb.Entity<AgentConversation>(entity =>
        {
            entity.key(e => e.Id);
            entity.idx(e => new { e.TenantId, e.UserId });
            entity.p(e => e.Title).max(500);
            entity.one(e => e.User)
                .many()
                .fk(e => e.UserId)
                .onDel(DeleteBehavior.Cascade);
            entity.hasM(e => e.Messages)
                .wOne(m => m.Conversation)
                .fk(m => m.ConversationId)
                .onDel(DeleteBehavior.Cascade);
        });
        mb.Entity<AgentMessage>(entity =>
        {
            entity.key(e => e.Id);
            entity.idx(e => new { e.ConversationId, e.CreatedAt });
        });
        mb.Entity<TenantAIUsage>(entity =>
        {
            entity.key(e => e.Id);
            entity.idx(e => new { e.TenantId, e.PeriodStart }).uniq();
        });
        mb.Entity<AIPromptLog>(entity =>
        {
            entity.key(e => e.Id);
            entity.idx(e => new { e.TenantId, e.CreatedAt });
            entity.idx(e => new { e.TenantId, e.UserId });
            entity.p(e => e.PromptHash).req().max(64);
            entity.p(e => e.ModelUsed).req().max(100);
            entity.one(e => e.User)
                .many()
                .fk(e => e.UserId)
                .onDel(DeleteBehavior.Cascade);
        });
        mb.Entity<PersonalSpace>(entity =>
        {
            entity.key(e => e.Id);
            entity.idx(e => new { e.TenantId, e.OwnerUserId }).uniq();
            entity.one(e => e.Owner)
                .many()
                .fk(e => e.OwnerUserId)
                .onDel(DeleteBehavior.Restrict);
            entity.hasM(e => e.Notes)
                .wOne(n => n.Space)
                .fk(n => n.PersonalSpaceId)
                .onDel(DeleteBehavior.Cascade);
        });
        mb.Entity<PersonalNote>(entity =>
        {
            entity.key(e => e.Id);
            entity.idx(e => new { e.OwnerUserId, e.ParentNoteId, e.SortOrder });
            entity.idx(e => e.PersonalSpaceId);
            entity.p(e => e.Language).req().max(10);
            entity.one(e => e.Parent)
                .many(p => p.Children)
                .fk(e => e.ParentNoteId)
                .onDel(DeleteBehavior.Restrict);
            entity.hasM(e => e.Blocks)
                .wOne(b => b.Note)
                .fk(b => b.PersonalNoteId)
                .onDel(DeleteBehavior.Cascade);
            entity.hasM(e => e.Shares)
                .wOne(s => s.Note)
                .fk(s => s.PersonalNoteId)
                .onDel(DeleteBehavior.Cascade);
            entity.hasM(e => e.NoteTags)
                .wOne(t => t.Note)
                .fk(t => t.PersonalNoteId)
                .onDel(DeleteBehavior.Cascade);
        });
        mb.Entity<PersonalNoteBlock>(entity =>
        {
            entity.key(e => e.Id);
            entity.idx(e => new { e.PersonalNoteId, e.Position });
            entity.p(e => e.BlockType).req().max(50);
            entity.p(e => e.ContentHash).req().max(64);
        });
        mb.Entity<PersonalNoteShare>(entity =>
        {
            entity.key(e => e.Id);
            entity.idx(e => new { e.PersonalNoteId, e.UserId, e.GroupId });
            entity.one(e => e.User)
                .many()
                .fk(e => e.UserId)
                .onDel(DeleteBehavior.Cascade);
            entity.one(e => e.Group)
                .many()
                .fk(e => e.GroupId)
                .onDel(DeleteBehavior.ClientCascade);
        });
        mb.Entity<PersonalNoteTag>(entity =>
        {
            entity.key(e => new { e.PersonalNoteId, e.TagId });
            entity.one(e => e.Tag)
                .many()
                .fk(e => e.TagId)
                .onDel(DeleteBehavior.Cascade);
        });
        mb.Entity<PersonalNoteEmbedding>(entity =>
        {
            entity.key(e => e.Id);
            entity.p(e => e.Id).ValueGeneratedOnAdd();
            entity.idx(e => new { e.PersonalNoteId, e.Language }).uniq();
            entity.idx(e => e.OwnerUserId);
            entity.p(e => e.Language).req().max(10);
            entity.p(e => e.EmbeddingModel).req().max(100);
            entity.p(e => e.ContentHash).req().max(64);
            entity.p(e => e.EmbeddingVector).HasColumnType("vector(1536)");
            entity.idx(e => e.EmbeddingVector)
                .HasMethod("hnsw")
                .HasOperators("vector_cosine_ops");
            entity.one(e => e.Note)
                .many()
                .fk(e => e.PersonalNoteId)
                .onDel(DeleteBehavior.Cascade);
        });
        mb.Entity<UserAIUsage>(entity =>
        {
            entity.key(e => e.Id);
            entity.idx(e => new { e.TenantId, e.UserId, e.PeriodStart }).uniq();
            entity.one(e => e.User)
                .many()
                .fk(e => e.UserId)
                .onDel(DeleteBehavior.Cascade);
        });
        if (!Database.IsNpgsql())
        {
            var vectorConverter = new ValueConverter<Vector, string>(
                v => string.Join(',', v.ToArray()),
                s => new Vector(s.Length == 0
                    ? Array.Empty<float>()
                    : s.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(float.Parse).ToArray()));
            mb.Entity<BlockEmbedding>().p(e => e.EmbeddingVector).HasConversion(vectorConverter);
            mb.Entity<EntryEmbedding>().p(e => e.EmbeddingVector).HasConversion(vectorConverter);
            mb.Entity<PersonalNoteEmbedding>().p(e => e.EmbeddingVector).HasConversion(vectorConverter);
        }
    }
    private static readonly MethodInfo s_applyFilterMethod =
        typeof(RasepiDbContext)
            .GetMethod(nameof(ApplyTenantFilterGeneric), BindingFlags.NonPublic | BindingFlags.Instance)!;
    private void ApplyTenantFilters(ModelBuilder mb)
    {
        foreach (var entityType in mb.Model.GetEntityTypes())
        {
            if (typeof(ITenantScoped).IsAssignableFrom(entityType.ClrType))
            {
                s_applyFilterMethod
                    .MakeGenericMethod(entityType.ClrType)
                    .Invoke(this, [mb]);
            }
        }
    }
    private void ApplyTenantFilterGeneric<T>(ModelBuilder mb)
        where T : class, ITenantScoped
    {
        mb.qf<T>(e => e.TenantId == _tenantContext.TenantId);
    }
    public override int SaveChanges()
    {
        EnforceTenantOnEntries();
        return base.SaveChanges();
    }
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        EnforceTenantOnEntries();
        return base.SaveChangesAsync(cancellationToken);
    }
    private void EnforceTenantOnEntries()
    {
        if (!_tenantContext.IsResolved)
            return;
        foreach (var entry in ChangeTracker.Entries<ITenantScoped>())
        {
            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                if (entry.Entity.TenantId == Guid.Empty)
                {
                    entry.Entity.TenantId = _tenantContext.TenantId;
                }
                else if (entry.Entity.TenantId != _tenantContext.TenantId)
                {
                    throw new IOE(
                        $"Cross-tenant write blocked. " +
                        $"Entity TenantId={entry.Entity.TenantId} does not match " +
                        $"current TenantContext.TenantId={_tenantContext.TenantId}. " +
                        $"Entity type: {entry.Entity.GetType().Name}");
                }
            }
        }
    }
}
