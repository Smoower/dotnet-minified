using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Smoower.Minified.Hosting;

namespace Smoower.Minified.AspNetCore;

// Program.cs bootstrap shorteners. `ctrl` registers MVC controllers and `ctx`
// registers a DbContext (provider-agnostic). Both come in an IServiceCollection
// form (chain off b.Services) and a WebApplicationBuilder form (chain off the
// builder itself and keep it for Build()). The DI shorteners single/scoped/trans
// from Smoower.Minified.Hosting are re-exposed on WebApplicationBuilder here so a
// whole registration block stays a single fluent chain.
public static class BuilderExtensions
{
    public static IServiceCollection ctrl(this IServiceCollection s)
    {
        s.AddControllers();
        return s;
    }

    public static WebApplicationBuilder ctrl(this WebApplicationBuilder b)
    {
        b.Services.AddControllers();
        return b;
    }

    public static WebApplicationBuilder ctx<T>(this WebApplicationBuilder b, Action<DbContextOptionsBuilder> opt) where T : DbContext
    {
        b.Services.AddDbContext<T>(opt);
        return b;
    }

    public static WebApplicationBuilder single<T>(this WebApplicationBuilder b) where T : class
    {
        b.Services.single<T>();
        return b;
    }

    public static WebApplicationBuilder single<TService, TImpl>(this WebApplicationBuilder b)
        where TService : class where TImpl : class, TService
    {
        b.Services.single<TService, TImpl>();
        return b;
    }

    public static WebApplicationBuilder scoped<T>(this WebApplicationBuilder b) where T : class
    {
        b.Services.scoped<T>();
        return b;
    }

    public static WebApplicationBuilder scoped<TService, TImpl>(this WebApplicationBuilder b)
        where TService : class where TImpl : class, TService
    {
        b.Services.scoped<TService, TImpl>();
        return b;
    }

    public static WebApplicationBuilder trans<T>(this WebApplicationBuilder b) where T : class
    {
        b.Services.trans<T>();
        return b;
    }

    public static WebApplicationBuilder trans<TService, TImpl>(this WebApplicationBuilder b)
        where TService : class where TImpl : class, TService
    {
        b.Services.trans<TService, TImpl>();
        return b;
    }
}
