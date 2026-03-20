using HobomSpace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HobomSpace.Infrastructure.Persistence.Configurations;

public sealed class PageLabelConfiguration : IEntityTypeConfiguration<PageLabel>
{
    public void Configure(EntityTypeBuilder<PageLabel> builder)
    {
        builder.ToTable("page_labels");

        builder.HasKey(pl => pl.Id);
        builder.Property(pl => pl.Id).UseIdentityAlwaysColumn();

        builder.Property(pl => pl.PageId).IsRequired();
        builder.Property(pl => pl.LabelId).IsRequired();
        builder.Property(pl => pl.CreatedAt).IsRequired();

        builder.HasIndex(pl => new { pl.PageId, pl.LabelId }).IsUnique();

        builder.HasOne<Page>()
            .WithMany()
            .HasForeignKey(pl => pl.PageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Label>()
            .WithMany()
            .HasForeignKey(pl => pl.LabelId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
