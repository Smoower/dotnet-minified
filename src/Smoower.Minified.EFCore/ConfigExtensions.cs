using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Smoower.Minified.EFCore;

// Compact EF Core model-configuration helpers for OnModelCreating. Same receiver
// types as the fluent API, so they chain identically. Short names collide with
// query/validation helpers only by spelling - the receiver type disambiguates.
public static class ConfigExtensions
{
    // Collapse modelBuilder.Entity<T>().HasQueryFilter(...) into one call.
    public static EntityTypeBuilder<T> qf<T>(this ModelBuilder mb, Expression<Func<T, bool>> filter) where T : class
        => mb.Entity<T>().HasQueryFilter(filter);

    public static EntityTypeBuilder<T> qf<T>(this EntityTypeBuilder<T> b, Expression<Func<T, bool>> filter) where T : class
        => b.HasQueryFilter(filter);

    public static KeyBuilder key<T>(this EntityTypeBuilder<T> b, Expression<Func<T, object?>> keys) where T : class
        => b.HasKey(keys);

    public static IndexBuilder<T> idx<T>(this EntityTypeBuilder<T> b, Expression<Func<T, object?>> keys) where T : class
        => b.HasIndex(keys);

    public static IndexBuilder<T> uniq<T>(this IndexBuilder<T> ix) => ix.IsUnique();

    public static PropertyBuilder<TP> p<T, TP>(this EntityTypeBuilder<T> b, Expression<Func<T, TP>> e) where T : class
        => b.Property(e);

    public static PropertyBuilder req(this PropertyBuilder p) => p.IsRequired();
    public static PropertyBuilder max(this PropertyBuilder p, int n) => p.HasMaxLength(n);
    public static PropertyBuilder conv<TProvider>(this PropertyBuilder p) => p.HasConversion<TProvider>();
    // Note: HasDefaultValue (def) / HasFilter are relational-only - they live in
    // the provider packages, not base EFCore, so they are not aliased here.

    // HasOne(...).WithMany(...)
    public static ReferenceNavigationBuilder<TE, TR> one<TE, TR>(this EntityTypeBuilder<TE> b, Expression<Func<TE, TR?>>? nav = null)
        where TE : class where TR : class => b.HasOne(nav);

    public static ReferenceCollectionBuilder<TR, TE> many<TE, TR>(this ReferenceNavigationBuilder<TE, TR> rb, Expression<Func<TR, IEnumerable<TE>?>>? nav = null)
        where TE : class where TR : class => rb.WithMany(nav);

    // HasMany(...).WithOne(...)
    public static CollectionNavigationBuilder<TE, TR> hasM<TE, TR>(this EntityTypeBuilder<TE> b, Expression<Func<TE, IEnumerable<TR>?>>? nav = null)
        where TE : class where TR : class => b.HasMany(nav);

    public static ReferenceCollectionBuilder<TE, TR> wOne<TE, TR>(this CollectionNavigationBuilder<TE, TR> cb, Expression<Func<TR, TE?>>? nav = null)
        where TE : class where TR : class => cb.WithOne(nav);

    // Shared tail of both relationship chains (lands on ReferenceCollectionBuilder).
    public static ReferenceCollectionBuilder<TP, TD> fk<TP, TD>(this ReferenceCollectionBuilder<TP, TD> rb, Expression<Func<TD, object?>> foreignKey)
        where TP : class where TD : class => rb.HasForeignKey(foreignKey);

    public static ReferenceCollectionBuilder<TP, TD> onDel<TP, TD>(this ReferenceCollectionBuilder<TP, TD> rb, DeleteBehavior behavior)
        where TP : class where TD : class => rb.OnDelete(behavior);

    // Shorthands for the delete behaviours that dominate a DbContext.
    public static ReferenceCollectionBuilder<TP, TD> cascade<TP, TD>(this ReferenceCollectionBuilder<TP, TD> rb)
        where TP : class where TD : class => rb.OnDelete(DeleteBehavior.Cascade);
    public static ReferenceCollectionBuilder<TP, TD> restrict<TP, TD>(this ReferenceCollectionBuilder<TP, TD> rb)
        where TP : class where TD : class => rb.OnDelete(DeleteBehavior.Restrict);
    public static ReferenceCollectionBuilder<TP, TD> setNull<TP, TD>(this ReferenceCollectionBuilder<TP, TD> rb)
        where TP : class where TD : class => rb.OnDelete(DeleteBehavior.SetNull);
    public static ReferenceCollectionBuilder<TP, TD> noAction<TP, TD>(this ReferenceCollectionBuilder<TP, TD> rb)
        where TP : class where TD : class => rb.OnDelete(DeleteBehavior.NoAction);
    public static ReferenceCollectionBuilder<TP, TD> clientSetNull<TP, TD>(this ReferenceCollectionBuilder<TP, TD> rb)
        where TP : class where TD : class => rb.OnDelete(DeleteBehavior.ClientSetNull);
    public static ReferenceCollectionBuilder<TP, TD> clientCascade<TP, TD>(this ReferenceCollectionBuilder<TP, TD> rb)
        where TP : class where TD : class => rb.OnDelete(DeleteBehavior.ClientCascade);
}
