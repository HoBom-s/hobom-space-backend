using Ardalis.Specification;
using FluentAssertions;
using HobomSpace.Application.Ports;
using HobomSpace.Application.Services;
using HobomSpace.Domain.Entities;
using HobomSpace.Tests.Unit.Helpers;
using NSubstitute;

namespace HobomSpace.Tests.Unit.Application;

public class CommentServiceTests
{
    private readonly IRepository<Page> _pageRepo = Substitute.For<IRepository<Page>>();
    private readonly IRepository<Comment> _commentRepo = Substitute.For<IRepository<Comment>>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICommentService _sut;

    public CommentServiceTests() => _sut = new CommentService(_pageRepo, _commentRepo, _uow);

    // ── CreateAsync ──

    [Fact]
    public async Task CreateAsync_WithValidArgs_ReturnsComment()
    {
        var page = EntityTestHelper.CreatePageWithId(1);
        _pageRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<Page>>(), Arg.Any<CancellationToken>())
            .Returns(page);

        var result = await _sut.CreateAsync("DEV", 1, null, "Hello", "author", "actor");

        result.IsSuccess.Should().BeTrue();
        result.Value.Content.Should().Be("Hello");
        await _commentRepo.Received(1).AddAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_PageNotFound_ReturnsFailure()
    {
        _pageRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<Page>>(), Arg.Any<CancellationToken>())
            .Returns((Page?)null);

        var result = await _sut.CreateAsync("DEV", 999, null, "Hello", null, null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Page.NotFound");
    }

    [Fact]
    public async Task CreateAsync_ParentNotFound_ReturnsFailure()
    {
        var page = EntityTestHelper.CreatePageWithId(1);
        _pageRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<Page>>(), Arg.Any<CancellationToken>())
            .Returns(page);
        _commentRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<Comment>>(), Arg.Any<CancellationToken>())
            .Returns((Comment?)null);

        var result = await _sut.CreateAsync("DEV", 1, 999, "Hello", null, null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Comment.ParentNotFound");
    }

    [Fact]
    public async Task CreateAsync_ParentOnDifferentPage_ReturnsFailure()
    {
        var page = EntityTestHelper.CreatePageWithId(1);
        var parentComment = EntityTestHelper.CreateCommentWithId(50, pageId: 99);
        _pageRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<Page>>(), Arg.Any<CancellationToken>())
            .Returns(page);
        _commentRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<Comment>>(), Arg.Any<CancellationToken>())
            .Returns(parentComment);

        var result = await _sut.CreateAsync("DEV", 1, 50, "Hello", null, null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Comment.ParentOnDifferentPage");
    }

    // ── UpdateAsync ──

    [Fact]
    public async Task UpdateAsync_ExistingComment_Updates()
    {
        var comment = EntityTestHelper.CreateCommentWithId(1);
        _commentRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<Comment>>(), Arg.Any<CancellationToken>())
            .Returns(comment);

        var result = await _sut.UpdateAsync(1, "Updated");

        result.IsSuccess.Should().BeTrue();
        result.Value.Content.Should().Be("Updated");
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_NotFound_ReturnsFailure()
    {
        _commentRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<Comment>>(), Arg.Any<CancellationToken>())
            .Returns((Comment?)null);

        var result = await _sut.UpdateAsync(999, "Updated");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Comment.NotFound");
    }

    // ── DeleteAsync ──

    [Fact]
    public async Task DeleteAsync_ExistingComment_Succeeds()
    {
        var comment = EntityTestHelper.CreateCommentWithId(1);
        _commentRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<Comment>>(), Arg.Any<CancellationToken>())
            .Returns(comment);

        var result = await _sut.DeleteAsync(1);

        result.IsSuccess.Should().BeTrue();
        await _commentRepo.Received(1).DeleteAsync(comment, Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_NotFound_ReturnsFailure()
    {
        _commentRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<Comment>>(), Arg.Any<CancellationToken>())
            .Returns((Comment?)null);

        var result = await _sut.DeleteAsync(999);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Comment.NotFound");
    }

    // ── GetByPageIdAsync ──

    [Fact]
    public async Task GetByPageIdAsync_ReturnsPaginatedResult()
    {
        var comments = new List<Comment> { EntityTestHelper.CreateCommentWithId(1), EntityTestHelper.CreateCommentWithId(2) };
        _commentRepo.ListAsync(Arg.Any<ISpecification<Comment>>(), Arg.Any<CancellationToken>())
            .Returns(comments);
        _commentRepo.CountAsync(Arg.Any<ISpecification<Comment>>(), Arg.Any<CancellationToken>())
            .Returns(2);

        var result = await _sut.GetByPageIdAsync(1, 0, 10);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }
}
