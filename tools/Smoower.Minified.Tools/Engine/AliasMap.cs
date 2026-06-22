using System.Text.Json;
using System.Text.RegularExpressions;

namespace Smoower.Minified.Tools.Engine;

/// <summary>
/// Bijective short&lt;-&gt;long alias tables loaded from vocab.json — the SAME file
/// the prompt and hover use, so there's one source of truth. Phase 1 only
/// admits aliases whose <c>long</c> form is a single simple identifier
/// (e.g. <c>w</c>→<c>Where</c>, <c>HG</c>→<c>HttpGet</c>, <c>CT</c>→
/// <c>CancellationToken</c>). Non-1:1 expansions (<c>Tr</c>→<c>Task&lt;IActionResult&gt;</c>,
/// <c>P200</c>, <c>nil()</c>, the fused terminators) are skipped here — they need
/// structural matching / semantic info and land in later phases. Skipping them
/// keeps expand and compact exact inverses, so <c>compact(expand(x))==x</c> holds.
/// </summary>
public sealed partial class AliasMap
{
    public IReadOnlyDictionary<string, string> MethodToLong { get; }
    public IReadOnlyDictionary<string, string> MethodToShort { get; }
    public IReadOnlyDictionary<string, string> AttrToLong { get; }
    public IReadOnlyDictionary<string, string> AttrToShort { get; }
    public IReadOnlyDictionary<string, string> TypeToLong { get; }
    public IReadOnlyDictionary<string, string> TypeToShort { get; }

    private AliasMap(
        Dictionary<string, string> methods,
        Dictionary<string, string> attrs,
        Dictionary<string, string> types)
    {
        MethodToLong = methods;
        MethodToShort = Invert(methods);
        AttrToLong = attrs;
        AttrToShort = Invert(attrs);
        TypeToLong = types;
        TypeToShort = Invert(types);
    }

    public static AliasMap Load(string vocabPath)
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(vocabPath));
        var root = doc.RootElement;
        return new AliasMap(
            ReadSimple(root, "methods"),
            ReadSimple(root, "attributes"),
            ReadSimple(root, "typeAliases"));
    }

    private static Dictionary<string, string> ReadSimple(JsonElement root, string section)
    {
        var fwd = new Dictionary<string, string>(StringComparer.Ordinal);
        if (root.TryGetProperty(section, out var sec) && sec.ValueKind == JsonValueKind.Object)
        {
            foreach (var p in sec.EnumerateObject())
            {
                if (!p.Value.TryGetProperty("long", out var l)) continue;
                var lng = l.GetString();
                if (lng is not null && SimpleId().IsMatch(lng))
                    fwd[p.Name] = lng;
            }
        }
        return fwd;
    }

    private static Dictionary<string, string> Invert(Dictionary<string, string> fwd)
    {
        var rev = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var kv in fwd) rev[kv.Value] = kv.Key; // longs are unique within a section
        return rev;
    }

    [GeneratedRegex(@"^[A-Za-z_][A-Za-z0-9_]*$")]
    private static partial Regex SimpleId();
}
