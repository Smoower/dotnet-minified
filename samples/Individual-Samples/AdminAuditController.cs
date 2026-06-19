using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rasepi.Api.Authorization;
using Rasepi.Api.Data;
using Rasepi.Api.Services.Tenants;
using Rasepi.Shared.Models;
using Rasepi.Shared.Models.Plugins.Classification;

namespace Rasepi.Api.Controllers;

/// <summary>
/// Cross-cutting audit-trail surfaces for tenant admins. The underlying entities
/// (expiry config changes, review history, expiry-notification delivery) are
/// all tenant-scoped via <see cref="Entry"/> navigation filters on the DbContext,
/// so a tenant admin sees only their own tenant's records.
///
/// This controller deliberately stays read-only — audit records are immutable
/// by design. Any write path on these entities lives on the domain controller
/// that owns the workflow (<see cref="EntryExpiryController"/>, etc.).
/// </summary>
[Authorize]
[RequireTenantAdmin]
[ApiController]
[Route("admin/audit")]
public class AdminAuditController : ControllerBase
{
    private readonly RasepiDbContext _db;
    private readonly TenantContext _tenantContext;

    public AdminAuditController(RasepiDbContext db, TenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    /// <summary>
    /// Who changed expiry templates / custom configs on which entries, when, and why.
    /// Filters compose (AND); date range is inclusive. Paged, newest first.
    /// </summary>
    [HttpGet("expiry-config")]
    public async Task<IActionResult> GetExpiryConfigAudit(
        [FromQuery] Guid? entryId,
        [FromQuery] Guid? userId,
        [FromQuery] DateTime? since,
        [FromQuery] DateTime? until,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 200);
        page = Math.Max(page, 1);

        var query = _db.ExpiryConfigAuditLogs
            .AsNoTracking()
            .Include(l => l.Entry)
            .Include(l => l.ChangedBy)
            .Include(l => l.OldTemplate)
            .Include(l => l.NewTemplate)
            .AsQueryable();

        if (entryId.HasValue) query = query.Where(l => l.EntryId == entryId.Value);
        if (userId.HasValue)  query = query.Where(l => l.ChangedByUserId == userId.Value);
        if (since.HasValue)   query = query.Where(l => l.ChangedAt >= since.Value);
        if (until.HasValue)   query = query.Where(l => l.ChangedAt <= until.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(l => l.ChangedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new
            {
                l.Id,
                l.EntryId,
                entryKey = l.Entry.Key,
                changeType = l.ChangeType.ToString(),
                l.OldTemplateId,
                oldTemplateName = l.OldTemplate != null ? l.OldTemplate.Name : null,
                l.NewTemplateId,
                newTemplateName = l.NewTemplate != null ? l.NewTemplate.Name : null,
                l.OldCustomConfig,
                l.NewCustomConfig,
                l.Reason,
                l.ChangedByUserId,
                changedByName = l.ChangedBy.Name,
                changedByEmail = l.ChangedBy.Email,
                l.ChangedAt,
            })
            .ToListAsync(ct);

        return Ok(new { items, total, page, pageSize });
    }

    /// <summary>
    /// Cross-entry review history: who reviewed what, when, how long they spent,
    /// whether they attested, whether they edited. Filters compose.
    /// </summary>
    [HttpGet("review-history")]
    public async Task<IActionResult> GetReviewHistory(
        [FromQuery] Guid? entryId,
        [FromQuery] Guid? hubId,
        [FromQuery] Guid? reviewerId,
        [FromQuery] ReviewType? reviewType,
        [FromQuery] bool? attestationSigned,
        [FromQuery] DateTime? since,
        [FromQuery] DateTime? until,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 200);
        page = Math.Max(page, 1);

        var query = _db.DocumentReviewHistories
            .AsNoTracking()
            .Include(h => h.Entry)
            .Include(h => h.Reviewer)
            .AsQueryable();

        if (entryId.HasValue)           query = query.Where(h => h.EntryId == entryId.Value);
        if (hubId.HasValue)             query = query.Where(h => h.Entry.HubId == hubId.Value);
        if (reviewerId.HasValue)        query = query.Where(h => h.ReviewerId == reviewerId.Value);
        if (reviewType.HasValue)        query = query.Where(h => h.ReviewType == reviewType.Value);
        if (attestationSigned.HasValue) query = query.Where(h => h.AttestationSigned == attestationSigned.Value);
        if (since.HasValue)             query = query.Where(h => h.ReviewedAt >= since.Value);
        if (until.HasValue)             query = query.Where(h => h.ReviewedAt <= until.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(h => h.ReviewedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(h => new
            {
                h.Id,
                h.EntryId,
                entryKey = h.Entry.Key,
                hubId = h.Entry.HubId,
                h.ReviewerId,
                reviewerName = h.Reviewer.Name,
                reviewerEmail = h.Reviewer.Email,
                h.ReviewedAt,
                reviewType = h.ReviewType.ToString(),
                h.ReviewNotes,
                h.ContentWasUpdated,
                h.BlocksChanged,
                h.ExpiryExtended,
                h.NewExpiryDate,
                h.TimeSpentSeconds,
                h.AttestationSigned,
            })
            .ToListAsync(ct);

        return Ok(new { items, total, page, pageSize });
    }

    /// <summary>
    /// Expiry-notification delivery log. Filter by status to find failures.
    /// </summary>
    [HttpGet("notifications")]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] Guid? entryId,
        [FromQuery] Guid? recipientId,
        [FromQuery] DeliveryStatus? status,
        [FromQuery] NotificationMethod? method,
        [FromQuery] DateTime? since,
        [FromQuery] DateTime? until,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 200);
        page = Math.Max(page, 1);

        var query = _db.ExpiryNotifications
            .AsNoTracking()
            .Include(n => n.Entry)
            .Include(n => n.Recipient)
            .AsQueryable();

        if (entryId.HasValue)     query = query.Where(n => n.EntryId == entryId.Value);
        if (recipientId.HasValue) query = query.Where(n => n.RecipientUserId == recipientId.Value);
        if (status.HasValue)      query = query.Where(n => n.Status == status.Value);
        if (method.HasValue)      query = query.Where(n => n.Method == method.Value);
        if (since.HasValue)       query = query.Where(n => n.SentAt >= since.Value);
        if (until.HasValue)       query = query.Where(n => n.SentAt <= until.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(n => n.SentAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(n => new
            {
                n.Id,
                n.EntryId,
                entryKey = n.Entry.Key,
                n.RecipientUserId,
                recipientName = n.Recipient.Name,
                recipientEmail = n.Recipient.Email,
                n.DaysBeforeExpiry,
                n.SentAt,
                method = n.Method.ToString(),
                n.NotificationChannelId,
                status = n.Status.ToString(),
                n.ErrorMessage,
                n.Acknowledged,
                n.AcknowledgedAt,
            })
            .ToListAsync(ct);

        return Ok(new { items, total, page, pageSize });
    }

    /// <summary>
    /// AI prompt audit: who ran which prompts, when, what model, how many tokens.
    /// The existing <see cref="AIAdminController"/> exposes only a flat recent list;
    /// this endpoint adds <c>userId</c> and time-range filters plus a user-name
    /// join so tenant admins can answer "which team member used the most AI last week?".
    /// </summary>
    /// <summary>
    /// Universal activity feed: every domain event that flowed through the
    /// event pipeline is persisted here by <c>EventConsumerWorker</c>. This is
    /// the single surface an admin uses to answer "what happened in my tenant
    /// last week". Filter by event name prefix (e.g. <c>Hub.</c>), entity, actor,
    /// or time range.
    /// </summary>
    [HttpGet("activity")]
    public async Task<IActionResult> GetActivity(
        [FromQuery] string? eventNamePrefix,
        [FromQuery] Guid? entityId,
        [FromQuery] Guid? actorUserId,
        [FromQuery] DateTime? since,
        [FromQuery] DateTime? until,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 200);
        page = Math.Max(page, 1);

        var query = _db.AuditLogEntries.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(eventNamePrefix))
            query = query.Where(a => a.EventName.StartsWith(eventNamePrefix));
        if (entityId.HasValue)     query = query.Where(a => a.EntityId == entityId.Value);
        if (actorUserId.HasValue)  query = query.Where(a => a.ActorUserId == actorUserId.Value);
        if (since.HasValue)        query = query.Where(a => a.OccurredAt >= since.Value);
        if (until.HasValue)        query = query.Where(a => a.OccurredAt <= until.Value);

        var total = await query.CountAsync(ct);

        var rawItems = await query
            .OrderByDescending(a => a.OccurredAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var userIds = rawItems
            .Where(a => a.ActorUserId.HasValue)
            .Select(a => a.ActorUserId!.Value)
            .Distinct()
            .ToList();
        var userLookup = await _db.Users
            .AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new { u.Id, u.Name, u.Email })
            .ToDictionaryAsync(u => u.Id, ct);

        var items = rawItems.Select(a => new
        {
            a.Id,
            a.EventName,
            a.EntityId,
            a.ActorUserId,
            actorName = a.ActorUserId.HasValue && userLookup.TryGetValue(a.ActorUserId.Value, out var u) ? u.Name : null,
            actorEmail = a.ActorUserId.HasValue && userLookup.TryGetValue(a.ActorUserId.Value, out var u2) ? u2.Email : null,
            a.PayloadType,
            a.PayloadJson,
            a.EventVersion,
            a.CorrelationId,
            a.OccurredAt,
        });

        return Ok(new { items, total, page, pageSize });
    }

    /// <summary>
    /// Entry content version history — one snapshot per original-language save.
    /// </summary>
    [HttpGet("entry-versions")]
    public async Task<IActionResult> GetEntryVersions(
        [FromQuery] Guid? entryId,
        [FromQuery] Guid? hubId,
        [FromQuery] Guid? savedById,
        [FromQuery] DateTime? since,
        [FromQuery] DateTime? until,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 200);
        page = Math.Max(page, 1);

        var query = _db.EntryVersions
            .AsNoTracking()
            .Include(v => v.Entry)
            .Include(v => v.SavedBy)
            .AsQueryable();

        if (entryId.HasValue)    query = query.Where(v => v.EntryId == entryId.Value);
        if (hubId.HasValue)      query = query.Where(v => v.Entry.HubId == hubId.Value);
        if (savedById.HasValue)  query = query.Where(v => v.SavedById == savedById.Value);
        if (since.HasValue)      query = query.Where(v => v.SavedAt >= since.Value);
        if (until.HasValue)      query = query.Where(v => v.SavedAt <= until.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(v => v.SavedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(v => new
            {
                v.Id,
                v.EntryId,
                entryKey = v.Entry.Key,
                hubId = v.Entry.HubId,
                v.VersionNumber,
                v.Title,
                v.SavedById,
                savedByName = v.SavedBy != null ? v.SavedBy.Name : null,
                v.SavedAt,
            })
            .ToListAsync(ct);

        return Ok(new { items, total, page, pageSize });
    }

    /// <summary>
    /// Translation provider call log — every DeepL / machine-translation API
    /// invocation. Useful for cost forensics and provider reliability reports.
    /// </summary>
    [HttpGet("translation-history")]
    public async Task<IActionResult> GetTranslationHistory(
        [FromQuery] Guid? entryId,
        [FromQuery] string? provider,
        [FromQuery] bool? success,
        [FromQuery] DateTime? since,
        [FromQuery] DateTime? until,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 200);
        page = Math.Max(page, 1);

        var query = _db.TranslationHistories.AsNoTracking();

        if (entryId.HasValue)                       query = query.Where(h => h.EntryId == entryId.Value);
        if (!string.IsNullOrWhiteSpace(provider))   query = query.Where(h => h.Provider == provider);
        if (success.HasValue)                       query = query.Where(h => h.Success == success.Value);
        if (since.HasValue)                         query = query.Where(h => h.OccurredAt >= since.Value);
        if (until.HasValue)                         query = query.Where(h => h.OccurredAt <= until.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(h => h.OccurredAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return Ok(new { items, total, page, pageSize });
    }

    /// <summary>
    /// Classification audit trail — every classification label change (direct
    /// set, change-request, approval, rejection, cancellation).
    /// </summary>
    [HttpGet("classification")]
    public async Task<IActionResult> GetClassificationAudit(
        [FromQuery] Guid? entryId,
        [FromQuery] Guid? performedByUserId,
        [FromQuery] string? actionType,
        [FromQuery] DateTime? since,
        [FromQuery] DateTime? until,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 200);
        page = Math.Max(page, 1);

        var query = _db.Set<EntryClassificationAuditLog>().AsNoTracking();

        if (entryId.HasValue)                           query = query.Where(c => c.EntryId == entryId.Value);
        if (performedByUserId.HasValue)                 query = query.Where(c => c.PerformedByUserId == performedByUserId.Value);
        if (!string.IsNullOrWhiteSpace(actionType))     query = query.Where(c => c.ActionType == actionType);
        if (since.HasValue)                             query = query.Where(c => c.PerformedAtUtc >= since.Value);
        if (until.HasValue)                             query = query.Where(c => c.PerformedAtUtc <= until.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(c => c.PerformedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return Ok(new { items, total, page, pageSize });
    }

    [HttpGet("ai-prompts")]
    public async Task<IActionResult> GetAIPromptAudit(
        [FromQuery] Guid? userId,
        [FromQuery] QuotaType? queryType,
        [FromQuery] DateTime? since,
        [FromQuery] DateTime? until,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 200);
        page = Math.Max(page, 1);

        var query = _db.AIPromptLogs.AsNoTracking();

        if (userId.HasValue)     query = query.Where(l => l.UserId == userId.Value);
        if (queryType.HasValue)  query = query.Where(l => l.QueryType == queryType.Value);
        if (since.HasValue)      query = query.Where(l => l.CreatedAt >= since.Value);
        if (until.HasValue)      query = query.Where(l => l.CreatedAt <= until.Value);

        var total = await query.CountAsync(ct);

        // The AIPromptLog.TenantId global filter enforces tenant isolation on the
        // log rows; user names are resolved separately because User is platform-level.
        var rawItems = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var userIds = rawItems.Select(i => i.UserId).Distinct().ToList();
        var userLookup = await _db.Users
            .AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new { u.Id, u.Name, u.Email })
            .ToDictionaryAsync(u => u.Id, ct);

        var items = rawItems.Select(l => new
        {
            l.Id,
            l.UserId,
            userName = userLookup.TryGetValue(l.UserId, out var u) ? u.Name : null,
            userEmail = userLookup.TryGetValue(l.UserId, out var u2) ? u2.Email : null,
            queryType = l.QueryType.ToString(),
            l.PromptHash,
            model = l.ModelUsed,
            l.InputTokens,
            l.OutputTokens,
            l.ResponseTimeMs,
            l.CreatedAt,
        });

        return Ok(new { items, total, page, pageSize });
    }
}
