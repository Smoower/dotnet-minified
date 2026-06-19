using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Smoower.Minified.EFCore;

public static class SqliteRegistrationExtensions
{
    public static IServiceCollection sqlite<T>(this IServiceCollection s, string connectionString) where T : DbContext
        => s.AddDbContext<T>(o => o.UseSqlite(connectionString));
}
