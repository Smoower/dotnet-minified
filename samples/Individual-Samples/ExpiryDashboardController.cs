using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rasepi.Api.Authorization;
using Rasepi.Api.Data;
using Rasepi.Api.Services.Freshness;
using Rasepi.Shared.Models;

namespace Rasepi.Api.Controllers;

/// <summary>
/// Dashboard aggregations for entry expiry.
///
/// Three levels of granularity:
///   GET /expiry-dashboard/me                    — personal dashboard for the calling user
///   GET /expiry-dashboard/summary              — per-hub expiry counts for the tenant
///   GET /expiry-dashboard/hubs/{hubKey}/entries — per-entry expiry detail for one hub
/// </summary>
[Authorize]
[ApiController]
[Route("expiry-dashboard")]
public class ExpiryDashboardController : ControllerBase
{
    private readonly RasepiDbContext _context;
    private readonly IAuthorizationService _authService;
    private readonly IExpiryService _expiryService;

    public ExpiryDashboardController(RasepiDbContext context, IAuthorizationService authService, IExpiryService expiryService)
    {
        _context       = context;
        _authService   = authService;
        _expiryService = expiryService;
    }

    // ── Personal dashboard ────────────────────────────────────────────────────

    /// <summary>
    /// Returns expiry stats and entry details for every active entry owned by the
    /// calling user, across all hubs in the tenant.
    ///
    /// Includes aggregate counts (overdue, expiring this week / month, healthy)
    /// plus a per-entry list ordered most-urgent first.
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(PersonalExpiryDashboard), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetPersonalDashboard()
    {
        var userId = _authService.GetCurrentUserId(HttpContext);
        if (userId == null) return Unauthorized();

        var entries = await _context.Entries
            .Where(e => e.OwnerId == userId.Value && e.DeletedAt == null)
            .Include(e => e.Hub)
            .Include(e => e.Translations.Where(t => t.DeletedAt == null))
            .ToListAsync();

        var now = DateTime.UtcNow;

        var details = new List<HubEntryExpiryDetail>();
        foreach (var entry in entries)
        {
            var title = entry.Translations
                    .FirstOrDefault(t => t.Language == entry.OriginalLanguage)?.Title
                ?? entry.Translations.FirstOrDefault()?.Title;

            DateTime? expiryDate = await _expiryService.CalculateExpiryDateAsync(entry.Id);
            int? daysUntil = expiryDate.HasValue ? (int)(expiryDate.Value - now).TotalDays : (int?)null;

            details.Add(new HubEntryExpiryDetail(
                EntryId:         entry.Id,
                EntryKey:        entry.Key,
                Title:           title,
                Status:          entry.Status.ToString(),
                ExpiryDate:      expiryDate,
                DaysUntilExpiry: daysUntil,
                HubKey:          entry.Hub.Key,
                HubName:         entry.Hub.Name
            ));
        }

        details = details.OrderBy(e => e.DaysUntilExpiry ?? int.MaxValue).ToList();

        var summary = new PersonalExpirySummary(
            TotalOwned:           details.Count,
            OverdueCount:         details.Count(e => (e.DaysUntilExpiry ?? 1) <= 0),
            ExpiringThisWeekCount:  details.Count(e => e.DaysUntilExpiry is > 0 and <= 7),
            ExpiringThisMonthCount: details.Count(e => e.DaysUntilExpiry is > 7 and <= 30),
            HealthyCount:         details.Count(e => e.DaysUntilExpiry == null || e.DaysUntilExpiry > 30)
        );

        return Ok(new PersonalExpiryDashboard(summary, details));
    }

    // ── Tenant summary ────────────────────────────────────────────────────────

    /// <summary>
    /// Returns a per-hub expiry summary for the current tenant.
    ///
    /// Counts are derived directly from <c>Entry.Status</c>, which is kept fresh
    /// by the background <c>ExpireDocumentsAsync</c> / <c>UpdateWarningStatusesAsync</c> jobs.
    ///
    /// Status mapping:
    ///   <c>Expired</c>      → <c>expiredCount</c>
    ///   <c>ExpiringSoon</c> → <c>expiringThisWeekCount</c>  (typically ≤ 7 days)
    ///   <c>Warning</c>      → <c>expiringThisMonthCount</c> (typically ≤ 30 days)
    ///   <c>Published</c> / <c>Draft</c> → <c>healthyCount</c>
    /// </summary>
    [HttpGet("summary")]
    [RequireGlobalAdmin]
    [ProducesResponseType(typeof(IEnumerable<HubExpirySummary>), 200)]
    public async Task<IActionResult> GetTenantSummary()
    {
        var hubs = await _context.Hubs
            .Where(h => h.DeletedAt == null)
            .Select(h => new { h.Id, h.Key, h.Name })
            .ToListAsync();

        if (!hubs.Any())
            return Ok(Array.Empty<HubExpirySummary>());

        var hubIds = hubs.Select(h => h.Id).ToHashSet();

        // Single DB round-trip: group entries by hub + status
        var entryCounts = await _context.Entries
            .Where(e => e.DeletedAt == null && hubIds.Contains(e.HubId))
            .GroupBy(e => new { e.HubId, e.Status })
            .Select(g => new { g.Key.HubId, g.Key.Status, Count = g.Count() })
            .ToListAsync();

        var result = hubs.Select(hub =>
        {
            var counts = entryCounts.Where(c => c.HubId == hub.Id).ToList();
            return new HubExpirySummary(
                HubId:                  hub.Id,
                HubKey:                 hub.Key,
                HubName:                hub.Name,
                TotalEntries:           counts.Sum(c => c.Count),
                ExpiredCount:           counts.Where(c => c.Status == EntryStatus.Expired).Sum(c => c.Count),
                ExpiringThisWeekCount:  counts.Where(c => c.Status == EntryStatus.ExpiringSoon).Sum(c => c.Count),
                ExpiringThisMonthCount: counts.Where(c => c.Status == EntryStatus.Warning).Sum(c => c.Count),
                HealthyCount:           counts.Where(c => c.Status is EntryStatus.Published or EntryStatus.Draft).Sum(c => c.Count)
            );
        }).OrderByDescending(h => h.ExpiredCount + h.ExpiringThisWeekCount);

        return Ok(result);
    }

    // ── Hub drill-down ────────────────────────────────────────────────────────

    /// <summary>
    /// Returns entry-level expiry details for every active entry in a hub.
    ///
    /// <c>daysUntilExpiry</c> is computed via the template-based <c>IExpiryService</c>.
    /// Negative values mean overdue.  Results are ordered most-urgent first.
    /// </summary>
    [HttpGet("hubs/{hubKey}/entries")]
    [RequireHubRole(PermissionLevel.Admin)]
    [ProducesResponseType(typeof(IEnumerable<HubEntryExpiryDetail>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetHubEntries(string hubKey)
    {
        var hub = await _context.Hubs
            .FirstOrDefaultAsync(h => h.Key == hubKey && h.DeletedAt == null);

        if (hub == null) return NotFound();

        var entries = await _context.Entries
            .Where(e => e.HubId == hub.Id && e.DeletedAt == null)
            .Include(e => e.Translations.Where(t => t.DeletedAt == null))
            .ToListAsync();

        var now = DateTime.UtcNow;

        var result = new List<HubEntryExpiryDetail>();
        foreach (var entry in entries)
        {
            // Prefer the original-language translation for the title
            var title = entry.Translations
                    .FirstOrDefault(t => t.Language == entry.OriginalLanguage)?.Title
                ?? entry.Translations.FirstOrDefault()?.Title;

            DateTime? expiryDate = await _expiryService.CalculateExpiryDateAsync(entry.Id);
            int? daysUntil = expiryDate.HasValue ? (int)(expiryDate.Value - now).TotalDays : (int?)null;

            result.Add(new HubEntryExpiryDetail(
                EntryId:       entry.Id,
                EntryKey:      entry.Key,
                Title:         title,
                Status:        entry.Status.ToString(),
                ExpiryDate:    expiryDate,
                DaysUntilExpiry: daysUntil,
                HubKey:        hub.Key,
                HubName:       hub.Name
            ));
        }

        return Ok(result.OrderBy(e => e.DaysUntilExpiry ?? int.MaxValue));
    }
}

// ── Response shapes ───────────────────────────────────────────────────────────

/// <summary>Per-hub expiry count summary for the tenant dashboard.</summary>
public record HubExpirySummary(
    Guid   HubId,
    string HubKey,
    string HubName,
    int    TotalEntries,
    int    ExpiredCount,
    /// <summary>Entries with status <c>ExpiringSoon</c> (typically ≤ 7 days).</summary>
    int    ExpiringThisWeekCount,
    /// <summary>Entries with status <c>Warning</c> (typically ≤ 30 days).</summary>
    int    ExpiringThisMonthCount,
    int    HealthyCount
);

/// <summary>Per-entry expiry detail record for a hub drill-down view.</summary>
public record HubEntryExpiryDetail(
    Guid      EntryId,
    string    EntryKey,
    string?   Title,
    string    Status,
    DateTime? ExpiryDate,
    /// <summary>Negative values indicate the entry is already overdue.</summary>
    int?      DaysUntilExpiry,
    string    HubKey,
    string    HubName
);

/// <summary>Aggregate expiry counts for the personal dashboard.</summary>
public record PersonalExpirySummary(
    int TotalOwned,
    int OverdueCount,
    int ExpiringThisWeekCount,
    int ExpiringThisMonthCount,
    int HealthyCount
);

/// <summary>Combined personal expiry dashboard: summary counts + ordered entry list.</summary>
public record PersonalExpiryDashboard(
    PersonalExpirySummary Summary,
    IEnumerable<HubEntryExpiryDetail> Entries
);
