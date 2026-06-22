using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Smoower.Minified.Tools.Engine;

/// <summary>
/// One rewriter, two directions. <see cref="Expand"/> applies short→long (for the
/// readable view); <see cref="Compact"/> applies the inverse long→short. Renames
/// are confined to precise syntactic positions so short single-letter aliases
/// can't hit ordinary locals:
///   • methods  — only a member-access/binding <c>.Name</c> (<c>.w</c> ↔ <c>.Where</c>)
///   • attributes — only an <c>AttributeSyntax</c> name (<c>HG</c> ↔ <c>HttpGet</c>)
///   • types — a standalone, unqualified identifier (<c>CT</c> ↔ <c>CancellationToken</c>)
/// Phase 1 is syntactic (no semantic model); the minified style guarantees these
/// tokens mean the Smoower vocabulary. Symbol-accurate binding arrives with the
/// terminator work in a later phase.
/// </summary>
public sealed class AliasRewriter : CSharpSyntaxRewriter
{
    private readonly IReadOnlyDictionary<string, string> _methods;
    private readonly IReadOnlyDictionary<string, string> _attrs;
    private readonly IReadOnlyDictionary<string, string> _types;

    private AliasRewriter(
        IReadOnlyDictionary<string, string> methods,
        IReadOnlyDictionary<string, string> attrs,
        IReadOnlyDictionary<string, string> types)
    {
        _methods = methods;
        _attrs = attrs;
        _types = types;
    }

    public static AliasRewriter Expand(AliasMap m) => new(m.MethodToLong, m.AttrToLong, m.TypeToLong);
    public static AliasRewriter Compact(AliasMap m) => new(m.MethodToShort, m.AttrToShort, m.TypeToShort);

    public override SyntaxNode? VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
    {
        var n = (MemberAccessExpressionSyntax)base.VisitMemberAccessExpression(node)!;
        return n.Name is IdentifierNameSyntax id && _methods.TryGetValue(id.Identifier.Text, out var to)
            ? n.WithName(Rename(id, to))
            : n;
    }

    public override SyntaxNode? VisitMemberBindingExpression(MemberBindingExpressionSyntax node)
    {
        var n = (MemberBindingExpressionSyntax)base.VisitMemberBindingExpression(node)!;
        return n.Name is IdentifierNameSyntax id && _methods.TryGetValue(id.Identifier.Text, out var to)
            ? n.WithName(Rename(id, to))
            : n;
    }

    public override SyntaxNode? VisitAttribute(AttributeSyntax node)
    {
        var n = (AttributeSyntax)base.VisitAttribute(node)!;
        return n.Name is IdentifierNameSyntax id && _attrs.TryGetValue(id.Identifier.Text, out var to)
            ? n.WithName(Rename(id, to))
            : n;
    }

    public override SyntaxNode? VisitIdentifierName(IdentifierNameSyntax node)
    {
        // Names owned by a more specific position are handled there, not as types.
        if (node.Parent is MemberAccessExpressionSyntax ma && ma.Name == node) return node;
        if (node.Parent is MemberBindingExpressionSyntax mb && mb.Name == node) return node;
        if (node.Parent is AttributeSyntax) return node;
        // Aliases are unqualified by convention — never touch a member of A.B.C.
        if (node.Parent is QualifiedNameSyntax) return node;

        return _types.TryGetValue(node.Identifier.Text, out var to) ? Rename(node, to) : node;
    }

    private static IdentifierNameSyntax Rename(IdentifierNameSyntax id, string to) =>
        SyntaxFactory.IdentifierName(SyntaxFactory.Identifier(to)).WithTriviaFrom(id);
}
