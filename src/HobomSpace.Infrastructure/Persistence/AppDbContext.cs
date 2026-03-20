using HobomSpace.Application.Ports;
using HobomSpace.Domain.Common;
using HobomSpace.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HobomSpace.Infrastructure.Persistence;

/// <summary>EF Core DbContext. IUnitOfWork를 구현하며 snake_case 네이밍으로 PostgreSQL에 매핑된다.</summary>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<Space> Spaces => Set<Space>();
    public DbSet<Page> Pages => Set<Page>();
    public DbSet<PageVersion> PageVersions => Set<PageVersion>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Label> Labels => Set<Label>();
    public DbSet<PageLabel> PageLabels => Set<PageLabel>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<ErrorEvent> ErrorEvents => Set<ErrorEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("space");
        modelBuilder.Ignore<DomainEvent>();
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    async Task IUnitOfWork.SaveChangesAsync(CancellationToken ct)
        => await base.SaveChangesAsync(ct);
}
