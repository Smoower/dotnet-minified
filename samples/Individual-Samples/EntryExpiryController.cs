using Microsoft.AspNetCore.Mvc;
using Rasepi.Api.Authorization;

namespace Rasepi.Api.Controllers;

/// <summary>
/// Manages expiry configuration, review history, renewal, and status queries for a entry.
/// </summary>
[Authorize]
[ApiController]
[Route("entries/{entryId:guid}")]
public class EntryExpiryController : ControllerBase
{
    private readonly IExpiryService _expiryService;
    private readonly IReviewService _reviewService;
    private readonly IRenewalService _renewalService;
    private readonly IAuthorizationService _authService;
    private readonly IExpiryNotificationService _notificationService;

    public EntryExpiryController(
        IExpiryService expiryService,
        IReviewService reviewService,
        IRenewalService renewalService,
        IAuthorizationService authService,
        IExpiryNotificationService notificationService)
    {
        _expiryService = expiryService;
        _reviewService = reviewService;
        _renewalService = renewalService;
        _authService = authService;
        _notificationService = notificationService;
    }

    // ── Expiry config ──────────────────────────────────────────────────

    /// <summary>Returns the resolved expiry configuration for this entry.</summary>
    [HttpGet("expiry-config")]
    [RequireDocumentRole(PermissionLevel.Viewer)]
    public async Task<IActionResult> GetExpiryConfig(Guid entryId)
    {
        try { return Ok(await _expiryService.GetEffectiveConfigAsync(entryId)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    /// <summary>Returns the effective expiry template (resolved according to fallback order).</summary>
    [HttpGet("effective-template")]
    [RequireDocumentRole(PermissionLevel.Viewer)]
    public async Task<IActionResult> GetEffectiveTemplate(Guid entryId)
    {
        try { return Ok(await _expiryService.GetEffectiveTemplateAsync(entryId)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    /// <summary>Returns current expiry status: days remaining, expiry date, warning state.</summary>
    [HttpGet("expiry-status")]
    [RequireDocumentRole(PermissionLevel.Viewer)]
    public async Task<IActionResult> GetExpiryStatus(Guid entryId)
    {
        try
        {
            var expiryDate = await _expiryService.CalculateExpiryDateAsync(entryId);
            var daysUntil = await _expiryService.GetDaysUntilExpiryAsync(entryId);
            var inWarning = await _expiryService.IsInWarningPeriodAsync(entryId);
            var paused = await _expiryService.IsExpiryPausedAsync(entryId);

            return Ok(new
            {
                expiryDate,
                daysUntilExpiry = daysUntil == int.MaxValue ? (int?)null : daysUntil,
                isInWarningPeriod = inWarning,
                isPaused = paused
            });
        }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    /// <summary>Assigns a specific expiry template to this entry.</summary>
    [HttpPost("change-template")]
    [RequireDocumentRole(PermissionLevel.Editor)]
    public async Task<IActionResult> ChangeTemplate(Guid entryId, [FromBody] ChangeTemplateRequest request)
    {
        var userId = _authService.GetCurrentUserId(HttpContext);
        if (userId == null) return Unauthorized();
        try
        {
            await _expiryService.ChangeTemplateAsync(entryId, request.TemplateId, userId.Value, request.Reason);
            return NoContent();
        }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    /// <summary>Sets a custom expiry config JSON on this entry (overrides template).</summary>
    [HttpPost("set-custom-config")]
    [RequireDocumentRole(PermissionLevel.Editor)]
    public async Task<IActionResult> SetCustomConfig(Guid entryId, [FromBody] SetCustomConfigRequest request)
    {
        var userId = _authService.GetCurrentUserId(HttpContext);
        if (userId == null) return Unauthorized();
        try
        {
            await _expiryService.SetCustomConfigAsync(entryId, request.ConfigJson, userId.Value, request.Reason);
            return NoContent();
        }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    /// <summary>Removes the custom config, reverting to template-based expiry.</summary>
    [HttpDelete("custom-config")]
    [RequireDocumentRole(PermissionLevel.Editor)]
    public async Task<IActionResult> ClearCustomConfig(Guid entryId)
    {
        var userId = _authService.GetCurrentUserId(HttpContext);
        if (userId == null) return Unauthorized();
        try
        {
            await _expiryService.ClearCustomConfigAsync(entryId, userId.Value);
            return NoContent();
        }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    // ── Review ──────────────────────────────────────────────────────────

    /// <summary>Records a review action against this entry.</summary>
    [HttpPost("review")]
    [RequireDocumentRole(PermissionLevel.Editor)]
    public async Task<IActionResult> CreateReview(Guid entryId, [FromBody] CreateReviewRequest request)
    {
        var userId = _authService.GetCurrentUserId(HttpContext);
        if (userId == null) return Unauthorized();
        try
        {
            var review = await _reviewService.CreateReviewAsync(
                entryId, userId.Value, request.ReviewType, request.Notes,
                request.TimeSpentSeconds, request.BlocksChanged, request.ContentWasUpdated,
                request.ChecklistCompletionJson);
            return Ok(review);
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (InvalidOperationException ex) { return UnprocessableEntity(new { error = ex.Message }); }
    }

    /// <summary>Returns the full review history for this entry.</summary>
    [HttpGet("review-history")]
    [RequireDocumentRole(PermissionLevel.Viewer)]
    public async Task<IActionResult> GetReviewHistory(Guid entryId) =>
        Ok(await _reviewService.GetHistoryAsync(entryId));

    /// <summary>Returns the most recent review.</summary>
    [HttpGet("last-review")]
    [RequireDocumentRole(PermissionLevel.Viewer)]
    public async Task<IActionResult> GetLastReview(Guid entryId)
    {
        var review = await _reviewService.GetLastReviewAsync(entryId);
        return review == null ? NotFound() : Ok(review);
    }

    /// <summary>Signs the attestation on a specific review record.</summary>
    [HttpPost("review/{reviewId:guid}/attest")]
    [RequireDocumentRole(PermissionLevel.Editor)]
    public async Task<IActionResult> SignAttestation(Guid entryId, Guid reviewId)
    {
        var userId = _authService.GetCurrentUserId(HttpContext);
        if (userId == null) return Unauthorized();
        try
        {
            var review = await _reviewService.SignAttestationAsync(reviewId, userId.Value);
            return Ok(review);
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (InvalidOperationException) { return Forbid(); }
    }

    /// <summary>Returns human-readable requirements for a review to be accepted.</summary>
    [HttpGet("review-requirements")]
    [RequireDocumentRole(PermissionLevel.Viewer)]
    public async Task<IActionResult> GetReviewRequirements(Guid entryId)
    {
        try { return Ok(await _reviewService.GetReviewRequirementsAsync(entryId)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    // ── Renewal ─────────────────────────────────────────────────────────

    /// <summary>Renews a entry, resetting its expiry clock.</summary>
    [HttpPost("renew")]
    [RequireDocumentRole(PermissionLevel.Editor)]
    public async Task<IActionResult> Renew(Guid entryId, [FromBody] RenewRequest? request = null)
    {
        var userId = _authService.GetCurrentUserId(HttpContext);
        if (userId == null) return Unauthorized();
        try
        {
            var doc = await _renewalService.RenewAsync(
                entryId, userId.Value, request?.CustomExpiryDays, request?.Notes);
            return Ok(doc);
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (InvalidOperationException ex) { return UnprocessableEntity(new { error = ex.Message }); }
    }

    /// <summary>Returns whether renewal is currently allowed and any blocking reasons.</summary>
    [HttpGet("renewal-eligibility")]
    public async Task<IActionResult> GetRenewalEligibility(Guid entryId)
    {
        var userId = _authService.GetCurrentUserId(HttpContext);
        if (userId == null) return Unauthorized();
        var blockers = await _renewalService.GetRenewalBlockersAsync(entryId);
        return Ok(new { canRenew = !blockers.Any(), blockers });
    }
}

// ── Request/response models ──────────────────────────────────────────────────

public record ChangeTemplateRequest(Guid TemplateId, string? Reason);

public record SetCustomConfigRequest(string ConfigJson, string? Reason);

public record CreateReviewRequest(
    ReviewType ReviewType,
    string? Notes,
    int TimeSpentSeconds,
    int BlocksChanged = 0,
    bool ContentWasUpdated = false,
    string? ChecklistCompletionJson = null);

public record RenewRequest(int? CustomExpiryDays, string? Notes);
