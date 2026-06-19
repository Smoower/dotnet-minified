using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Smoower.Minified.EFCore;

public static class SqlServerRegistrationExtensions
{
    public static IServiceCollection sql<T>(this IServiceCollection s, string connectionString) where T : DbContext
        => s.AddDbContext<T>(o => o.UseSqlServer(connectionString));
}
