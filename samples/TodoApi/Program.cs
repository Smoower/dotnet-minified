using Microsoft.EntityFrameworkCore;
using TodoApi;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddDbContext<TodoDb>(o => o.UseInMemoryDatabase("todo"));
builder.Services.AddScoped<TaskService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TodoDb>();
    var list = new TodoList { Name = "Inbox", CreatedAt = DateTime.UtcNow };
    db.Lists.Add(list);
    db.SaveChanges();
    db.Tasks.AddRange(
        new TodoTask { ListId = list.Id, Title = "Write the spec", Priority = Priority.High, Status = TaskState.Todo, DueAt = DateTime.UtcNow.AddDays(2), CreatedAt = DateTime.UtcNow },
        new TodoTask { ListId = list.Id, Title = "Review the PR", Priority = Priority.Normal, Status = TaskState.InProgress, AssigneeId = 1, CreatedAt = DateTime.UtcNow });
    db.SaveChanges();
}

app.MapControllers();
app.Run();
