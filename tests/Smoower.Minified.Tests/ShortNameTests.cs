using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text.Json;
using JPN = System.Text.Json.Serialization.JsonPropertyNameAttribute;
using Smoower.Minified.EFCore;
using Xunit;

namespace Smoower.Minified.Tests;

public class ShortNameTests
{
    private class Row
    {
        [Col("RecurrenceDays")] public int? RD { get; set; }
    }

    private class Dto
    {
        [JPN("recurrenceDays")] public int? RD { get; set; }
    }

    [Fact]
    public void Col_IsResolvedAsColumnWithLongName()
    {
        var attr = typeof(Row).GetProperty(nameof(Row.RD))!.GetCustomAttribute<ColumnAttribute>();
        Assert.NotNull(attr);
        Assert.Equal("RecurrenceDays", attr!.Name);
    }

    [Fact]
    public void Jpn_PinsTheWireName()
        => Assert.Equal("{\"recurrenceDays\":7}", JsonSerializer.Serialize(new Dto { RD = 7 }));
}
