using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Smoower.Minified.AspNetCore;

// The standard paged-list envelope: items plus the counts a client needs to page.
public record PagedResult<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize);

// Result-fusing terminator for the dominant read pattern on list endpoints: clamp
// the page args, count the filtered query, take one page, and wrap it in a 200 with
// the PagedResult envelope. Collapses the clamp + CountAsync + Skip/Take + Ok(new{...})
// ceremony to one call. Order the query before calling this; paging needs it.
public static class PagingExtensions
{
    public static async Task<IActionResult> paged<T>(this IQueryable<T> q, int page, int pageSize, CancellationToken ct = default, int max = 200)
    {
        pageSize = Math.Clamp(pageSize, 1, max);
        page = Math.Max(page, 1);
        var total = await q.CountAsync(ct);
        var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new OkObjectResult(new PagedResult<T>(items, total, page, pageSize));
    }

    public static async Task<IActionResult> paged<T, TOut>(this IQueryable<T> q, int page, int pageSize, Expression<Func<T, TOut>> selector, CancellationToken ct = default, int max = 200)
    {
        pageSize = Math.Clamp(pageSize, 1, max);
        page = Math.Max(page, 1);
        var total = await q.CountAsync(ct);
        var items = await q.Skip((page - 1) * pageSize).Take(pageSize).Select(selector).ToListAsync(ct);
        return new OkObjectResult(new PagedResult<TOut>(items, total, page, pageSize));
    }
}
