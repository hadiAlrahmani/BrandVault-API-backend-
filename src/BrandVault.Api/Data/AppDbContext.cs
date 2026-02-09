namespace BrandVault.Api.Data;

using Microsoft.EntityFrameworkCore;
using BrandVault.Api.Common;
using BrandVault.Api.Models;

/// <summary>
/// The central EF Core class that manages database access.
///
/// Express/Prisma equivalent: This is your PrismaClient. Instead of:
///   const prisma = new PrismaClient();
///   const users = await prisma.user.findMany();
///
/// In .NET you inject AppDbContext and do:
///   var users = await _context.Users.ToListAsync();
///
/// Each DbSet&lt;T&gt; property represents a database table.
/// The generic parameter tells EF Core which C# class maps to which table.
/// </summary>
public class AppDbContext : DbContext
{
    // Constructor receives connection options via dependency injection.
    // In Express terms, this is like receiving the database URL in your module's constructor.
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Database tables — each DbSet<T> = one table
    // Usage: _context.Users.Where(u => u.Email == "...").FirstOrDefaultAsync()
    public DbSet<User> Users => Set<User>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Workspace> Workspaces => Set<Workspace>();
    public DbSet<WorkspaceAssignment> WorkspaceAssignments => Set<WorkspaceAssignment>();
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<AssetVersion> AssetVersions => Set<AssetVersion>();
    public DbSet<ReviewLink> ReviewLinks => Set<ReviewLink>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<ApprovalAction> ApprovalActions => Set<ApprovalAction>();

    /// <summary>
    /// Called once when EF Core builds its internal model of the database.
    /// This is where we apply all the entity configurations (table names,
    /// constraints, indexes, relationships) from the Configurations/ folder.
    ///
    /// ApplyConfigurationsFromAssembly auto-discovers all classes that implement
    /// IEntityTypeConfiguration&lt;T&gt; — no need to register each one manually.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    /// <summary>
    /// Override SaveChangesAsync to auto-set CreatedAt and Id on new entities.
    ///
    /// This is like a Prisma middleware or a Sequelize beforeCreate hook:
    /// every time we save, we check for newly added entities and stamp them.
    /// </summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;

                if (entry.Entity.Id == Guid.Empty)
                {
                    entry.Entity.Id = Guid.NewGuid();
                }
            }
        }

        // Normalize all DateTime properties to UTC — Npgsql requires Kind=Utc
        // for 'timestamp with time zone' columns.
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                foreach (var prop in entry.Properties)
                {
                    if (prop.CurrentValue is DateTime dt && dt.Kind == DateTimeKind.Unspecified)
                    {
                        prop.CurrentValue = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
                    }
                }
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
