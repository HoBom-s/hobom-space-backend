using HobomSpace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HobomSpace.Infrastructure.Persistence.Configurations;

public sealed class ErrorEventConfiguration : IEntityTypeConfiguration<ErrorEvent>
{
    public void Configure(EntityTypeBuilder<ErrorEvent> builder)
    {
        builder.ToTable("error_events");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).UseIdentityAlwaysColumn();

        builder.Property(e => e.Message).HasMaxLength(2000).IsRequired();
        builder.Property(e => e.StackTrace).HasColumnType("text");
        builder.Property(e => e.Screen).HasMaxLength(500).IsRequired();
        builder.Property(e => e.ErrorType).HasMaxLength(50).IsRequired();
        builder.Property(e => e.UserAgent).HasMaxLength(500);
        builder.Property(e => e.Nickname).HasMaxLength(255);
        builder.Property(e => e.CreatedAt).IsRequired();

        builder.HasIndex(e => e.ErrorType);
        builder.HasIndex(e => e.Screen);
        builder.HasIndex(e => e.CreatedAt);
    }
}
