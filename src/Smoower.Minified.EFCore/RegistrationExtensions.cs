using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Smoower.Minified.EFCore;

// Provider-agnostic DbContext registration. `ctx` fuses the AddDbContext call so
// a Program.cs registers a context in one chained term. The provider-specific
// shorteners (sql/pg/sqlite/mem) live in the Smoower.Minified.EFCore.* satellite
// packages so this package keeps zero provider dependencies; each satellite adds
// its method into this same namespace.
public static class RegistrationExtensions
{
    public static IServiceCollection ctx<T>(this IServiceCollection s, Action<DbContextOptionsBuilder> opt) where T : DbContext
        => s.AddDbContext<T>(opt);
}
