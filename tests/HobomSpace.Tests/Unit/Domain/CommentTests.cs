using FluentAssertions;
using HobomSpace.Domain.Common;
using HobomSpace.Domain.Entities;
using HobomSpace.Domain.ValueObjects;

namespace HobomSpace.Tests.Unit.Domain;

public class CommentTests
{
    private static Page ValidPage()
    {
        var key = SpaceKey.Create("TEST").Value;
        var space = Space.Create(key, "Test Space", null).Value;
        return Page.Create(space, null, "Title", "Content").Value;
    }

    [Fact]
    public void Create_WithValidArgs_ReturnsComment()
    {
        var page = ValidPage();

        var result = Comment.Create(page, null, "Hello", "author", "TEST", null);

        result.IsSuccess.Should().BeTrue();
        var comment = result.Value;
        comment.PageId.Should().Be(page.Id);
        comment.ParentCommentId.Should().BeNull();
        comment.Content.Should().Be("Hello");
        comment.Author.Should().Be("author");
        comment.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        comment.UpdatedAt.Should().Be(comment.CreatedAt);
    }

    [Fact]
    public void Create_WithParentCommentId_SetsParent()
    {
        var result = Comment.Create(ValidPage(), 99, "Reply", null, "TEST", null);

        result.IsSuccess.Should().BeTrue();
        result.Value.ParentCommentId.Should().Be(99);
    }

    [Fact]
    public void Create_WithNullAuthor_SetsNull()
    {
        var result = Comment.Create(ValidPage(), null, "content", null, "TEST", null);

        result.IsSuccess.Should().BeTrue();
        result.Value.Author.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidContent_ReturnsFailure(string? content)
    {
        var result = Comment.Create(ValidPage(), null, content!, "author", "TEST", null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Comment.ContentEmpty");
    }

    [Fact]
    public void Update_WithValidContent_UpdatesFields()
    {
        var comment = Comment.Create(ValidPage(), null, "Old", "author", "TEST", null).Value;
        var createdAt = comment.CreatedAt;

        var result = comment.Update("New content");

        result.IsSuccess.Should().BeTrue();
        comment.Content.Should().Be("New content");
        comment.UpdatedAt.Should().BeOnOrAfter(createdAt);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithInvalidContent_ReturnsFailure(string? content)
    {
        var comment = Comment.Create(ValidPage(), null, "Valid", "author", "TEST", null).Value;

        var result = comment.Update(content!);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Comment.ContentEmpty");
    }
}
