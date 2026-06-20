// SHORT-NAMED form — what the AI authors (illustration; excluded from compile).
// Rules applied:
//  - hot TYPES get a global using alias (entity/DTO only; never enums)
//  - multi-token, high-use PROPERTIES get a short name + [Col] pinning the column
//  - single-token props (Id/Title/Notes/Status) left alone — no token headroom
//  - enum TYPES never get a global using alias (bad code: obscures switch/nameof);
//    enum values could be shortened via [JsonStringEnumMemberName] but the full
//    type name dominates the reference, so it rarely pays — left as-is here
// The DB schema is identical (every column pinned). Long forms: see names.map /
// the attributes / Domain.cs.

// --- belongs in GlobalUsings.cs ---
global using TT = TodoApi.TodoTask;
global using TL = TodoApi.TodoList;

using Smoower.Minified.EFCore;
namespace TodoApi;

public enum TaskState { Todo, InProgress, Blocked, Done, Archived }
public enum Priority { Low, Normal, High, Urgent }

public class TodoList {
 public int Id { get; set; }
 public string Name { get; set; } = "";
 [Col("CreatedAt")] public DateTime CA { get; set; }
}

public class TodoTask {
 public int Id { get; set; }
 [Col("ListId")] public int LId { get; set; }
 public string Title { get; set; } = "";
 public string? Notes { get; set; }
 public TaskState Status { get; set; }
 public Priority Priority { get; set; }
 [Col("DueAt")] public DateTime? DA { get; set; }
 [Col("AssigneeId")] public int? AID { get; set; }
 [Col("CompletedAt")] public DateTime? CmA { get; set; }
 [Col("RecurrenceDays")] public int? RD { get; set; }
 [Col("ParentTaskId")] public int? PTI { get; set; }
 [Col("CreatedAt")] public DateTime CA { get; set; }
 public List<TT> Subtasks { get; set; } = [];
}
