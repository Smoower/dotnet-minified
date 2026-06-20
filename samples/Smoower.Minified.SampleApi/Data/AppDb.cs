using Microsoft.EntityFrameworkCore;

namespace Smoower.Minified.SampleApi;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
}

public class AppDb(DbContextOptions<AppDb> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Product> Products => Set<Product>();
}

// Trivial singleton, registered with the Hosting helper to demonstrate DI.
public class Clock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
