using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TodoApi.Controllers;

[ApiController]
[Route("api")]
public class TasksController(TodoDb db, TaskService svc) : ControllerBase
{
    [HttpGet("lists/{listId:int}/tasks")]
    public async Task<IActionResult> List(
        int listId,
        [FromQuery] TaskState? status,
        [FromQuery] int? assignee,
        [FromQuery] bool overdueOnly = false,
        CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var query = db.Tasks.AsNoTracking().Where(t => t.ListId == listId);

        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);
        if (assignee.HasValue)
            query = query.Where(t => t.AssigneeId == assignee.Value);
        if (overdueOnly)
            query = query.Where(t => t.DueAt != null && t.DueAt < now
                && t.Status != TaskState.Done && t.Status != TaskState.Archived);

        var items = await query
            .OrderBy(t => t.DueAt ?? DateTime.MaxValue)
            .Select(t => new TaskDto(t.Id, t.Title, t.Status.ToString(), t.Priority.ToString(), t.DueAt, t.AssigneeId))
            .ToListAsync(ct);

        return Ok(items);
    }

    [HttpGet("tasks/{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken ct)
    {
        var task = await db.Tasks.AsNoTracking()
            .Where(t => t.Id == id)
            .Select(t => new TaskDto(t.Id, t.Title, t.Status.ToString(), t.Priority.ToString(), t.DueAt, t.AssigneeId))
            .FirstOrDefaultAsync(ct);

        return task is null ? NotFound() : Ok(task);
    }

    [HttpPost("lists/{listId:int}/tasks")]
    public async Task<IActionResult> Create(int listId, [FromBody] CreateTaskRequest r, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(r.Title))
            return BadRequest(new { error = "Title is required." });
        if (r.Title.Length > 200)
            return BadRequest(new { error = "Title must be 200 characters or fewer." });
        if (!await db.Lists.AnyAsync(l => l.Id == listId, ct))
            return NotFound();

        var task = new TodoTask
        {
            ListId = listId,
            Title = r.Title.Trim(),
            Notes = r.Notes,
            Priority = r.Priority,
            DueAt = r.DueAt,
            RecurrenceDays = r.RecurrenceDays,
            ParentTaskId = r.ParentTaskId,
            Status = TaskState.Todo,
            CreatedAt = DateTime.UtcNow,
        };
        db.Tasks.Add(task);
        await db.SaveChangesAsync(ct);

        var dto = new TaskDto(task.Id, task.Title, task.Status.ToString(), task.Priority.ToString(), task.DueAt, task.AssigneeId);
        return CreatedAtAction(nameof(Get), new { id = task.Id }, dto);
    }

    [HttpPost("tasks/{id:int}/transition")]
    public async Task<IActionResult> Transition(int id, [FromBody] TransitionRequest r, CancellationToken ct)
    {
        try
        {
            var task = await svc.TransitionAsync(id, r.To, ct);
            return Ok(new TaskDto(task.Id, task.Title, task.Status.ToString(), task.Priority.ToString(), task.DueAt, task.AssigneeId));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (DomainException ex)
        {
            return UnprocessableEntity(new { error = ex.Message });
        }
    }

    [HttpPost("tasks/{id:int}/assign")]
    public async Task<IActionResult> Assign(int id, [FromBody] AssignRequest r, CancellationToken ct)
    {
        try
        {
            var task = await svc.AssignAsync(id, r.AssigneeId, ct);
            return Ok(new TaskDto(task.Id, task.Title, task.Status.ToString(), task.Priority.ToString(), task.DueAt, task.AssigneeId));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (DomainException ex)
        {
            return UnprocessableEntity(new { error = ex.Message });
        }
    }

    [HttpDelete("tasks/{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var task = await db.Tasks.FindAsync([id], ct);
        if (task is null)
            return NotFound();

        db.Tasks.Remove(task);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }
}
