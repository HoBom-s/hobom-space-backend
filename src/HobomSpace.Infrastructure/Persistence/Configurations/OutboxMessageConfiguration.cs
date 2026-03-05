using HobomSpace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HobomSpace.Infrastructure.Persistence.Configurations;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");

        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).UseIdentityAlwaysColumn();

        builder.Property(o => o.EventId).HasMaxLength(36).IsRequired();
        builder.Property(o => o.EventType).HasMaxLength(50).IsRequired();
        builder.Property(o => o.Payload).HasColumnType("text").IsRequired();
        builder.Property(o => o.Status).HasMaxLength(20).IsRequired();
        builder.Property(o => o.RetryCount).IsRequired();
        builder.Property(o => o.LastError).HasColumnType("text");
        builder.Property(o => o.Version).IsRequired();
        builder.Property(o => o.CreatedAt).IsRequired();
        builder.Property(o => o.UpdatedAt).IsRequired();

        builder.HasIndex(o => o.EventId).IsUnique();
        builder.HasIndex(o => new { o.EventType, o.Status });
    }
}
