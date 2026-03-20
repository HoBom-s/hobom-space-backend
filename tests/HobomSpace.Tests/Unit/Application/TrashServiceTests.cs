using Ardalis.Specification;
using FluentAssertions;
using HobomSpace.Application.Ports;
using HobomSpace.Application.Services;
using HobomSpace.Domain.Entities;
using HobomSpace.Tests.Unit.Helpers;
using NSubstitute;

namespace HobomSpace.Tests.Unit.Application;

public class TrashServiceTests
{
    private readonly IRepository<Space> _spaceRepo = Substitute.For<IRepository<Space>>();
    private readonly IRepository<Page> _pageRepo = Substitute.For<IRepository<Page>>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ITrashService _sut;

    public TrashServiceTests() => _sut = new TrashService(_spaceRepo, _pageRepo, _uow);

    private Space SetupSpace(long id = 1, string key = "DEV")
    {
        var space = EntityTestHelper.CreateSpaceWithId(id, key);
        _spaceRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<Space>>(), Arg.Any<CancellationToken>())
            .Returns(space);
        return space;
    }

    // ── GetDeletedPagesAsync ──

    [Fact]
    public async Task GetDeletedPagesAsync_ReturnsPaginatedResult()
    {
        var space = SetupSpace();
        var page = EntityTestHelper.CreatePageWithId(1, space.Id);
        page.SoftDelete("DEV", null);

        _pageRepo.ListAsync(Arg.Any<ISpecification<Page>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Page> { page });
        _pageRepo.CountAsync(Arg.Any<ISpecification<Page>>(), Arg.Any<CancellationToken>())
            .Returns(1);

        var result = await _sut.GetDeletedPagesAsync("DEV", 0, 10);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetDeletedPagesAsync_SpaceNotFound_ReturnsFailure()
    {
        _spaceRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<Space>>(), Arg.Any<CancellationToken>())
            .Returns((Space?)null);

        var result = await _sut.GetDeletedPagesAsync("NOPE", 0, 10);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Space.NotFound");
    }

    // ── RestoreAsync ──

    [Fact]
    public async Task RestoreAsync_DeletedPage_RestoresSuccessfully()
    {
        var space = SetupSpace();
        var page = EntityTestHelper.CreatePageWithId(10, space.Id);
        page.SoftDelete("DEV", null);

        _pageRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<Page>>(), Arg.Any<CancellationToken>())
            .Returns(page);

        var result = await _sut.RestoreAsync("DEV", 10);

        result.IsSuccess.Should().BeTrue();
        result.Value.DeletedAt.Should().BeNull();
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RestoreAsync_PageNotFound_ReturnsFailure()
    {
        SetupSpace();
        _pageRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<Page>>(), Arg.Any<CancellationToken>())
            .Returns((Page?)null);

        var result = await _sut.RestoreAsync("DEV", 999);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Page.NotFound");
    }

    [Fact]
    public async Task RestoreAsync_PageNotInSpace_ReturnsFailure()
    {
        SetupSpace(id: 1);
        var page = EntityTestHelper.CreatePageWithId(10, spaceId: 99);
        page.SoftDelete("OTHER", null);
        _pageRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<Page>>(), Arg.Any<CancellationToken>())
            .Returns(page);

        var result = await _sut.RestoreAsync("DEV", 10);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Page.NotFound");
    }

    // ── PermanentDeleteAsync ──

    [Fact]
    public async Task PermanentDeleteAsync_DeletedPage_Succeeds()
    {
        var space = SetupSpace();
        var page = EntityTestHelper.CreatePageWithId(10, space.Id);
        page.SoftDelete("DEV", null);
        _pageRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<Page>>(), Arg.Any<CancellationToken>())
            .Returns(page);

        var result = await _sut.PermanentDeleteAsync("DEV", 10);

        result.IsSuccess.Should().BeTrue();
        await _pageRepo.Received(1).DeleteAsync(page, Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PermanentDeleteAsync_PageNotFound_ReturnsFailure()
    {
        SetupSpace();
        _pageRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<Page>>(), Arg.Any<CancellationToken>())
            .Returns((Page?)null);

        var result = await _sut.PermanentDeleteAsync("DEV", 999);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Page.NotFound");
    }
}
