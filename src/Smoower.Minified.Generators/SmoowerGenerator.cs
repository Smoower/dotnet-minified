using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Smoower.Minified.Generators;

[Generator(LanguageNames.CSharp)]
public sealed class SmoowerGenerator : IIncrementalGenerator
{
    const string CrudMeta = "Smoower.Minified.Core.CrudAttribute`3";

    static readonly DiagnosticDescriptor NotPartial = new(
        "SMOO001",
        "Crud controller must be partial",
        "Controller '{0}' has [Crud<>] but is not declared 'partial'; the generator cannot add its actions",
        "Smoower.Minified",
        DiagnosticSeverity.Error,
        true);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var models = context.SyntaxProvider.ForAttributeWithMetadataName(
            CrudMeta,
            predicate: static (n, _) => n is ClassDeclarationSyntax,
            transform: static (ctx, ct) => CrudConvention.Build(ctx, ct));

        context.RegisterSourceOutput(models, static (spc, m) =>
        {
            if (m is null) return;
            if (!m.IsPartial)
            {
                spc.ReportDiagnostic(Diagnostic.Create(NotPartial, m.Location, m.Name));
                return;
            }
            spc.AddSource(m.HintName, CrudEmitter.Emit(m));
        });
    }
}

public sealed record CrudModel(
    string HintName,
    bool IsPartial,
    Location? Location,
    string Name,
    string? Namespace,
    string Route,
    string EntityFq,
    string TInFq,
    string TOutFq,
    string DbField,
    string DbSet,
    string? ValField,
    string KeyName,
    string KeyTypeFq,
    string ProjArgs,
    string EntityInitArgs,
    string PutAssigns,
    bool EmitGet,
    bool EmitList,
    bool EmitCreate,
    bool EmitUpdate,
    bool EmitDelete);

public static class CrudConvention
{
    static readonly SymbolDisplayFormat Fq = SymbolDisplayFormat.FullyQualifiedFormat;

    public static CrudModel? Build(GeneratorAttributeSyntaxContext ctx, CancellationToken ct)
    {
        if (ctx.TargetSymbol is not INamedTypeSymbol type) return null;
        if (ctx.Attributes.Length == 0) return null;
        var attr = ctx.Attributes[0];
        if (attr.AttributeClass is not { TypeArguments.Length: 3 } ac) return null;

        var name = type.Name;
        var ns = type.ContainingNamespace.IsGlobalNamespace ? null : type.ContainingNamespace.ToDisplayString();
        var hint = (ns is null ? name : ns + "." + name).Replace('.', '_') + ".Crud.g.cs";

        var isPartial = ctx.TargetNode is ClassDeclarationSyntax cds && cds.Modifiers.Any(SyntaxKind.PartialKeyword);
        if (!isPartial)
            return new CrudModel(hint, false, ctx.TargetNode.GetLocation(), name, ns, "",
                "", "", "", "", "", null, "", "", "", "", "", false, false, false, false, false);

        var entity = ac.TypeArguments[0];
        var tin = ac.TypeArguments[1];
        var tout = ac.TypeArguments[2];

        var route = attr.ConstructorArguments.Length > 0 ? attr.ConstructorArguments[0].Value as string ?? "" : "";

        int only = 31, except = 0;
        string? key = null;
        foreach (var na in attr.NamedArguments)
        {
            if (na.Key == "Only") only = ToInt(na.Value.Value);
            else if (na.Key == "Except") except = ToInt(na.Value.Value);
            else if (na.Key == "Key") key = na.Value.Value as string;
        }

        var ctor = PrimaryCtor(type);
        IParameterSymbol? dbP = null, valP = null;
        if (ctor is not null)
            foreach (var p in ctor.Parameters)
            {
                if (dbP is null && InheritsFrom(p.Type, "Microsoft.EntityFrameworkCore.DbContext")) dbP = p;
                else if (valP is null && IsValidatorOf(p.Type, tin)) valP = p;
            }

        var entityProps = AllProperties(entity).ToList();

        var keyName = key ?? "Id";
        var keyProp = entityProps.FirstOrDefault(p => string.Equals(p.Name, keyName, StringComparison.OrdinalIgnoreCase));
        var keyTypeFq = keyProp?.Type.ToDisplayString(Fq) ?? "global::System.Int32";
        keyName = keyProp?.Name ?? keyName;

        var entityFq = entity.ToDisplayString(Fq);
        var tinFq = tin.ToDisplayString(Fq);
        var toutFq = tout.ToDisplayString(Fq);

        var dbField = dbP?.Name ?? "db";
        var dbSet = dbP is null ? $"Set<{entityFq}>()" : DbSetName(dbP.Type, entity, entityFq);

        var toutCtor = tout is INamedTypeSymbol tn ? PrimaryCtor(tn) : null;
        var projArgs = toutCtor is null ? "" : string.Join(", ", toutCtor.Parameters.Select(p =>
        {
            var m = Match(entityProps, p.Name);
            return m is null ? "default" : "x." + m.Name;
        }));

        var tinCtor = tin is INamedTypeSymbol tinN ? PrimaryCtor(tinN) : null;
        var pairs = tinCtor is null
            ? new List<(string Prop, string Param)>()
            : tinCtor.Parameters
                .Select(p => (m: Match(entityProps, p.Name), p))
                .Where(t => t.m is not null)
                .Select(t => (t.m!.Name, t.p.Name))
                .ToList();
        var initArgs = string.Join(", ", pairs.Select(t => $"{t.Item1} = r.{t.Item2}"));
        var putAssigns = string.Join(" ", pairs.Select(t => $"x.{t.Item1} = r.{t.Item2};"));

        int occupied = 0;
        foreach (var m in type.GetMembers().OfType<IMethodSymbol>())
            foreach (var a in m.GetAttributes())
                occupied |= Slot(a.AttributeClass?.Name, a.ConstructorArguments.Length > 0 ? a.ConstructorArguments[0].Value as string : null);

        int final = (only & ~except) & ~occupied;

        return new CrudModel(hint, true, null, name, ns, route,
            entityFq, tinFq, toutFq, dbField, dbSet, valP?.Name, keyName, keyTypeFq,
            projArgs, initArgs, putAssigns,
            (final & 1) != 0, (final & 2) != 0, (final & 4) != 0, (final & 8) != 0, (final & 16) != 0);
    }

    static int ToInt(object? o) => o is null ? 0 : Convert.ToInt32(o);

    static IPropertySymbol? Match(List<IPropertySymbol> props, string name)
        => props.FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));

    static int Slot(string? attrName, string? template) => attrName switch
    {
        "HGAttribute" => HasId(template) ? 1 : 2,
        "HPOAttribute" => 4,
        "HPUAttribute" => 8,
        "HDAttribute" => 16,
        _ => 0,
    };

    static bool HasId(string? t) => t is not null && t.Contains("{");

    static IMethodSymbol? PrimaryCtor(INamedTypeSymbol t) => t.InstanceConstructors
        .Where(c => c.Parameters.Length > 0 &&
            !(c.Parameters.Length == 1 && SymbolEqualityComparer.Default.Equals(c.Parameters[0].Type, t)))
        .OrderByDescending(c => c.Parameters.Length)
        .FirstOrDefault();

    static bool InheritsFrom(ITypeSymbol t, string fq)
    {
        for (var b = t; b is not null; b = b.BaseType)
            if (b.ToDisplayString() == fq) return true;
        return false;
    }

    static bool IsValidatorOf(ITypeSymbol t, ITypeSymbol tin)
        => t is INamedTypeSymbol nt && nt.Name == "IValidator" && nt.TypeArguments.Length == 1
           && SymbolEqualityComparer.Default.Equals(nt.TypeArguments[0], tin);

    static string DbSetName(ITypeSymbol dbType, ITypeSymbol entity, string entityFq)
    {
        for (var cur = dbType; cur is not null; cur = cur.BaseType)
            foreach (var m in cur.GetMembers().OfType<IPropertySymbol>())
                if (m.Type is INamedTypeSymbol pt && pt.Name == "DbSet" && pt.TypeArguments.Length == 1
                    && SymbolEqualityComparer.Default.Equals(pt.TypeArguments[0], entity))
                    return m.Name;
        return $"Set<{entityFq}>()";
    }

    static IEnumerable<IPropertySymbol> AllProperties(ITypeSymbol t)
    {
        for (var cur = t; cur is not null && cur.SpecialType != SpecialType.System_Object; cur = cur.BaseType)
            foreach (var m in cur.GetMembers().OfType<IPropertySymbol>())
                if (!m.IsStatic && m.DeclaredAccessibility == Accessibility.Public)
                    yield return m;
    }
}

public static class CrudEmitter
{
    public static string Emit(CrudModel m)
    {
        var db = m.DbField;
        var set = $"{db}.{m.DbSet}";
        var proj = $"new {m.TOutFq}({m.ProjArgs})";

        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine("using Smoower.Minified.AspNetCore;");
        sb.AppendLine("using Smoower.Minified.EFCore;");
        if (m.ValField is not null) sb.AppendLine("using FluentValidation;");
        sb.AppendLine();
        if (m.Namespace is not null)
        {
            sb.AppendLine($"namespace {m.Namespace};");
            sb.AppendLine();
        }
        sb.AppendLine($"[API, RT(\"{m.Route}\")]");
        sb.AppendLine($"partial class {m.Name} : Ctl");
        sb.AppendLine("{");

        if (m.EmitGet)
        {
            sb.AppendLine("    [HG(\"{id}\")][P200, P404]");
            sb.AppendLine($"    public Tr Get({m.KeyTypeFq} id) => {set}.nt().w(x => x.{m.KeyName} == id).s(x => {proj}).ok1();");
        }
        if (m.EmitList)
        {
            sb.AppendLine("    [HG][P200]");
            sb.AppendLine($"    public Tr All() => {set}.nt().s(x => {proj}).okl();");
        }
        if (m.EmitCreate)
        {
            sb.AppendLine("    [HPO][P201, P400]");
            sb.AppendLine($"    public async Tr Post({m.TInFq} r)");
            sb.AppendLine("    {");
            if (m.ValField is not null)
                sb.AppendLine($"        var v = await {m.ValField}.ValidateAsync(r); if (!v.IsValid) return BadRequest(v.Errors);");
            sb.AppendLine($"        var x = await {db}.add(new {m.EntityFq} {{ {m.EntityInitArgs} }});");
            sb.AppendLine($"        return {proj}.created();");
            sb.AppendLine("    }");
        }
        if (m.EmitUpdate)
        {
            sb.AppendLine("    [HPU(\"{id}\")][P200, P404, P400]");
            sb.AppendLine($"    public async Tr Put({m.KeyTypeFq} id, {m.TInFq} r)");
            sb.AppendLine("    {");
            if (m.ValField is not null)
                sb.AppendLine($"        var v = await {m.ValField}.ValidateAsync(r); if (!v.IsValid) return BadRequest(v.Errors);");
            sb.AppendLine($"        var x = await {set}.id(id);");
            sb.AppendLine("        if (x is null) return NotFound();");
            sb.AppendLine($"        {m.PutAssigns}");
            sb.AppendLine($"        await {db}.save();");
            sb.AppendLine($"        return Ok({proj});");
            sb.AppendLine("    }");
        }
        if (m.EmitDelete)
        {
            sb.AppendLine("    [HD(\"{id}\")][P204, P404]");
            sb.AppendLine($"    public Tr Del({m.KeyTypeFq} id) => {db}.delById<{m.EntityFq}>(id);");
        }

        sb.AppendLine("}");
        return sb.ToString();
    }
}
