using FluentAssertions;
using HobomSpace.Domain.Entities;

namespace HobomSpace.Tests.Unit.Domain;

public class CommentTests
{
    [Fact]
    public void Create_WithValidArgs_ReturnsComment()
    {
        var comment = Comment.Create(1, null, "Hello", "author");

        comment.PageId.Should().Be(1);
        comment.ParentCommentId.Should().BeNull();
        comment.Content.Should().Be("Hello");
        comment.Author.Should().Be("author");
        comment.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        comment.UpdatedAt.Should().Be(comment.CreatedAt);
    }

    [Fact]
    public void Create_WithParentCommentId_SetsParent()
    {
        var comment = Comment.Create(1, 99, "Reply", null);

        comment.ParentCommentId.Should().Be(99);
    }

    [Fact]
    public void Create_WithNullAuthor_SetsNull()
    {
        var comment = Comment.Create(1, null, "content", null);

        comment.Author.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidContent_ThrowsArgumentException(string? content)
    {
        var act = () => Comment.Create(1, null, content!, "author");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Update_WithValidContent_UpdatesFields()
    {
        var comment = Comment.Create(1, null, "Old", "author");
        var createdAt = comment.CreatedAt;

        comment.Update("New content");

        comment.Content.Should().Be("New content");
        comment.UpdatedAt.Should().BeOnOrAfter(createdAt);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithInvalidContent_ThrowsArgumentException(string? content)
    {
        var comment = Comment.Create(1, null, "Valid", "author");

        var act = () => comment.Update(content!);

        act.Should().Throw<ArgumentException>();
    }
}
