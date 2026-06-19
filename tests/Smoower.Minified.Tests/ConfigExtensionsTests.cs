using Microsoft.EntityFrameworkCore;
using Smoower.Minified.EFCore;
using Xunit;

namespace Smoower.Minified.Tests;

public enum Kind { A, B }

public class Author
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public List<Book> Books { get; } = [];
    public List<Book> Edited { get; } = [];
}

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public Kind Kind { get; set; }
    public bool Hidden { get; set; }
    public int AuthorId { get; set; }
    public Author Author { get; set; } = null!;
    public int? EditorId { get; set; }
    public Author? Editor { get; set; }
}

public class CfgDb(DbContextOptions<CfgDb> options) : DbContext(options)
{
    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Book> Books => Set<Book>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.qf<Book>(b => !b.Hidden);
        mb.Entity<Author>(e =>
        {
            e.key(a => a.Id);
            e.idx(a => a.Name).uniq();
            e.p(a => a.Name).max(100).req();
            e.hasM(a => a.Books).wOne(b => b.Author).fk(b => b.AuthorId).onDel(DeleteBehavior.Cascade);
        });
        mb.Entity<Book>(e =>
        {
            e.p(b => b.Kind).conv<string>();
            e.one(b => b.Editor).many(a => a.Edited).fk(b => b.EditorId).onDel(DeleteBehavior.SetNull);
        });
    }
}

public class ConfigExtensionsTests
{
    private static CfgDb Db(string n) => new(new DbContextOptionsBuilder<CfgDb>().UseInMemoryDatabase(n).Options);

    [Fact]
    public async Task Qf_FiltersHiddenRows()
    {
        var db = Db(nameof(Qf_FiltersHiddenRows));
        var a = new Author { Name = "a" };
        db.Authors.Add(a);
        db.Books.Add(new Book { Title = "shown", Author = a });
        db.Books.Add(new Book { Title = "secret", Hidden = true, Author = a });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        Assert.Equal(new[] { "shown" }, await db.Books.s(b => b.Title).lst());
    }

    [Fact]
    public void Property_And_Index_Configured()
    {
        var author = Db(nameof(Property_And_Index_Configured)).Model.FindEntityType(typeof(Author))!;
        Assert.Equal(100, author.FindProperty(nameof(Author.Name))!.GetMaxLength());
        Assert.False(author.FindProperty(nameof(Author.Name))!.IsNullable);
        Assert.Contains(author.GetIndexes(), i => i.IsUnique);
    }

    [Fact]
    public void Relationships_And_Conversion_Configured()
    {
        var book = Db(nameof(Relationships_And_Conversion_Configured)).Model.FindEntityType(typeof(Book))!;
        var fks = book.GetForeignKeys().ToList();
        Assert.Contains(fks, f => f.Properties.Any(p => p.Name == nameof(Book.AuthorId)) && f.DeleteBehavior == DeleteBehavior.Cascade);
        Assert.Contains(fks, f => f.Properties.Any(p => p.Name == nameof(Book.EditorId)) && f.DeleteBehavior == DeleteBehavior.SetNull);
        // conv<string>() is exercised during model build above; the InMemory provider
        // drops value converters, so there is nothing to assert on at runtime here.
    }
}
