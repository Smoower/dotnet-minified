using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TodoApi.Controllers;

[ApiController]
[Route("api")]
public class DashboardController(TodoDb db) : ControllerBase
{
    [HttpGet("lists/{listId:int}/summary")]
    public async Task<IActionResult> Summary(int listId, CancellationToken ct)
    {
        if (!await db.Lists.AnyAsync(l => l.Id == listId, ct))
            return NotFound();

        var tasks = await db.Tasks.AsNoTracking().Where(t => t.ListId == listId).ToListAsync(ct);
        var now = DateTime.UtcNow;

        bool IsOpen(TodoTask t) => t.Status != TaskState.Done && t.Status != TaskState.Archived;
        var done = tasks.Count(t => t.Status == TaskState.Done || t.Status == TaskState.Archived);

        var summary = new ListSummary(
            ListId: listId,
            Total: tasks.Count,
            ByStatus: tasks.GroupBy(t => t.Status).ToDictionary(g => g.Key.ToString(), g => g.Count()),
            Overdue: tasks.Count(t => t.DueAt is { } d && d < now && IsOpen(t)),
            DueThisWeek: tasks.Count(t => t.DueAt is { } d && d >= now && d <= now.AddDays(7) && IsOpen(t)),
            CompletionPct: tasks.Count == 0 ? 0 : Math.Round(100.0 * done / tasks.Count, 1));

        return Ok(summary);
    }
}
