using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Smoower.Minified.Tools.Engine;

/// <summary>
/// Phase 0 vocab.json generator. Type aliases are harvested from a real
/// GlobalUsings.cs (the single machine-readable source: <c>global using X = Y;</c>).
/// Attributes/methods/terminators are seeded from the documented vocabulary for
/// now; Phase 1 replaces the seed with a <c>[MinifiedAlias]</c> harvest off the
/// packages so prompt + CLI + extension share ONE source of truth (kills the
/// drift recorded in planning/design.md:146).
/// </summary>
public static class VocabGenerator
{
    public static string Generate(string? globalUsingsSource)
    {
        var typeAliases = globalUsingsSource is null
            ? new Dictionary<string, Entry>()
            : HarvestTypeAliases(globalUsingsSource);

        var vocab = new Vocab
        {
            Version = "0.1",
            Language = "csharp",
            TypeAliases = typeAliases,
            Attributes = SeedAttributes,
            Methods = SeedMethods,
            Terminators = SeedTerminators,
        };

        var opts = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };
        return JsonSerializer.Serialize(vocab, opts);
    }

    private static Dictionary<string, Entry> HarvestTypeAliases(string source)
    {
        var root = (CompilationUnitSyntax)CSharpSyntaxTree.ParseText(source).GetRoot();
        var result = new Dictionary<string, Entry>();
        foreach (var u in root.DescendantNodes().OfType<UsingDirectiveSyntax>())
        {
            if (u.Alias is null || u.NamespaceOrType is null) continue;
            var name = u.Alias.Name.Identifier.Text;
            var full = u.NamespaceOrType.ToString();
            var display = Simplify(u.NamespaceOrType);
            result[name] = new Entry { Long = display, Full = full == display ? null : full };
        }
        return result;
    }

    /// <summary>Strip namespace qualifiers so Tr -> Task&lt;IActionResult&gt; (not the fully-qualified form).</summary>
    private static string Simplify(TypeSyntax type) =>
        ((TypeSyntax)new NamespaceStripper().Visit(type)).ToString();

    private sealed class NamespaceStripper : CSharpSyntaxRewriter
    {
        public override SyntaxNode? VisitQualifiedName(QualifiedNameSyntax node) => Visit(node.Right);
    }

    private sealed class Vocab
    {
        [JsonPropertyName("version")] public string Version { get; set; } = "";
        [JsonPropertyName("language")] public string Language { get; set; } = "";
        [JsonPropertyName("typeAliases")] public Dictionary<string, Entry> TypeAliases { get; set; } = new();
        [JsonPropertyName("attributes")] public Dictionary<string, Entry> Attributes { get; set; } = new();
        [JsonPropertyName("methods")] public Dictionary<string, Entry> Methods { get; set; } = new();
        [JsonPropertyName("terminators")] public Dictionary<string, Entry> Terminators { get; set; } = new();
    }

    private sealed class Entry
    {
        [JsonPropertyName("long")] public string Long { get; set; } = "";
        [JsonPropertyName("full")] public string? Full { get; set; }
        [JsonPropertyName("note")] public string? Note { get; set; }
    }

    private static Entry E(string @long, string? note = null) => new() { Long = @long, Note = note };

    private static readonly Dictionary<string, Entry> SeedAttributes = new()
    {
        ["API"] = E("ApiController"), ["RT"] = E("Route"),
        ["HG"] = E("HttpGet"), ["HPO"] = E("HttpPost"), ["HPU"] = E("HttpPut"),
        ["HPA"] = E("HttpPatch"), ["HD"] = E("HttpDelete"),
        ["AUTH"] = E("Authorize"), ["ANON"] = E("AllowAnonymous"),
        ["FB"] = E("FromBody"), ["FR"] = E("FromRoute"), ["FQ"] = E("FromQuery"), ["FH"] = E("FromHeader"),
        ["P200"] = E("ProducesResponseType(200)"), ["P201"] = E("ProducesResponseType(201)"),
        ["P204"] = E("ProducesResponseType(204)"), ["P400"] = E("ProducesResponseType(400)"),
        ["P404"] = E("ProducesResponseType(404)"),
        ["F"] = E("Fact"), ["Th"] = E("Theory"), ["In"] = E("InlineData"), ["Mem"] = E("MemberData"),
    };

    private static readonly Dictionary<string, Entry> SeedMethods = new()
    {
        // EF query
        ["w"] = E("Where"), ["s"] = E("Select"),
        ["ob"] = E("OrderBy"), ["obd"] = E("OrderByDescending"),
        ["tb"] = E("ThenBy"), ["tbd"] = E("ThenByDescending"),
        ["sk"] = E("Skip"), ["tk"] = E("Take"), ["nt"] = E("AsNoTracking"), ["inc"] = E("Include"),
        ["lst"] = E("ToListAsync"), ["one"] = E("FirstOrDefaultAsync"),
        ["single"] = E("SingleOrDefaultAsync"), ["any"] = E("AnyAsync"), ["cnt"] = E("CountAsync"),
        // EF write
        ["save"] = E("SaveChangesAsync"), ["upd"] = E("Update"), ["del"] = E("Remove"),
        // guards
        ["nil"] = E("string.IsNullOrWhiteSpace"), ["emp"] = E("is empty", "collection-empty guard"),
        ["none"] = E("!Any()"),
        // logging
        ["inf"] = E("LogInformation"), ["wrn"] = E("LogWarning"), ["err"] = E("LogError"), ["dbg"] = E("LogDebug"),
        // DI
        ["scoped"] = E("AddScoped"), ["trans"] = E("AddTransient"),
        // json / http
        ["toJson"] = E("JsonSerializer.Serialize"), ["fromJson"] = E("JsonSerializer.Deserialize"),
        ["getJson"] = E("GetFromJsonAsync"), ["postJson"] = E("PostAsJsonAsync"),
    };

    private static readonly Dictionary<string, Entry> SeedTerminators = new()
    {
        ["ok1"] = E("FirstOrDefaultAsync + 200/404", "single: 200 with value, 404 if null"),
        ["okl"] = E("ToListAsync + 200", "200 with list"),
        ["okc"] = E("CountAsync + 200", "200 with count"),
        ["okId"] = E("Set.FindAsync + 200/404", "200 by key, 404 if missing"),
        ["okAdd"] = E("Add + SaveChangesAsync + 200", "add then 200 with entity"),
        ["okNew"] = E("Add + SaveChangesAsync + 201", "add then 201 Created"),
        ["created"] = E("201 Created", "wrap value as 201"),
        ["delById"] = E("Find + Remove + Save + 204/404", "delete by key, 204 or 404"),
    };
}
