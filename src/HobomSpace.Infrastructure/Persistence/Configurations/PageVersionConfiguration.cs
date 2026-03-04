using HobomSpace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HobomSpace.Infrastructure.Persistence.Configurations;

public sealed class PageVersionConfiguration : IEntityTypeConfiguration<PageVersion>
{
    public void Configure(EntityTypeBuilder<PageVersion> builder)
    {
        builder.ToTable("page_versions");

        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).UseIdentityAlwaysColumn();

        builder.Property(v => v.PageId).IsRequired();
        builder.Property(v => v.Version).IsRequired();
        builder.Property(v => v.Title).HasMaxLength(500).IsRequired();
        builder.Property(v => v.Content).HasColumnType("text").IsRequired();
        builder.Property(v => v.EditedBy).HasMaxLength(255);
        builder.Property(v => v.CreatedAt).IsRequired();

        builder.HasIndex(v => new { v.PageId, v.Version }).IsUnique();

        builder.HasOne<Page>()
            .WithMany()
            .HasForeignKey(v => v.PageId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
