using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Smoower.Minified.Tools.Engine;

/// <summary>
/// Phase 0 compactor: strip comments and pack whitespace via Roslyn's
/// NormalizeWhitespace. This is the reverse of <see cref="Expander"/>; together
/// they prove the round-trip plumbing before any vocabulary logic exists.
/// Compaction discards ALL whitespace, so reflow is free in the round trip.
/// </summary>
public static class Compactor
{
    public static string Compact(string source, AliasMap? aliases = null)
    {
        var tree = CSharpSyntaxTree.ParseText(source);
        var root = (CompilationUnitSyntax)tree.GetRoot();
        root = (CompilationUnitSyntax)new CommentStripper().Visit(root);

        if (aliases is not null)
            root = (CompilationUnitSyntax)AliasRewriter.Compact(aliases).Visit(root)!;

        return root.NormalizeWhitespace(indentation: "", eol: "").ToFullString();
    }

    /// <summary>Removes comment and documentation trivia (L3 carries no comments).</summary>
    private sealed class CommentStripper : CSharpSyntaxRewriter
    {
        public CommentStripper() : base(visitIntoStructuredTrivia: true) { }

        public override SyntaxTrivia VisitTrivia(SyntaxTrivia trivia)
        {
            switch (trivia.Kind())
            {
                case SyntaxKind.SingleLineCommentTrivia:
                case SyntaxKind.MultiLineCommentTrivia:
                case SyntaxKind.SingleLineDocumentationCommentTrivia:
                case SyntaxKind.MultiLineDocumentationCommentTrivia:
                    return default; // drop it
                default:
                    return trivia;
            }
        }
    }
}
