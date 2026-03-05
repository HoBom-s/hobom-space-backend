using FluentAssertions;
using HobomSpace.Application.Ports;
using HobomSpace.Application.Services;
using HobomSpace.Domain.Entities;
using HobomSpace.Domain.Exceptions;
using HobomSpace.Tests.Unit.Helpers;
using NSubstitute;

namespace HobomSpace.Tests.Unit.Application;

public class PageServiceTests
{
    private readonly ISpaceRepository _spaceRepo = Substitute.For<ISpaceRepository>();
    private readonly IPageRepository _pageRepo = Substitute.For<IPageRepository>();
    private readonly IPageVersionService _versionService = Substitute.For<IPageVersionService>();
    private readonly IOutboxRepository _outboxRepo = Substitute.For<IOutboxRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly PageService _sut;

    public PageServiceTests()
    {
        _sut = new PageService(_spaceRepo, _pageRepo, _versionService, _outboxRepo, _uow);
    }

    [Fact]
    public async Task CreateAsync_WithValidSpaceKey_CreatesPage()
    {
        var space = EntityTestHelper.CreateSpaceWithId(1, "DEV");
        _spaceRepo.GetByKeyAsync("DEV").Returns(space);

        var result = await _sut.CreateAsync("DEV", "Title", "Content", null, 0);

        result.SpaceId.Should().Be(1);
        result.Title.Should().Be("Title");
        await _pageRepo.Received(1).AddAsync(result, Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_WithNonExistentSpace_ThrowsNotFoundException()
    {
        _spaceRepo.GetByKeyAsync("NONE").Returns((Space?)null);

        var act = () => _sut.CreateAsync("NONE", "Title", "Content", null, 0);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetBySpaceKeyAsync_WithExistingSpace_ReturnsPages()
    {
        var space = EntityTestHelper.CreateSpaceWithId(1, "DEV");
        _spaceRepo.GetByKeyAsync("DEV").Returns(space);
        var pages = new List<Page> { EntityTestHelper.CreatePageWithId(1) };
        _pageRepo.GetBySpaceIdAsync(1).Returns(pages);

        var result = await _sut.GetBySpaceKeyAsync("DEV");

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetBySpaceKeyAsync_WithNonExistentSpace_ThrowsNotFoundException()
    {
        _spaceRepo.GetByKeyAsync("NONE").Returns((Space?)null);

        var act = () => _sut.GetBySpaceKeyAsync("NONE");

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingPage_ReturnsPage()
    {
        var space = EntityTestHelper.CreateSpaceWithId(1, "DEV");
        _spaceRepo.GetByKeyAsync("DEV").Returns(space);
        var page = EntityTestHelper.CreatePageWithId(1, spaceId: 1);
        _pageRepo.GetByIdAsync(1).Returns(page);

        var result = await _sut.GetByIdAsync("DEV", 1);

        result.Should().Be(page);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentPage_ThrowsNotFoundException()
    {
        var space = EntityTestHelper.CreateSpaceWithId(1, "DEV");
        _spaceRepo.GetByKeyAsync("DEV").Returns(space);
        _pageRepo.GetByIdAsync(99).Returns((Page?)null);

        var act = () => _sut.GetByIdAsync("DEV", 99);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetByIdAsync_PageBelongsToDifferentSpace_ThrowsNotFoundException()
    {
        var space = EntityTestHelper.CreateSpaceWithId(1, "DEV");
        _spaceRepo.GetByKeyAsync("DEV").Returns(space);
        var page = EntityTestHelper.CreatePageWithId(1, spaceId: 999);
        _pageRepo.GetByIdAsync(1).Returns(page);

        var act = () => _sut.GetByIdAsync("DEV", 1);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateAsync_WithExistingPage_UpdatesAndReturns()
    {
        var space = EntityTestHelper.CreateSpaceWithId(1, "DEV");
        _spaceRepo.GetByKeyAsync("DEV").Returns(space);
        var page = EntityTestHelper.CreatePageWithId(1, spaceId: 1);
        _pageRepo.GetByIdAsync(1).Returns(page);

        var result = await _sut.UpdateAsync("DEV", 1, "New Title", "New Content", 5);

        result.Title.Should().Be("New Title");
        result.Content.Should().Be("New Content");
        result.Position.Should().Be(5);
        await _versionService.Received(1).SaveVersionAsync(1, Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistentPage_ThrowsNotFoundException()
    {
        var space = EntityTestHelper.CreateSpaceWithId(1, "DEV");
        _spaceRepo.GetByKeyAsync("DEV").Returns(space);
        _pageRepo.GetByIdAsync(99).Returns((Page?)null);

        var act = () => _sut.UpdateAsync("DEV", 99, "Title", "Content", null);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateAsync_PageBelongsToDifferentSpace_ThrowsNotFoundException()
    {
        var space = EntityTestHelper.CreateSpaceWithId(1, "DEV");
        _spaceRepo.GetByKeyAsync("DEV").Returns(space);
        var page = EntityTestHelper.CreatePageWithId(1, spaceId: 999);
        _pageRepo.GetByIdAsync(1).Returns(page);

        var act = () => _sut.UpdateAsync("DEV", 1, "Title", "Content", null);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_WithExistingPage_RemovesAndSaves()
    {
        var space = EntityTestHelper.CreateSpaceWithId(1, "DEV");
        _spaceRepo.GetByKeyAsync("DEV").Returns(space);
        var page = EntityTestHelper.CreatePageWithId(1, spaceId: 1);
        _pageRepo.GetByIdAsync(1).Returns(page);

        await _sut.DeleteAsync("DEV", 1);

        _pageRepo.Received(1).Remove(page);
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentPage_ThrowsNotFoundException()
    {
        var space = EntityTestHelper.CreateSpaceWithId(1, "DEV");
        _spaceRepo.GetByKeyAsync("DEV").Returns(space);
        _pageRepo.GetByIdAsync(99).Returns((Page?)null);

        var act = () => _sut.DeleteAsync("DEV", 99);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
