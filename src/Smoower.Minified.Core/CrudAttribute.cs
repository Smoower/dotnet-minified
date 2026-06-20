namespace Smoower.Minified.Core;

[Flags]
public enum Verbs
{
    None = 0,
    Get = 1,
    List = 2,
    Create = 4,
    Update = 8,
    Delete = 16,
    Read = Get | List,
    Write = Create | Update | Delete,
    All = Read | Write,
}

[AttributeUsage(AttributeTargets.Class)]
public sealed class CrudAttribute<TEntity, TIn, TOut>(string route) : Attribute
    where TEntity : class
{
    public string Route { get; } = route;
    public Verbs Only { get; set; } = Verbs.All;
    public Verbs Except { get; set; } = Verbs.None;
    public string? Key { get; set; }
}
