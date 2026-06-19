namespace Smoower.Minified.Extensions;

// An injectable clock so time stays testable. Short members replace the long BCL
// names: clock.utc for DateTime.UtcNow, clock.unix for the Unix timestamp, etc.
public sealed class Clock
{
    public DateTime utc => DateTime.UtcNow;
    public DateTime now => DateTime.Now;
    public DateOnly today => DateOnly.FromDateTime(DateTime.UtcNow);
    public long unix => DateTimeOffset.UtcNow.ToUnixTimeSeconds();
}

// Static mirror for code that can't take a Clock dependency (scripts, top-level
// statements). Prefer the injectable Clock where testability matters.
public static class Clk
{
    public static DateTime utc => DateTime.UtcNow;
    public static DateTime now => DateTime.Now;
    public static DateOnly today => DateOnly.FromDateTime(DateTime.UtcNow);
    public static long unix => DateTimeOffset.UtcNow.ToUnixTimeSeconds();
}

// DateTime / TimeSpan shorteners. Only the long, multi-token BCL members are
// wrapped. One-token members (ToString) are deliberately left out: aliasing them
// saves ~0 tokens on Claude and the model emits the long form from habit anyway.
public static class TimeExtensions
{
    public static DateTime utc(this DateTime dt) => dt.ToUniversalTime();
    public static string sd(this DateTime dt) => dt.ToShortDateString();
    public static string ld(this DateTime dt) => dt.ToLongDateString();
    public static string st(this DateTime dt) => dt.ToShortTimeString();
    public static string lt(this DateTime dt) => dt.ToLongTimeString();

    public static TimeSpan ms(this int n) => TimeSpan.FromMilliseconds(n);
    public static TimeSpan secs(this int n) => TimeSpan.FromSeconds(n);
    public static TimeSpan mins(this int n) => TimeSpan.FromMinutes(n);
    public static TimeSpan hrs(this int n) => TimeSpan.FromHours(n);
    public static TimeSpan days(this int n) => TimeSpan.FromDays(n);
}
