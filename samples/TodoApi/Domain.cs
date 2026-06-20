using Microsoft.EntityFrameworkCore;

namespace TodoApi;

public enum TaskState { Todo, InProgress, Blocked, Done, Archived }

public enum Priority { Low, Normal, High, Urgent }

public class TodoList
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}

public class TodoTask
{
    public int Id { get; set; }
    public int ListId { get; set; }
    public string Title { get; set; } = "";
    public string? Notes { get; set; }
    public TaskState Status { get; set; }
    public Priority Priority { get; set; }
    public DateTime? DueAt { get; set; }
    public int? AssigneeId { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? RecurrenceDays { get; set; }
    public int? ParentTaskId { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<TodoTask> Subtasks { get; set; } = [];
}

public class TodoDb(DbContextOptions<TodoDb> options) : DbContext(options)
{
    public DbSet<TodoList> Lists => Set<TodoList>();
    public DbSet<TodoTask> Tasks => Set<TodoTask>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<TodoTask>()
            .HasMany(t => t.Subtasks)
            .WithOne()
            .HasForeignKey(t => t.ParentTaskId);
    }
}

// Raised when a business rule is violated (as opposed to a missing entity). The
// controllers map this to 422; KeyNotFoundException maps to 404.
public class DomainException(string message) : Exception(message);
