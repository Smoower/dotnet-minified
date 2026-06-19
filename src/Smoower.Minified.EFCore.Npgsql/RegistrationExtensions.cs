using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Smoower.Minified.EFCore;

public static class NpgsqlRegistrationExtensions
{
    public static IServiceCollection pg<T>(this IServiceCollection s, string connectionString) where T : DbContext
        => s.AddDbContext<T>(o => o.UseNpgsql(connectionString));
}
