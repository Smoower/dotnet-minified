
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

    // Multilingual
    public DbSet<TenantLanguageConfig> TenantLanguageConfigs => Set<TenantLanguageConfig>();

    // Glossary
    public DbSet<TenantGlossary> TenantGlossaries => Set<TenantGlossary>();
    public DbSet<TenantGlossaryEntry> TenantGlossaryEntries => Set<TenantGlossaryEntry>();

    // Style rules
    public DbSet<TenantStyleRuleList> TenantStyleRuleLists => Set<TenantStyleRuleList>();
    public DbSet<TenantCustomInstruction> TenantCustomInstructions => Set<TenantCustomInstruction>();

    // Plugin platform
    public DbSet<TenantPluginInstallation> TenantPluginInstallations => Set<TenantPluginInstallation>();

    // Connectors (Sidecar Mode) — external doc-platform installations per tenant
    public DbSet<ConnectorInstallation> ConnectorInstallations => Set<ConnectorInstallation>();

    // Templates
    public DbSet<EntryTemplate> EntryTemplates => Set<EntryTemplate>();
    public DbSet<EntryTemplateBlock> EntryTemplateBlocks => Set<EntryTemplateBlock>();
    public DbSet<TemplateRating> TemplateRatings => Set<TemplateRating>();
    public DbSet<TemplateUsage> TemplateUsages => Set<TemplateUsage>();

    // Translation usage history
    public DbSet<TranslationHistory> TranslationHistories => Set<TranslationHistory>();

    // Entry user engagement
    public DbSet<EntryStarred> EntryStarreds => Set<EntryStarred>();
    public DbSet<EntryPin> EntryPins => Set<EntryPin>();
    public DbSet<EntryWatcher> EntryWatchers => Set<EntryWatcher>();
    public DbSet<EntryVersion> EntryVersions => Set<EntryVersion>();
    public DbSet<EntryVersionBlock> EntryVersionBlocks => Set<EntryVersionBlock>();

    // Freshness / Knowledge Freshness Engine
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

    // AI / Search / Agent
    public DbSet<BlockEmbedding> BlockEmbeddings => Set<BlockEmbedding>();
    public DbSet<EntryEmbedding> EntryEmbeddings => Set<EntryEmbedding>();
    public DbSet<AgentConversation> AgentConversations => Set<AgentConversation>();
    public DbSet<AgentMessage> AgentMessages => Set<AgentMessage>();
    public DbSet<TenantAIUsage> TenantAIUsages => Set<TenantAIUsage>();
    public DbSet<AIPromptLog> AIPromptLogs => Set<AIPromptLog>();

    // Personal Spaces — private, encrypted per-user notes
    public DbSet<PersonalSpace> PersonalSpaces => Set<PersonalSpace>();
    public DbSet<PersonalNote> PersonalNotes => Set<PersonalNote>();
    public DbSet<PersonalNoteBlock> PersonalNoteBlocks => Set<PersonalNoteBlock>();
    public DbSet<PersonalNoteShare> PersonalNoteShares => Set<PersonalNoteShare>();
    public DbSet<PersonalNoteTag> PersonalNoteTags => Set<PersonalNoteTag>();
    public DbSet<PersonalNoteEmbedding> PersonalNoteEmbeddings => Set<PersonalNoteEmbedding>();
    public DbSet<UserAIUsage> UserAIUsages => Set<UserAIUsage>();

    // Billing
    public DbSet<TenantSubscription> TenantSubscriptions => Set<TenantSubscription>();

    // Share links
    public DbSet<ShareLink> ShareLinks => Set<ShareLink>();

    // Universal audit log — one row per domain event, written by EventConsumerWorker.
    public DbSet<AuditLogEntry> AuditLogEntries => Set<AuditLogEntry>();

    // Trust engine — AI-use policies and the trust decision ledger
    public DbSet<ApiKey> ApiKeys => Set<ApiKey>();
    public DbSet<AiApplication> AiApplications => Set<AiApplication>();
    public DbSet<AiApplicationSource> AiApplicationSources => Set<AiApplicationSource>();
    public DbSet<AiUsePolicy> AiUsePolicies => Set<AiUsePolicy>();
    public DbSet<EntryAiPolicyOverride> EntryAiPolicyOverrides => Set<EntryAiPolicyOverride>();
    public DbSet<SensitiveDetection> SensitiveDetections => Set<SensitiveDetection>();
    public DbSet<TrustDecisionRecord> TrustDecisionRecords => Set<TrustDecisionRecord>();
    public DbSet<TrustDecisionSource> TrustDecisionSources => Set<TrustDecisionSource>();
    public DbSet<TrustUsageLog> TrustUsageLogs => Set<TrustUsageLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // pgvector — required so the migration creates `CREATE EXTENSION vector`
        // before any vector(1536) column or HNSW index is built. No-op on the
        // in-memory provider used by tests.
        if (Database.IsNpgsql())
            modelBuilder.HasPostgresExtension("vector");

        // OpenIddict entity tables (Applications, Authorizations, Scopes, Tokens)
        modelBuilder.UseOpenIddict();

        // ----------------------------------------------------------------
        // Structural entity configurations — applied from this assembly.
        // Plugins contribute IEntityTypeConfiguration<T> classes inside
        // their own folders; this single call discovers and applies them
        // all without the DbContext needing to know about each plugin.
        // ----------------------------------------------------------------
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);

        // ----------------------------------------------------------------
        // Tenant query filters — applied automatically for every entity
        // type in the model that implements ITenantScoped (Tier 1).
        // Plugins that implement ITenantScoped get their filter for free;
        // no DbContext changes required when a new plugin is added.
        //
        // Tier 2 — child entities filtered via navigation to their root:
        //   (kept below — cannot be automated without knowing the path)
        //
        // Note: User is platform-level — deliberately not filtered.
        // ----------------------------------------------------------------
        ApplyTenantFilters(modelBuilder);

        // Tier 2 — navigation-based filters
        modelBuilder.Entity<HubMembership>()
            .HasQueryFilter(e => e.Hub.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<HubTag>()
            .HasQueryFilter(e => e.Hub.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<GroupMembership>()
            .HasQueryFilter(e => e.Group.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<EntryBlock>()
            .HasQueryFilter(e => e.Entry.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<TranslationBlock>()
            .HasQueryFilter(e => e.Entry.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<EntryTranslation>()
            .HasQueryFilter(e => e.Entry.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<EditSession>()
            .HasQueryFilter(e => e.Entry.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<EntryPermission>()
            .HasQueryFilter(e => e.Entry.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<EntryTag>()
            .HasQueryFilter(e => e.Entry.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<EntryActivity>()
            .HasQueryFilter(e => e.Entry.TenantId == _tenantContext.TenantId);

        // Freshness — child-of-Entry filters
        modelBuilder.Entity<EntryReviewHistory>()
            .HasQueryFilter(e => e.Entry.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<ExpiryNotification>()
            .HasQueryFilter(e => e.Entry.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<ExpiryConfigAuditLog>()
            .HasQueryFilter(e => e.Entry.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<FreshnessSignal>()
            .HasQueryFilter(e => e.Entry.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<FreshnessScore>()
            .HasQueryFilter(e => e.Entry.TenantId == _tenantContext.TenantId);

        // ExternalSourceLink is ITenantScoped (caught by ApplyTenantFilters above);
        // ExternalSourceChange is also ITenantScoped, but we add a belt-and-braces
        // filter via navigation so that even if the direct TenantId field drifts
        // out of sync, queries still respect the parent link's tenant.
        modelBuilder.Entity<ExternalSourceChange>()
            .HasQueryFilter(c => c.SourceLink.TenantId == _tenantContext.TenantId);

        // AI — child-of-Entry / child-of-Conversation filters (Tier 2)
        modelBuilder.Entity<BlockEmbedding>()
            .HasQueryFilter(e => e.Entry.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<EntryEmbedding>()
            .HasQueryFilter(e => e.Entry.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<AgentMessage>()
            .HasQueryFilter(e => e.Conversation.TenantId == _tenantContext.TenantId);

        // ----------------------------------------------------------------
        // Personal Spaces — owner-only query filters.
        //
        // These OVERRIDE the tenant-only filter that ApplyTenantFilters stamps on
        // the ITenantScoped personal entities. The default visibility is the owner
        // and nobody else — not tenant admins, not global admins. Access to notes
        // SHARED with the current user is an explicit, audited code path in
        // PersonalAccessService using IgnoreQueryFilters; it deliberately does not
        // widen this default filter.
        // ----------------------------------------------------------------
        modelBuilder.Entity<PersonalSpace>()
            .HasQueryFilter(e => e.TenantId == _tenantContext.TenantId
                && e.OwnerUserId == _tenantContext.UserId);
        modelBuilder.Entity<PersonalNote>()
            .HasQueryFilter(e => e.TenantId == _tenantContext.TenantId
                && e.OwnerUserId == _tenantContext.UserId
                && e.DeletedAt == null);
        modelBuilder.Entity<PersonalNoteBlock>()
            .HasQueryFilter(e => e.Note.TenantId == _tenantContext.TenantId
                && e.Note.OwnerUserId == _tenantContext.UserId);
        modelBuilder.Entity<PersonalNoteShare>()
            .HasQueryFilter(e => e.Note.TenantId == _tenantContext.TenantId
                && e.Note.OwnerUserId == _tenantContext.UserId);
        modelBuilder.Entity<PersonalNoteTag>()
            .HasQueryFilter(e => e.Note.TenantId == _tenantContext.TenantId
                && e.Note.OwnerUserId == _tenantContext.UserId);
        modelBuilder.Entity<PersonalNoteEmbedding>()
            .HasQueryFilter(e => e.TenantId == _tenantContext.TenantId
                && e.OwnerUserId == _tenantContext.UserId);
        modelBuilder.Entity<UserAIUsage>()
            .HasQueryFilter(e => e.TenantId == _tenantContext.TenantId
                && e.UserId == _tenantContext.UserId);

        // Glossary — child entity filters (TenantGlossary is ITenantScoped, caught by ApplyTenantFilters)
        modelBuilder.Entity<TenantGlossaryEntry>()
            .HasQueryFilter(e => e.Glossary.TenantId == _tenantContext.TenantId);

        // Style rules — child entity filters (TenantStyleRuleList is ITenantScoped, caught by ApplyTenantFilters)
        modelBuilder.Entity<TenantCustomInstruction>()
            .HasQueryFilter(e => e.StyleRuleList.TenantId == _tenantContext.TenantId);

        // Entry user engagement — child-of-Entry filters
        modelBuilder.Entity<EntryStarred>()
            .HasQueryFilter(e => e.Entry.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<EntryWatcher>()
            .HasQueryFilter(e => e.Entry.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<EntryVersion>()
            .HasQueryFilter(e => e.Entry.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<EntryVersionBlock>()
            .HasQueryFilter(b => b.Version.Entry.TenantId == _tenantContext.TenantId);

        // Share links — child-of-Entry filter
        modelBuilder.Entity<ShareLink>()
            .HasQueryFilter(l => l.Entry!.TenantId == _tenantContext.TenantId);

        // Templates — child-of-EntryTemplate filters
        // EntryTemplate is ITenantScoped (caught by ApplyTenantFilters).
        // TemplateRating/TemplateUsage are filtered via navigation to their template.
        modelBuilder.Entity<TemplateRating>()
            .HasQueryFilter(r => r.Template.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<EntryTemplateBlock>()
            .HasQueryFilter(b => b.Template.TenantId == _tenantContext.TenantId);
        // TemplateUsage records the consuming tenant directly;
        // we filter by that so each tenant only sees its own usage rows.
        modelBuilder.Entity<TemplateUsage>()
            .HasQueryFilter(u => u.TenantId == _tenantContext.TenantId);

        // Tenant configuration
        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.Property(e => e.Slug).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);

            entity.HasMany(e => e.TenantMemberships)
                .WithOne(m => m.Tenant)
                .HasForeignKey(m => m.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Hubs)
                .WithOne(s => s.Tenant)
                .HasForeignKey(s => s.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Groups)
                .WithOne()
                .HasForeignKey(g => g.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Subscription)
                .WithOne(s => s.Tenant)
                .HasForeignKey<TenantSubscription>(s => s.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // TenantSubscription configuration
        modelBuilder.Entity<TenantSubscription>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.TenantId).IsUnique();
            // Quoted identifiers: Postgres folds unquoted names to lowercase, but EF
            // creates the columns with their PascalCase names, so the filter must quote them.
            entity.HasIndex(e => e.StripeCustomerId).IsUnique().HasFilter("\"StripeCustomerId\" IS NOT NULL");
            entity.HasIndex(e => e.StripeSubscriptionId).IsUnique().HasFilter("\"StripeSubscriptionId\" IS NOT NULL");
            entity.Property(e => e.StripeCustomerId).HasMaxLength(255);
            entity.Property(e => e.StripeSubscriptionId).HasMaxLength(255);
            entity.Property(e => e.StripePriceId).HasMaxLength(255);
        });

        // TenantMembership configuration
        modelBuilder.Entity<TenantMembership>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.UserId }).IsUnique();

            // M3: A user can own at most one tenant. Enforced at DB level.
            entity.HasIndex(e => e.UserId)
                .IsUnique()
                .HasFilter($"\"Role\" = {(int)Rasepi.Shared.Models.Tenants.TenantRole.Owner}")
                .HasDatabaseName("IX_TenantMembership_OneOwnerPerUser");

            entity.HasOne(e => e.Tenant)
                .WithMany(t => t.TenantMemberships)
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany(u => u.TenantMemberships)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict); // SQL Server: cannot have two cascade paths to TenantMemberships

            entity.HasOne(e => e.InvitedBy)
                .WithMany()
                .HasForeignKey(e => e.InvitedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull); // SQL Server: SET NULL not allowed when multiple FKs from Users→TenantMemberships; EF handles null in-memory
        });

        // TenantInvitation configuration
        modelBuilder.Entity<TenantInvitation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.Email });
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);

            entity.HasOne(e => e.Tenant)
                .WithMany()
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.InvitedBy)
                .WithMany()
                .HasForeignKey(e => e.InvitedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.AcceptedBy)
                .WithMany()
                .HasForeignKey(e => e.AcceptedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        // Hub configuration
        modelBuilder.Entity<Hub>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.Key }).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Key).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Icon).HasMaxLength(100);
            entity.Property(e => e.IconColor).HasMaxLength(20);

            // Sidecar Mode (§19.5) — origin discriminator + connector backlink.
            // Default "Native" so existing/native rows are always a valid enum string.
            entity.Property(e => e.Origin)
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasDefaultValue(HubOrigin.Native);
            entity.HasIndex(e => new { e.TenantId, e.Origin });
            entity.HasOne(e => e.ConnectorInstallation)
                .WithMany()
                .HasForeignKey(e => e.ConnectorInstallationId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Owner)
                .WithMany()
                .HasForeignKey(e => e.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(e => e.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.UpdatedBy)
                .WithMany()
                .HasForeignKey(e => e.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.DeletedBy)
                .WithMany()
                .HasForeignKey(e => e.DeletedById)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Entry configuration
        modelBuilder.Entity<Entry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.HubId, e.Key }).IsUnique();
            entity.Property(e => e.OriginalLanguage).IsRequired().HasMaxLength(10);

            // Sidecar Mode (§2) — origin discriminator + connector projection fields.
            // Default "Native" so existing/native rows are always a valid enum string.
            entity.Property(e => e.Origin)
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasDefaultValue(EntryOrigin.Native);
            entity.Property(e => e.ConnectorExternalId).HasMaxLength(500);
            entity.Property(e => e.ConnectorExternalUrl).HasMaxLength(2000);
            entity.Property(e => e.ConnectorExternalVersion).HasMaxLength(200);
            entity.Property(e => e.ConnectorTitle).HasMaxLength(500);
            // Fast lookup when a webhook/poll resolves an external id back to its shadow entry.
            entity.HasIndex(e => new { e.ConnectorInstallationId, e.ConnectorExternalId });
            entity.HasOne(e => e.ConnectorInstallation)
                .WithMany()
                .HasForeignKey(e => e.ConnectorInstallationId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Hub)
                .WithMany(s => s.Entries)
                .HasForeignKey(e => e.HubId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Owner)
                .WithMany()
                .HasForeignKey(e => e.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(e => e.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.UpdatedBy)
                .WithMany()
                .HasForeignKey(e => e.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.DeletedBy)
                .WithMany()
                .HasForeignKey(e => e.DeletedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(e => e.SortOrder).HasDefaultValue(0);
            entity.HasIndex(e => new { e.HubId, e.SortOrder });
        });

        // EntryBlock configuration
        modelBuilder.Entity<EntryBlock>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.EntryId, e.Language, e.Position });
            entity.Property(e => e.Language).IsRequired().HasMaxLength(10);
            entity.Property(e => e.BlockType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ContentHash).IsRequired().HasMaxLength(64);
            // Sidecar Mode (§10) — connector-derived block key for shadow blocks (null for native).
            entity.Property(e => e.ConnectorBlockKey).HasMaxLength(1000);

            entity.HasOne(e => e.Entry)
                .WithMany()
                .HasForeignKey(e => e.EntryId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(e => e.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.UpdatedBy)
                .WithMany()
                .HasForeignKey(e => e.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.DeletedBy)
                .WithMany()
                .HasForeignKey(e => e.DeletedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.BlockTypeHeuristic)
                .WithMany()
                .HasForeignKey(e => e.BlockTypeHeuristicId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // TranslationBlock configuration
        modelBuilder.Entity<TranslationBlock>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.SourceBlockId, e.Language }).IsUnique();
            entity.HasIndex(e => new { e.EntryId, e.Language, e.Position });
            entity.Property(e => e.Language).IsRequired().HasMaxLength(10);
            entity.Property(e => e.BlockType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.SourceBlockType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.SourceContentHash).IsRequired().HasMaxLength(64);
            
            entity.HasOne(e => e.SourceBlock)
                .WithMany(b => b.Translations)
                .HasForeignKey(e => e.SourceBlockId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Entry)
                .WithMany()
                .HasForeignKey(e => e.EntryId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.TranslatedBy)
                .WithMany()
                .HasForeignKey(e => e.TranslatedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(e => e.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.UpdatedBy)
                .WithMany()
                .HasForeignKey(e => e.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.DeletedBy)
                .WithMany()
                .HasForeignKey(e => e.DeletedById)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // EntryTranslation configuration
        modelBuilder.Entity<EntryTranslation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.EntryId, e.Language }).IsUnique();
            entity.Property(e => e.Language).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
            
            entity.HasOne(e => e.Entry)
                .WithMany(d => d.Translations)
                .HasForeignKey(e => e.EntryId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(e => e.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.UpdatedBy)
                .WithMany()
                .HasForeignKey(e => e.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.DeletedBy)
                .WithMany()
                .HasForeignKey(e => e.DeletedById)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // User configuration — platform-level, not tenant-scoped.
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
        });

        // EditSession configuration
        modelBuilder.Entity<EditSession>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.EntryId, e.Language, e.UserId });
            
            entity.HasOne(e => e.Entry)
                .WithMany(d => d.EditSessions)
                .HasForeignKey(e => e.EntryId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Group configuration
        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.Name }).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);

            entity.HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(e => e.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.UpdatedBy)
                .WithMany()
                .HasForeignKey(e => e.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.DeletedBy)
                .WithMany()
                .HasForeignKey(e => e.DeletedById)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // GroupMembership configuration
        modelBuilder.Entity<GroupMembership>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.GroupId, e.UserId }).IsUnique();
            
            entity.HasOne(e => e.Group)
                .WithMany(g => g.Members)
                .HasForeignKey(e => e.GroupId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.User)
                .WithMany(u => u.GroupMemberships)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // HubMembership configuration
        modelBuilder.Entity<HubMembership>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.HubId, e.UserId, e.GroupId });
            
            entity.HasOne(e => e.Hub)
                .WithMany(s => s.Memberships)
                .HasForeignKey(e => e.HubId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.User)
                .WithMany(u => u.HubMemberships)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Group)
                .WithMany(g => g.HubMemberships)
                .HasForeignKey(e => e.GroupId)
                .OnDelete(DeleteBehavior.ClientCascade); // SQL Server: GroupId + HubId both cascade from Tenant; EF handles group-driven deletions in-memory
        });

        // EntryPermission configuration
        modelBuilder.Entity<EntryPermission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.EntryId, e.UserId, e.GroupId });
            
            entity.HasOne(e => e.Entry)
                .WithMany(d => d.Permissions)
                .HasForeignKey(e => e.EntryId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.User)
                .WithMany(u => u.EntryPermissions)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Group)
                .WithMany(g => g.EntryPermissions)
                .HasForeignKey(e => e.GroupId)
                .OnDelete(DeleteBehavior.ClientCascade); // SQL Server: GroupId + EntryId both cascade from Tenant; EF handles group-driven deletions in-memory
        });

        // Category configuration
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            // Unique per tenant — two tenants can have categories with the same name
            entity.HasIndex(e => new { e.TenantId, e.Name }).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Color).HasMaxLength(7);
            entity.Property(e => e.Icon).HasMaxLength(50);
        });

        // BlockTypeHeuristic configuration
        modelBuilder.Entity<BlockTypeHeuristic>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.Name }).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Color).HasMaxLength(7);
            entity.Property(e => e.Icon).HasMaxLength(50);
        });

        // Tag configuration
        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.Id);
            // Unique per tenant — two tenants can have tags with the same name
            entity.HasIndex(e => new { e.TenantId, e.Name }).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Color).HasMaxLength(7);
        });

        // HubTag configuration (many-to-many)
        modelBuilder.Entity<HubTag>(entity =>
        {
            entity.HasKey(e => new { e.HubId, e.TagId });
            
            entity.HasOne(e => e.Hub)
                .WithMany(s => s.HubTags)
                .HasForeignKey(e => e.HubId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Tag)
                .WithMany(t => t.HubTags)
                .HasForeignKey(e => e.TagId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // EntryActivity configuration
        modelBuilder.Entity<EntryActivity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.EntryId, e.OccurredAt });
            entity.HasIndex(e => new { e.EntryId, e.EventType });
            entity.Property(e => e.Language).HasMaxLength(10);
            entity.Property(e => e.Metadata).HasMaxLength(2000);

            entity.HasOne(e => e.Entry)
                .WithMany()
                .HasForeignKey(e => e.EntryId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<TranslationHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.OccurredAt });
            entity.HasIndex(e => new { e.TenantId, e.TargetLanguage });
            entity.Property(e => e.SourceLanguage).IsRequired().HasMaxLength(20);
            entity.Property(e => e.TargetLanguage).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Provider).HasMaxLength(100);
            entity.Property(e => e.ErrorMessage).HasMaxLength(1000);
        });

        // ----------------------------------------------------------------
        // Freshness entity configurations
        // ----------------------------------------------------------------

        modelBuilder.Entity<ExpiryTemplate>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.IsDefault });
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.WarningThresholds).HasMaxLength(100);
            entity.Property(e => e.Color).HasMaxLength(20);
            entity.Property(e => e.Icon).HasMaxLength(50);

            entity.HasMany(e => e.Entries)
                .WithOne(d => d.ExpiryTemplate)
                .HasForeignKey(d => d.ExpiryTemplateId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(e => e.HubsUsingAsDefault)
                .WithOne(s => s.DefaultExpiryTemplate)
                .HasForeignKey(s => s.DefaultExpiryTemplateId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(e => e.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.UpdatedBy)
                .WithMany()
                .HasForeignKey(e => e.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.DeletedBy)
                .WithMany()
                .HasForeignKey(e => e.DeletedById)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<EntryReviewHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.EntryId, e.ReviewedAt });
            entity.HasIndex(e => new { e.EntryId, e.ReviewerId });

            entity.HasOne(e => e.Entry)
                .WithMany(d => d.ReviewHistory)
                .HasForeignKey(e => e.EntryId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Reviewer)
                .WithMany()
                .HasForeignKey(e => e.ReviewerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ExpiryNotification>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.EntryId, e.RecipientUserId, e.DaysBeforeExpiry }).IsUnique();
            entity.HasIndex(e => new { e.RecipientUserId, e.Acknowledged });

            entity.HasOne(e => e.Entry)
                .WithMany(d => d.ExpiryNotifications)
                .HasForeignKey(e => e.EntryId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Recipient)
                .WithMany()
                .HasForeignKey(e => e.RecipientUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.NotificationChannel)
                .WithMany(c => c.Notifications)
                .HasForeignKey(e => e.NotificationChannelId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<NotificationChannel>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.IsActive });
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.EventTypes).HasMaxLength(500);

            entity.HasOne(e => e.Hub)
                .WithMany(s => s.NotificationChannels)
                .HasForeignKey(e => e.HubId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(e => e.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.UpdatedBy)
                .WithMany()
                .HasForeignKey(e => e.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.DeletedBy)
                .WithMany()
                .HasForeignKey(e => e.DeletedById)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ReviewChecklist>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.HubId });
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);

            entity.HasOne(e => e.Hub)
                .WithMany(s => s.ReviewChecklists)
                .HasForeignKey(e => e.HubId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(e => e.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.UpdatedBy)
                .WithMany()
                .HasForeignKey(e => e.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.DeletedBy)
                .WithMany()
                .HasForeignKey(e => e.DeletedById)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ExpiryConfigAuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.EntryId, e.ChangedAt });

            entity.HasOne(e => e.Entry)
                .WithMany(d => d.ExpiryConfigAuditLogs)
                .HasForeignKey(e => e.EntryId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ChangedBy)
                .WithMany()
                .HasForeignKey(e => e.ChangedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.OldTemplate)
                .WithMany()
                .HasForeignKey(e => e.OldTemplateId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(e => e.NewTemplate)
                .WithMany()
                .HasForeignKey(e => e.NewTemplateId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        // ----------------------------------------------------------------
        // Trust engine entity configurations
        // ----------------------------------------------------------------

        modelBuilder.Entity<ApiKey>(entity =>
        {
            entity.HasKey(e => e.Id);
            // Platform-wide unique: the auth handler looks keys up by prefix
            // before any tenant context exists.
            entity.HasIndex(e => e.KeyPrefix).IsUnique();
            entity.HasIndex(e => new { e.TenantId, e.RevokedAt });
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.KeyPrefix).IsRequired().HasMaxLength(50);
            entity.Property(e => e.SecretHash).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Scopes).IsRequired().HasMaxLength(500);
            entity.Property(e => e.OpenIddictClientId).HasMaxLength(100);

            entity.HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(e => e.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.RevokedBy)
                .WithMany()
                .HasForeignKey(e => e.RevokedById)
                .OnDelete(DeleteBehavior.Restrict);

            // AI App Registry: a key may belong to a registered application.
            // SetNull so deleting an app leaves its keys standalone, not orphaned.
            entity.HasOne(e => e.AiApplication)
                .WithMany(a => a.ApiKeys)
                .HasForeignKey(e => e.AiApplicationId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<AiApplication>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.Status });
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.Purpose).HasMaxLength(2000);

            entity.HasOne(e => e.Owner)
                .WithMany()
                .HasForeignKey(e => e.OwnerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.DefaultPolicy)
                .WithMany()
                .HasForeignKey(e => e.DefaultPolicyId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(e => e.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.UpdatedBy)
                .WithMany()
                .HasForeignKey(e => e.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AiApplicationSource>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.AiApplicationId, e.SourceRef }).IsUnique();
            entity.Property(e => e.SourceRef).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.Label).HasMaxLength(200);

            entity.HasOne(e => e.Application)
                .WithMany(a => a.Sources)
                .HasForeignKey(e => e.AiApplicationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AiUsePolicy>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.IsDefault });
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);

            entity.HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(e => e.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.UpdatedBy)
                .WithMany()
                .HasForeignKey(e => e.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.DeletedBy)
                .WithMany()
                .HasForeignKey(e => e.DeletedById)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Hub/Entry → AiUsePolicy references. SetNull so deleting a policy
        // gracefully falls entries back through the resolution chain.
        modelBuilder.Entity<Hub>()
            .HasOne(h => h.AiUsePolicy)
            .WithMany()
            .HasForeignKey(h => h.AiUsePolicyId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Entry>()
            .HasOne(e => e.AiUsePolicy)
            .WithMany()
            .HasForeignKey(e => e.AiUsePolicyId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<EntryAiPolicyOverride>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.EntryId }).IsUnique();

            entity.HasOne(e => e.Entry)
                .WithOne(d => d.AiPolicyOverride)
                .HasForeignKey<EntryAiPolicyOverride>(e => e.EntryId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.LegalApprovedBy)
                .WithMany()
                .HasForeignKey(e => e.LegalApprovedById)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SensitiveDetection>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.EntryId });
            entity.Property(e => e.Detector).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<TrustDecisionRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.EvaluatedAt });
            // Per-app reporting (Phase 2) filters decisions by application over a window.
            entity.HasIndex(e => new { e.TenantId, e.AiApplicationId, e.EvaluatedAt });
            entity.Property(e => e.ActorLabel).HasMaxLength(200);
            entity.Property(e => e.Action).HasMaxLength(100);
            entity.Property(e => e.Channel).HasMaxLength(100);
        });

        modelBuilder.Entity<TrustDecisionSource>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.DecisionRecordId });
            entity.HasIndex(e => new { e.TenantId, e.EntryId, e.CreatedAt });
            entity.Property(e => e.RequestedSourceId).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.PolicySource).HasMaxLength(300);

            entity.HasOne(e => e.DecisionRecord)
                .WithMany(r => r.Sources)
                .HasForeignKey(e => e.DecisionRecordId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TrustUsageLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.LoggedAt });
            entity.Property(e => e.ActorLabel).HasMaxLength(200);
            entity.Property(e => e.Action).HasMaxLength(100);
            entity.Property(e => e.Outcome).HasMaxLength(1000);

            entity.HasOne(e => e.DecisionRecord)
                .WithMany()
                .HasForeignKey(e => e.DecisionRecordId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Freshness scoring entity configurations
        modelBuilder.Entity<FreshnessSignal>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.EntryId, e.SignalType }).IsUnique();

            entity.HasOne(e => e.Entry)
                .WithMany(d => d.FreshnessSignals)
                .HasForeignKey(e => e.EntryId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.SignalType)
                .HasConversion<string>()
                .HasMaxLength(50);
        });

        modelBuilder.Entity<FreshnessScore>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.EntryId).IsUnique();

            entity.HasOne(e => e.Entry)
                .WithOne(d => d.FreshnessScore)
                .HasForeignKey<FreshnessScore>(e => e.EntryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ExternalSourceLink>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.EntryId });
            entity.HasIndex(e => new { e.TenantId, e.SourceType, e.LastCheckedAt });
            // Sidecar Mode (§21.2) — fast lookup of connector-managed links during reconciliation.
            entity.HasIndex(e => new { e.EntryId, e.ManagedByConnectorInstallationId });
            entity.Property(e => e.Url).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.Label).HasMaxLength(200);
            entity.Property(e => e.Selector).HasMaxLength(500);
            entity.Property(e => e.LastObservedSignature).HasMaxLength(200);
            entity.Property(e => e.LastChangeSummary).HasMaxLength(500);
            entity.Property(e => e.SourceType)
                .HasConversion<string>()
                .HasMaxLength(50);
            entity.Property(e => e.Importance)
                .HasConversion<string>()
                .HasMaxLength(30);

            entity.HasOne(e => e.Entry)
                .WithMany()
                .HasForeignKey(e => e.EntryId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(e => e.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.Changes)
                .WithOne(c => c.SourceLink)
                .HasForeignKey(c => c.SourceLinkId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ExternalSourceChange>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.SourceLinkId, e.DetectedAt });
            entity.Property(e => e.ExternalRef).HasMaxLength(200);
            entity.Property(e => e.Summary).HasMaxLength(500);

            entity.HasOne(e => e.AcknowledgedBy)
                .WithMany()
                .HasForeignKey(e => e.AcknowledgedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Entry — add ExpiryConfigChangedBy FK
        modelBuilder.Entity<Entry>(entity =>
        {
            entity.HasOne(e => e.ExpiryConfigChangedBy)
                .WithMany()
                .HasForeignKey(e => e.ExpiryConfigChangedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // TenantLanguageConfig configuration
        modelBuilder.Entity<TenantLanguageConfig>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.LanguageCode }).IsUnique();
            entity.Property(e => e.LanguageCode).IsRequired().HasMaxLength(10);
            entity.Property(e => e.DisplayName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.TranslationProvider).HasMaxLength(100);

            entity.HasOne(e => e.Tenant)
                .WithMany(t => t.LanguageConfigs)
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ----------------------------------------------------------------
        // Glossary entity configurations
        // ----------------------------------------------------------------

        modelBuilder.Entity<TenantGlossary>(entity =>
        {
            entity.HasKey(g => g.Id);
            entity.HasIndex(g => new { g.TenantId, g.SourceLanguage, g.TargetLanguage }).IsUnique();
            entity.Property(g => g.Name).HasMaxLength(256);
            entity.Property(g => g.SourceLanguage).IsRequired().HasMaxLength(10);
            entity.Property(g => g.TargetLanguage).IsRequired().HasMaxLength(10);
            entity.Property(g => g.DeepLGlossaryId).HasMaxLength(64);

            entity.HasMany(g => g.Entries)
                .WithOne(e => e.Glossary)
                .HasForeignKey(e => e.GlossaryId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(g => g.Tenant)
                .WithMany()
                .HasForeignKey(g => g.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TenantGlossaryEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.GlossaryId, e.SourceTerm }).IsUnique();
            entity.Property(e => e.SourceTerm).IsRequired().HasMaxLength(500);
            entity.Property(e => e.TargetTerm).IsRequired().HasMaxLength(500);
        });

        // ----------------------------------------------------------------
        // Style rule entity configurations
        // ----------------------------------------------------------------

        modelBuilder.Entity<TenantStyleRuleList>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.HasIndex(s => new { s.TenantId, s.TargetLanguage }).IsUnique();
            entity.Property(s => s.Name).HasMaxLength(256);
            entity.Property(s => s.TargetLanguage).IsRequired().HasMaxLength(10);
            entity.Property(s => s.DeepLStyleId).HasMaxLength(64);

            entity.HasMany(s => s.CustomInstructions)
                .WithOne(ci => ci.StyleRuleList)
                .HasForeignKey(ci => ci.StyleRuleListId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(s => s.Tenant)
                .WithMany()
                .HasForeignKey(s => s.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TenantCustomInstruction>(entity =>
        {
            entity.HasKey(ci => ci.Id);
            entity.Property(ci => ci.Label).IsRequired().HasMaxLength(200);
            entity.Property(ci => ci.Prompt).IsRequired().HasMaxLength(300);
            entity.Property(ci => ci.SourceLanguage).HasMaxLength(10);
            entity.Property(ci => ci.DeepLInstructionId).HasMaxLength(64);
        });

        // ----------------------------------------------------------------
        // Plugin platform entity configurations
        // ----------------------------------------------------------------

        modelBuilder.Entity<TenantPluginInstallation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.PluginId }).IsUnique();
            entity.Property(e => e.PluginId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Version).IsRequired().HasMaxLength(50);
        });

        // ConnectorInstallation (Sidecar Mode) — ITenantScoped, so the tenant query filter
        // is applied automatically by ApplyTenantFilters. Not unique on (TenantId, ConnectorKey):
        // a tenant may install the same connector twice (e.g. two Confluence sites or repos).
        modelBuilder.Entity<ConnectorInstallation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.ConnectorKey });
            entity.Property(e => e.ConnectorKey).IsRequired().HasMaxLength(50);
            entity.Property(e => e.DisplayName).HasMaxLength(200);
            entity.Property(e => e.LastError).HasMaxLength(1000);
            entity.Property(e => e.Status)
                .HasConversion<string>()
                .HasMaxLength(30);
        });

        // ----------------------------------------------------------------
        // Template entity configurations
        // ----------------------------------------------------------------

        modelBuilder.Entity<EntryTemplate>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ExternalId).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.IconName).HasMaxLength(50);
            entity.Property(e => e.ExternalId).HasMaxLength(100);
            entity.Property(e => e.SourceUrl).HasMaxLength(500);
            entity.Property(e => e.TagsJson).HasMaxLength(1000);

            entity.HasMany(e => e.Blocks)
                .WithOne(b => b.Template)
                .HasForeignKey(b => b.TemplateId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Ratings)
                .WithOne(r => r.Template)
                .HasForeignKey(r => r.TemplateId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(e => e.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.UpdatedBy)
                .WithMany()
                .HasForeignKey(e => e.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<EntryTemplateBlock>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TemplateId, e.Position });
            entity.Property(e => e.BlockType).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<TemplateRating>(entity =>
        {
            entity.HasKey(e => e.Id);
            // One rating per user per template
            entity.HasIndex(e => new { e.TemplateId, e.UserId }).IsUnique();

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TemplateUsage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TemplateId, e.UsedAt });
            entity.HasIndex(e => new { e.TenantId, e.UsedAt });

            entity.HasOne(e => e.Template)
                .WithMany()
                .HasForeignKey(e => e.TemplateId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // EntryTag configuration (many-to-many)
        modelBuilder.Entity<EntryTag>(entity =>
        {
            entity.HasKey(e => new { e.EntryId, e.TagId });
            
            entity.HasOne(e => e.Entry)
                .WithMany(d => d.EntryTags)
                .HasForeignKey(e => e.EntryId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Tag)
                .WithMany(t => t.EntryTags)
                .HasForeignKey(e => e.TagId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Entry user engagement configurations

        modelBuilder.Entity<EntryStarred>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.EntryId, e.UserId }).IsUnique();

            entity.HasOne(e => e.Entry)
                .WithMany()
                .HasForeignKey(e => e.EntryId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<EntryPin>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.EntryId).IsUnique();

            entity.HasOne(e => e.Entry)
                .WithMany()
                .HasForeignKey(e => e.EntryId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.PinnedByUserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<EntryWatcher>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.EntryId, e.UserId }).IsUnique();
            entity.Property(e => e.WatchLevel).IsRequired().HasMaxLength(50);

            entity.HasOne(e => e.Entry)
                .WithMany()
                .HasForeignKey(e => e.EntryId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<EntryVersion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.EntryId, e.VersionNumber });

            entity.HasOne(e => e.Entry)
                .WithMany()
                .HasForeignKey(e => e.EntryId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.SavedBy)
                .WithMany()
                .HasForeignKey(e => e.SavedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.ChangedBlocks)
                .WithOne(b => b.Version)
                .HasForeignKey(b => b.EntryVersionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<EntryVersionBlock>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.EntryVersionId);
            entity.Property(e => e.ChangeType).IsRequired().HasMaxLength(20);
        });

        // Share links
        modelBuilder.Entity<ShareLink>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasIndex(e => e.EntryId);
            entity.Property(e => e.Token).IsRequired().HasMaxLength(64);

            entity.HasOne(e => e.Entry)
                .WithMany()
                .HasForeignKey(e => e.EntryId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(e => e.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ----------------------------------------------------------------
        // AI / Search / Agent entity configurations
        // ----------------------------------------------------------------

        modelBuilder.Entity<BlockEmbedding>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasIndex(e => new { e.EntryBlockId, e.Language }).IsUnique();
            entity.HasIndex(e => new { e.EntryId, e.Language });
            entity.HasIndex(e => e.TenantId);
            entity.Property(e => e.Language).IsRequired().HasMaxLength(10);
            entity.Property(e => e.EmbeddingModel).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ContentHash).IsRequired().HasMaxLength(64);
            entity.Property(e => e.EmbeddingVector)
                .HasColumnType("vector(1536)");

            // HNSW index for cosine-distance ANN search (matches VECTOR_DISTANCE("cosine", ...)).
            entity.HasIndex(e => e.EmbeddingVector)
                .HasMethod("hnsw")
                .HasOperators("vector_cosine_ops");

            entity.HasOne(e => e.EntryBlock)
                .WithMany()
                .HasForeignKey(e => e.EntryBlockId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Entry)
                .WithMany()
                .HasForeignKey(e => e.EntryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<EntryEmbedding>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasIndex(e => new { e.EntryId, e.Language }).IsUnique();
            entity.HasIndex(e => e.TenantId);
            entity.Property(e => e.Language).IsRequired().HasMaxLength(10);
            entity.Property(e => e.EmbeddingModel).IsRequired().HasMaxLength(100);
            entity.Property(e => e.EmbeddingVector)
                .HasColumnType("vector(1536)");

            // HNSW index for cosine-distance ANN search.
            entity.HasIndex(e => e.EmbeddingVector)
                .HasMethod("hnsw")
                .HasOperators("vector_cosine_ops");

            entity.HasOne(e => e.Entry)
                .WithMany()
                .HasForeignKey(e => e.EntryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AgentConversation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.UserId });
            entity.Property(e => e.Title).HasMaxLength(500);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Messages)
                .WithOne(m => m.Conversation)
                .HasForeignKey(m => m.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AgentMessage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.ConversationId, e.CreatedAt });
        });

        modelBuilder.Entity<TenantAIUsage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.PeriodStart }).IsUnique();
        });

        modelBuilder.Entity<AIPromptLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.CreatedAt });
            entity.HasIndex(e => new { e.TenantId, e.UserId });
            entity.Property(e => e.PromptHash).IsRequired().HasMaxLength(64);
            entity.Property(e => e.ModelUsed).IsRequired().HasMaxLength(100);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ----------------------------------------------------------------
        // Personal Spaces entity configurations
        // ----------------------------------------------------------------

        modelBuilder.Entity<PersonalSpace>(entity =>
        {
            entity.HasKey(e => e.Id);
            // Exactly one personal space per user per tenant.
            entity.HasIndex(e => new { e.TenantId, e.OwnerUserId }).IsUnique();

            entity.HasOne(e => e.Owner)
                .WithMany()
                .HasForeignKey(e => e.OwnerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.Notes)
                .WithOne(n => n.Space)
                .HasForeignKey(n => n.PersonalSpaceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PersonalNote>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.OwnerUserId, e.ParentNoteId, e.SortOrder });
            entity.HasIndex(e => e.PersonalSpaceId);
            entity.Property(e => e.Language).IsRequired().HasMaxLength(10);

            entity.HasOne(e => e.Parent)
                .WithMany(p => p.Children)
                .HasForeignKey(e => e.ParentNoteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.Blocks)
                .WithOne(b => b.Note)
                .HasForeignKey(b => b.PersonalNoteId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Shares)
                .WithOne(s => s.Note)
                .HasForeignKey(s => s.PersonalNoteId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.NoteTags)
                .WithOne(t => t.Note)
                .HasForeignKey(t => t.PersonalNoteId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PersonalNoteBlock>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.PersonalNoteId, e.Position });
            entity.Property(e => e.BlockType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ContentHash).IsRequired().HasMaxLength(64);
        });

        modelBuilder.Entity<PersonalNoteShare>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.PersonalNoteId, e.UserId, e.GroupId });

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Group)
                .WithMany()
                .HasForeignKey(e => e.GroupId)
                .OnDelete(DeleteBehavior.ClientCascade);
        });

        modelBuilder.Entity<PersonalNoteTag>(entity =>
        {
            entity.HasKey(e => new { e.PersonalNoteId, e.TagId });

            entity.HasOne(e => e.Tag)
                .WithMany()
                .HasForeignKey(e => e.TagId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PersonalNoteEmbedding>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasIndex(e => new { e.PersonalNoteId, e.Language }).IsUnique();
            entity.HasIndex(e => e.OwnerUserId);
            entity.Property(e => e.Language).IsRequired().HasMaxLength(10);
            entity.Property(e => e.EmbeddingModel).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ContentHash).IsRequired().HasMaxLength(64);
            entity.Property(e => e.EmbeddingVector).HasColumnType("vector(1536)");

            // HNSW index for cosine-distance ANN search.
            entity.HasIndex(e => e.EmbeddingVector)
                .HasMethod("hnsw")
                .HasOperators("vector_cosine_ops");

            entity.HasOne(e => e.Note)
                .WithMany()
                .HasForeignKey(e => e.PersonalNoteId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserAIUsage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.UserId, e.PeriodStart }).IsUnique();

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ----------------------------------------------------------------
        // pgvector fallback mapping for non-Npgsql providers (the in-memory
        // provider used by tests). Npgsql maps `Vector` natively via UseVector();
        // other providers have no mapping, so the model would fail to build. We
        // store the vector as a comma-separated string there. The HNSW index and
        // vector(1536) column type configured above are Npgsql-only annotations
        // and are simply ignored by the in-memory provider.
        // ----------------------------------------------------------------
        if (!Database.IsNpgsql())
        {
            var vectorConverter = new ValueConverter<Vector, string>(
                v => string.Join(',', v.ToArray()),
                s => new Vector(s.Length == 0
                    ? Array.Empty<float>()
                    : s.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(float.Parse).ToArray()));

            modelBuilder.Entity<BlockEmbedding>().Property(e => e.EmbeddingVector).HasConversion(vectorConverter);
            modelBuilder.Entity<EntryEmbedding>().Property(e => e.EmbeddingVector).HasConversion(vectorConverter);
            modelBuilder.Entity<PersonalNoteEmbedding>().Property(e => e.EmbeddingVector).HasConversion(vectorConverter);
        }
    }

    // ----------------------------------------------------------------
    // Tenant query-filter helpers
    // Iterates every entity type registered in the model; for each type
    // that implements ITenantScoped the generic helper below stamps it
    // with HasQueryFilter(e => e.TenantId == tenantContext.TenantId).
    //
    // Plugins add new ITenantScoped entities via IEntityTypeConfiguration<T>
    // (discovered by ApplyConfigurationsFromAssembly above) and their query
    // filter is applied here automatically — no DbContext changes needed.
    // ----------------------------------------------------------------

    private static readonly MethodInfo s_applyFilterMethod =
        typeof(RasepiDbContext)
            .GetMethod(nameof(ApplyTenantFilterGeneric), BindingFlags.NonPublic | BindingFlags.Instance)!;

    private void ApplyTenantFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ITenantScoped).IsAssignableFrom(entityType.ClrType))
            {
                s_applyFilterMethod
                    .MakeGenericMethod(entityType.ClrType)
                    .Invoke(this, [modelBuilder]);
            }
        }
    }

    private void ApplyTenantFilterGeneric<T>(ModelBuilder modelBuilder)
        where T : class, ITenantScoped
    {
        modelBuilder.Entity<T>()
            .HasQueryFilter(e => e.TenantId == _tenantContext.TenantId);
    }

    // ----------------------------------------------------------------
    // SaveChanges guard
    // For every ITenantScoped entity being inserted or updated:
    //   - If TenantId is empty, stamp it from the current TenantContext.
    //   - If TenantId is set to a different tenant, throw immediately.
    // This prevents cross-tenant writes even if application code forgets
    // to set TenantId explicitly.
    // ----------------------------------------------------------------
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
        // Skip guard when TenantContext is not resolved (e.g. migrations, seed jobs).
        // Explicit background jobs should populate TenantContext themselves.
        if (!_tenantContext.IsResolved)
            return;

        foreach (var entry in ChangeTracker.Entries<ITenantScoped>())
        {
            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                if (entry.Entity.TenantId == Guid.Empty)
                {
                    // Auto-stamp new entities with the current tenant.
                    entry.Entity.TenantId = _tenantContext.TenantId;
                }
                else if (entry.Entity.TenantId != _tenantContext.TenantId)
                {
                    throw new InvalidOperationException(
                        $"Cross-tenant write blocked. " +
                        $"Entity TenantId={entry.Entity.TenantId} does not match " +
                        $"current TenantContext.TenantId={_tenantContext.TenantId}. " +
                        $"Entity type: {entry.Entity.GetType().Name}");
                }
            }
        }
    }
}
