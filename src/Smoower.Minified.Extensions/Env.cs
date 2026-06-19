namespace Smoower.Minified.Extensions;

// Environment-variable access. GetEnvironmentVariable is long and common in
// startup / design-time factories.
public static class Env
{
    public static string? get(string name) => Environment.GetEnvironmentVariable(name);
    public static void set(string name, string? value) => Environment.SetEnvironmentVariable(name, value);
}
