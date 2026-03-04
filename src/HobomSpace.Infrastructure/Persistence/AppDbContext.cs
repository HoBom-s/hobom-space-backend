using HobomSpace.Application.Ports;
using HobomSpace.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HobomSpace.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<Space> Spaces => Set<Space>();
    public DbSet<Page> Pages => Set<Page>();
    public DbSet<PageVersion> PageVersions => Set<PageVersion>();
    public DbSet<Comment> Comments => Set<Comment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("space");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    async Task IUnitOfWork.SaveChangesAsync(CancellationToken ct)
        => await base.SaveChangesAsync(ct);
}
