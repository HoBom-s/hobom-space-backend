using HobomSpace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HobomSpace.Infrastructure.Persistence.Configurations;

public sealed class PageConfiguration : IEntityTypeConfiguration<Page>
{
    public void Configure(EntityTypeBuilder<Page> builder)
    {
        builder.ToTable("pages");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).UseIdentityAlwaysColumn();

        builder.Property(p => p.SpaceId).IsRequired();
        builder.HasIndex(p => p.SpaceId);

        builder.Property(p => p.ParentPageId);
        builder.HasIndex(p => p.ParentPageId);

        builder.Property(p => p.Title).HasMaxLength(500).IsRequired();
        builder.Property(p => p.Content).HasColumnType("text").IsRequired();
        builder.Property(p => p.Position).HasDefaultValue(0);

        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.UpdatedAt).IsRequired();

        builder.HasOne<Space>()
            .WithMany()
            .HasForeignKey(p => p.SpaceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Page>()
            .WithMany()
            .HasForeignKey(p => p.ParentPageId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);
    }
}
