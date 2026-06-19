using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Smoower.Minified.EFCore;

public static class InMemoryRegistrationExtensions
{
    public static IServiceCollection mem<T>(this IServiceCollection s, string name) where T : DbContext
        => s.AddDbContext<T>(o => o.UseInMemoryDatabase(name));
}
