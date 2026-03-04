using HobomSpace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HobomSpace.Infrastructure.Persistence.Configurations;

public sealed class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable("comments");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).UseIdentityAlwaysColumn();

        builder.Property(c => c.PageId).IsRequired();
        builder.HasIndex(c => c.PageId);

        builder.Property(c => c.ParentCommentId);

        builder.Property(c => c.Content).HasColumnType("text").IsRequired();
        builder.Property(c => c.Author).HasMaxLength(255);
        builder.Property(c => c.CreatedAt).IsRequired();
        builder.Property(c => c.UpdatedAt).IsRequired();

        builder.HasOne<Page>()
            .WithMany()
            .HasForeignKey(c => c.PageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Comment>()
            .WithMany()
            .HasForeignKey(c => c.ParentCommentId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);
    }
}
