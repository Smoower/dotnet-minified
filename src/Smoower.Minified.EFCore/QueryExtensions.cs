using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Smoower.Minified.EFCore;

// IQueryable / EF Core query shorteners. Predicates and projections use
// Expression<Func<...>> so EF Core can translate them to SQL.
public static class QueryExtensions
{
    public static IQueryable<T> w<T>(this IQueryable<T> q, Expression<Func<T, bool>> predicate)
        => q.Where(predicate);

    // Conditional Where: apply the predicate only when condition is true. Collapses
    // the `if (filter.HasValue) q = q.Where(...)` chains that dominate filtered list
    // endpoints into a single fluent expression.
    public static IQueryable<T> whereIf<T>(this IQueryable<T> q, bool condition, Expression<Func<T, bool>> predicate)
        => condition ? q.Where(predicate) : q;

    public static IQueryable<TResult> s<T, TResult>(this IQueryable<T> q, Expression<Func<T, TResult>> selector)
        => q.Select(selector);

    public static IOrderedQueryable<T> ob<T, TKey>(this IQueryable<T> q, Expression<Func<T, TKey>> key)
        => q.OrderBy(key);

    public static IOrderedQueryable<T> obd<T, TKey>(this IQueryable<T> q, Expression<Func<T, TKey>> key)
        => q.OrderByDescending(key);

    public static IOrderedQueryable<T> tb<T, TKey>(this IOrderedQueryable<T> q, Expression<Func<T, TKey>> key)
        => q.ThenBy(key);

    public static IOrderedQueryable<T> tbd<T, TKey>(this IOrderedQueryable<T> q, Expression<Func<T, TKey>> key)
        => q.ThenByDescending(key);

    public static IQueryable<T> sk<T>(this IQueryable<T> q, int count)
        => q.Skip(count);

    public static IQueryable<T> tk<T>(this IQueryable<T> q, int count)
        => q.Take(count);

    public static IQueryable<T> nt<T>(this IQueryable<T> q) where T : class
        => q.AsNoTracking();

    public static IIncludableQueryable<T, TProperty> inc<T, TProperty>(this IQueryable<T> q, Expression<Func<T, TProperty>> path) where T : class
        => q.Include(path);

    public static Task<List<T>> lst<T>(this IQueryable<T> q, CancellationToken ct = default)
        => q.ToListAsync(ct);

    public static Task<T?> one<T>(this IQueryable<T> q, CancellationToken ct = default)
        => q.FirstOrDefaultAsync(ct);

    public static Task<T?> single<T>(this IQueryable<T> q, CancellationToken ct = default)
        => q.SingleOrDefaultAsync(ct);

    public static Task<bool> any<T>(this IQueryable<T> q, CancellationToken ct = default)
        => q.AnyAsync(ct);

    public static Task<int> cnt<T>(this IQueryable<T> q, CancellationToken ct = default)
        => q.CountAsync(ct);

    public static Task<TResult> max<T, TResult>(this IQueryable<T> q, Expression<Func<T, TResult>> selector, CancellationToken ct = default)
        => q.MaxAsync(selector, ct);

    public static Task<TResult> min<T, TResult>(this IQueryable<T> q, Expression<Func<T, TResult>> selector, CancellationToken ct = default)
        => q.MinAsync(selector, ct);

    public static IQueryable<T> ntir<T>(this IQueryable<T> q) where T : class
        => q.AsNoTrackingWithIdentityResolution();

    public static IIncludableQueryable<TEntity, TProperty> tinc<TEntity, TPrev, TProperty>(
        this IIncludableQueryable<TEntity, IEnumerable<TPrev>> q, Expression<Func<TPrev, TProperty>> path) where TEntity : class
        => q.ThenInclude(path);

    public static IIncludableQueryable<TEntity, TProperty> tinc<TEntity, TPrev, TProperty>(
        this IIncludableQueryable<TEntity, TPrev> q, Expression<Func<TPrev, TProperty>> path) where TEntity : class
        => q.ThenInclude(path);

    public static IQueryable<IGrouping<TKey, T>> gb<T, TKey>(this IQueryable<T> q, Expression<Func<T, TKey>> key)
        => q.GroupBy(key);

    // Synchronous terminators (suffix S). See WriteExtensions for the rationale:
    // async stays unmarked because that's where the token savings are.
    public static List<T> lstS<T>(this IQueryable<T> q) => q.ToList();

    public static T? oneS<T>(this IQueryable<T> q) => q.FirstOrDefault();

    public static T? singleS<T>(this IQueryable<T> q) => q.SingleOrDefault();

    public static bool anyS<T>(this IQueryable<T> q) => q.Any();

    public static int cntS<T>(this IQueryable<T> q) => q.Count();
}
