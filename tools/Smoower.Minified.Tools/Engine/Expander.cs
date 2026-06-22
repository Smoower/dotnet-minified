using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Smoower.Minified.Tools.Engine;

/// <summary>
/// Phase 0 expander: reflow only. Parses the (possibly single-line, L3-packed)
/// source and re-emits it with readable indentation via Roslyn's
/// NormalizeWhitespace. Alias resolution (Tier A/B) and terminator expansion
/// (Tier C) are layered on top in later phases; the format pass always runs last.
/// </summary>
public static class Expander
{
    public static string Expand(string source, AliasMap? aliases = null)
    {
        var tree = CSharpSyntaxTree.ParseText(source);
        var root = (CompilationUnitSyntax)tree.GetRoot();

        if (aliases is not null)
            root = (CompilationUnitSyntax)AliasRewriter.Expand(aliases).Visit(root)!;

        // Phase 3+: root = TerminatorExpander.Expand(root, semanticModel);

        return root.NormalizeWhitespace(indentation: "    ", eol: "\n").ToFullString();
    }
}
