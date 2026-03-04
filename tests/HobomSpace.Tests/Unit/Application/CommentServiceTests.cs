using FluentAssertions;
using HobomSpace.Application.Ports;
using HobomSpace.Application.Services;
using HobomSpace.Domain.Entities;
using HobomSpace.Domain.Exceptions;
using HobomSpace.Tests.Unit.Helpers;
using NSubstitute;

namespace HobomSpace.Tests.Unit.Application;

public class CommentServiceTests
{
    private readonly ICommentRepository _repo = Substitute.For<ICommentRepository>();
    private readonly IPageRepository _pageRepo = Substitute.For<IPageRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly CommentService _sut;

    public CommentServiceTests()
    {
        _sut = new CommentService(_repo, _pageRepo, _uow);
    }

    [Fact]
    public async Task CreateAsync_WithValidArgs_CreatesAndReturnsComment()
    {
        var page = EntityTestHelper.CreatePageWithId(1);
        _pageRepo.GetByIdAsync(1).Returns(page);

        var result = await _sut.CreateAsync(1, null, "Hello", "author");

        result.PageId.Should().Be(1);
        result.Content.Should().Be("Hello");
        await _repo.Received(1).AddAsync(result, Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_WithNonExistentPageId_ThrowsNotFoundException()
    {
        _pageRepo.GetByIdAsync(999).Returns((Page?)null);

        var act = () => _sut.CreateAsync(999, null, "Hello", "author");

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateAsync_WithParentCommentId_SetsParent()
    {
        var page = EntityTestHelper.CreatePageWithId(1);
        _pageRepo.GetByIdAsync(1).Returns(page);
        var parentComment = EntityTestHelper.CreateCommentWithId(99, pageId: 1);
        _repo.GetByIdAsync(99).Returns(parentComment);

        var result = await _sut.CreateAsync(1, 99, "Reply", null);

        result.ParentCommentId.Should().Be(99);
    }

    [Fact]
    public async Task CreateAsync_WithNonExistentParentCommentId_ThrowsNotFoundException()
    {
        var page = EntityTestHelper.CreatePageWithId(1);
        _pageRepo.GetByIdAsync(1).Returns(page);
        _repo.GetByIdAsync(999).Returns((Comment?)null);

        var act = () => _sut.CreateAsync(1, 999, "Reply", null);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateAsync_WithParentCommentOnDifferentPage_ThrowsArgumentException()
    {
        var page = EntityTestHelper.CreatePageWithId(1);
        _pageRepo.GetByIdAsync(1).Returns(page);
        var parentComment = EntityTestHelper.CreateCommentWithId(99, pageId: 2);
        _repo.GetByIdAsync(99).Returns(parentComment);

        var act = () => _sut.CreateAsync(1, 99, "Reply", null);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetByPageIdAsync_ReturnsPaginatedResult()
    {
        var comments = new List<Comment> { EntityTestHelper.CreateCommentWithId(1) };
        _repo.GetByPageIdAsync(1, 0, 20).Returns(comments);
        _repo.CountByPageIdAsync(1).Returns(1);

        var result = await _sut.GetByPageIdAsync(1, 0, 20);

        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task UpdateAsync_WithExistingComment_UpdatesAndReturns()
    {
        var comment = EntityTestHelper.CreateCommentWithId(1, content: "Old");
        _repo.GetByIdAsync(1).Returns(comment);

        var result = await _sut.UpdateAsync(1, "New content");

        result.Content.Should().Be("New content");
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistentComment_ThrowsNotFoundException()
    {
        _repo.GetByIdAsync(999).Returns((Comment?)null);

        var act = () => _sut.UpdateAsync(999, "content");

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_WithExistingComment_RemovesAndSaves()
    {
        var comment = EntityTestHelper.CreateCommentWithId(1);
        _repo.GetByIdAsync(1).Returns(comment);

        await _sut.DeleteAsync(1);

        _repo.Received(1).Remove(comment);
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentComment_ThrowsNotFoundException()
    {
        _repo.GetByIdAsync(999).Returns((Comment?)null);

        var act = () => _sut.DeleteAsync(999);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
