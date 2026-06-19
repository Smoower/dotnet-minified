using Smoower.Minified.Extensions;
using Xunit;

namespace Smoower.Minified.Tests;

public class ExtensionsTests
{
    [Fact]
    public void IntFactories_BuildTimeSpans()
    {
        Assert.Equal(TimeSpan.FromMilliseconds(250), 250.ms());
        Assert.Equal(TimeSpan.FromSeconds(30), 30.secs());
        Assert.Equal(TimeSpan.FromMinutes(5), 5.mins());
        Assert.Equal(TimeSpan.FromHours(2), 2.hrs());
        Assert.Equal(TimeSpan.FromDays(7), 7.days());
    }

    [Fact]
    public void Clock_ReflectsNow()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var c = new Clock();
        Assert.InRange(c.utc, before, DateTime.UtcNow.AddSeconds(1));
        Assert.True(c.unix > 0);
        Assert.Equal(DateOnly.FromDateTime(DateTime.UtcNow), c.today);
    }

    [Fact]
    public void Clk_StaticMirror()
    {
        Assert.True(Clk.unix > 0);
        Assert.Equal(DateOnly.FromDateTime(DateTime.UtcNow), Clk.today);
    }

    [Fact]
    public void Env_RoundTrips()
    {
        Env.set("SMOOWER_TEST_VAR", "hello");
        Assert.Equal("hello", Env.get("SMOOWER_TEST_VAR"));
    }

    [Fact]
    public void DateExtensions_MatchBcl()
    {
        var dt = new DateTime(2026, 6, 19, 13, 45, 0, DateTimeKind.Utc);
        Assert.Equal(dt.ToShortDateString(), dt.sd());
        Assert.Equal(dt.ToLongDateString(), dt.ld());
        Assert.Equal(dt.ToShortTimeString(), dt.st());
        Assert.Equal(dt.ToLongTimeString(), dt.lt());
        Assert.Equal(dt.ToUniversalTime(), dt.utc());
    }
}
