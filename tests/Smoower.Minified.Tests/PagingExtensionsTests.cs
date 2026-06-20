using Microsoft.AspNetCore.Mvc;
using Smoower.Minified.AspNetCore;
using Smoower.Minified.EFCore;
using Xunit;

namespace Smoower.Minified.Tests;

public class PagingExtensionsTests
{
    private static async Task<TestDb> Seed(string name, int n = 25)
    {
        var db = TestDbFactory.Create(name);
        for (var i = 1; i <= n; i++) db.Things.Add(new Thing { Name = $"t{i:00}", Rank = i });
        await db.SaveChangesAsync();
        return db;
    }

    [Fact]
    public async Task WhereIf_AppliesPredicateWhenTrue()
        => Assert.Equal(5, await (await Seed(nameof(WhereIf_AppliesPredicateWhenTrue))).Things.whereIf(true, t => t.Rank <= 5).cnt());

    [Fact]
    public async Task WhereIf_NoOpWhenFalse()
        => Assert.Equal(25, await (await Seed(nameof(WhereIf_NoOpWhenFalse))).Things.whereIf(false, t => t.Rank <= 5).cnt());

    [Fact]
    public async Task Paged_ReturnsEnvelopeAndTakesPage()
    {
        var db = await Seed(nameof(Paged_ReturnsEnvelopeAndTakesPage));
        var ok = Assert.IsType<OkObjectResult>(await db.Things.ob(t => t.Rank).paged(2, 10));
        var p = Assert.IsType<PagedResult<Thing>>(ok.Value);
        Assert.Equal(25, p.Total);
        Assert.Equal(2, p.Page);
        Assert.Equal(10, p.PageSize);
        Assert.Equal(10, p.Items.Count);
        Assert.Equal(11, p.Items[0].Rank);
    }

    [Fact]
    public async Task Paged_Projects()
    {
        var db = await Seed(nameof(Paged_Projects));
        var ok = Assert.IsType<OkObjectResult>(await db.Things.ob(t => t.Rank).paged(1, 3, t => t.Name));
        var p = Assert.IsType<PagedResult<string>>(ok.Value);
        Assert.Equal(new[] { "t01", "t02", "t03" }, p.Items);
        Assert.Equal(25, p.Total);
    }

    [Fact]
    public async Task Paged_ClampsPageSizeToMaxAndPageToOne()
    {
        var db = await Seed(nameof(Paged_ClampsPageSizeToMaxAndPageToOne));
        var ok = Assert.IsType<OkObjectResult>(await db.Things.ob(t => t.Rank).paged(0, 9999, CancellationToken.None, max: 20));
        var p = Assert.IsType<PagedResult<Thing>>(ok.Value);
        Assert.Equal(1, p.Page);
        Assert.Equal(20, p.PageSize);
        Assert.Equal(20, p.Items.Count);
    }

    [Fact]
    public async Task WhereIf_ChainsIntoPaged()
    {
        var db = await Seed(nameof(WhereIf_ChainsIntoPaged));
        DateTime? since = null;
        int floor = 20;
        var ok = Assert.IsType<OkObjectResult>(
            await db.Things.whereIf(since.HasValue, t => t.Rank >= 0).whereIf(true, t => t.Rank >= floor).ob(t => t.Rank).paged(1, 50));
        var p = Assert.IsType<PagedResult<Thing>>(ok.Value);
        Assert.Equal(6, p.Total);
    }
}
