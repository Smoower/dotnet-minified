using Microsoft.EntityFrameworkCore;

namespace TodoApi;

// All the non-CRUD rules live here: a status state-machine, completion gating on
// subtasks, a per-assignee work-in-progress limit, and recurrence spawning.
public class TaskService(TodoDb db)
{
    private const int WipLimit = 5;

    private static readonly Dictionary<TaskState, TaskState[]> Allowed = new()
    {
        [TaskState.Todo] = [TaskState.InProgress, TaskState.Archived],
        [TaskState.InProgress] = [TaskState.Blocked, TaskState.Done, TaskState.Todo],
        [TaskState.Blocked] = [TaskState.InProgress, TaskState.Archived],
        [TaskState.Done] = [TaskState.Archived, TaskState.InProgress],
        [TaskState.Archived] = [],
    };

    public async Task<TodoTask> TransitionAsync(int id, TaskState to, CancellationToken ct)
    {
        var task = await db.Tasks.Include(t => t.Subtasks).FirstOrDefaultAsync(t => t.Id == id, ct)
            ?? throw new KeyNotFoundException();

        if (task.Status == to)
            return task;

        if (!Allowed.TryGetValue(task.Status, out var next) || !next.Contains(to))
            throw new DomainException($"Cannot move a task from {task.Status} to {to}.");

        if (to == TaskState.Done && task.Subtasks.Any(s => s.Status is not (TaskState.Done or TaskState.Archived)))
            throw new DomainException("Cannot complete a task while it has open subtasks.");

        if (to == TaskState.InProgress)
            await GuardWipAsync(task.AssigneeId, id, ct);

        task.Status = to;
        if (to == TaskState.Done)
        {
            task.CompletedAt = DateTime.UtcNow;
            if (task.RecurrenceDays is int days)
                db.Tasks.Add(NextOccurrence(task, days));
        }

        await db.SaveChangesAsync(ct);
        return task;
    }

    public async Task<TodoTask> AssignAsync(int id, int? assigneeId, CancellationToken ct)
    {
        var task = await db.Tasks.FindAsync([id], ct) ?? throw new KeyNotFoundException();

        if (task.Status == TaskState.InProgress)
            await GuardWipAsync(assigneeId, id, ct);

        task.AssigneeId = assigneeId;
        await db.SaveChangesAsync(ct);
        return task;
    }

    private async Task GuardWipAsync(int? assigneeId, int excludeId, CancellationToken ct)
    {
        if (assigneeId is not int who)
            return;

        var wip = await db.Tasks.CountAsync(
            t => t.AssigneeId == who && t.Status == TaskState.InProgress && t.Id != excludeId, ct);

        if (wip >= WipLimit)
            throw new DomainException($"Assignee {who} is at the WIP limit ({WipLimit}).");
    }

    private static TodoTask NextOccurrence(TodoTask t, int days) => new()
    {
        ListId = t.ListId,
        Title = t.Title,
        Notes = t.Notes,
        Priority = t.Priority,
        AssigneeId = t.AssigneeId,
        RecurrenceDays = t.RecurrenceDays,
        DueAt = (t.DueAt ?? DateTime.UtcNow).AddDays(days),
        Status = TaskState.Todo,
        CreatedAt = DateTime.UtcNow,
    };
}
