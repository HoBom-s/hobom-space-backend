using Ardalis.Specification;
using FluentAssertions;
using HobomSpace.Application.Ports;
using HobomSpace.Application.Services;
using HobomSpace.Domain.Entities;
using HobomSpace.Tests.Unit.Helpers;
using NSubstitute;

namespace HobomSpace.Tests.Unit.Application;

public class PageServiceTests
{
    private readonly IRepository<Space> _spaceRepo = Substitute.For<IRepository<Space>>();
    private readonly IRepository<Page> _pageRepo = Substitute.For<IRepository<Page>>();
    private readonly IRepository<PageVersion> _versionRepo = Substitute.For<IRepository<PageVersion>>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly IPageService _sut;

    public PageServiceTests() => _sut = new PageService(_spaceRepo, _pageRepo, _versionRepo, _uow);

    private Space SetupSpace(long id = 1, string key = "DEV")
    {
        var space = EntityTestHelper.CreateSpaceWithId(id, key);
        _spaceRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<Space>>(), Arg.Any<CancellationToken>())
            .Returns(space);
        return space;
    }

    // ── CreateAsync ──

    [Fact]
    public async Task CreateAsync_WithValidArgs_ReturnsPage()
    {
        SetupSpace();

        var result = await _sut.CreateAsync("DEV", "Title", "Content", null, 0, "actor");

        result.IsSuccess.Should().BeTrue();
        result.Value.Title.Should().Be("Title");
        await _pageRepo.Received(1).AddAsync(Arg.Any<Page>(), Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_SpaceNotFound_ReturnsFailure()
    {
        _spaceRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<Space>>(), Arg.Any<CancellationToken>())
            .Returns((Space?)null);

        var result = await _sut.CreateAsync("NOPE", "Title", "Content", null, 0, null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Space.NotFound");
    }

    [Fact]
    public async Task CreateAsync_EmptyTitle_ReturnsFailure()
    {
        SetupSpace();

        var result = await _sut.CreateAsync("DEV", "", "Content", null, 0, null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Page.TitleEmpty");
    }

    // ── UpdateAsync ──

    [Fact]
    public async Task UpdateAsync_SavesVersionSnapshotBeforeUpdate()
    {
        var space = SetupSpace();
        var page = EntityTestHelper.CreatePageWithId(10, space.Id, "Old Title", "Old Content");
        _pageRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<Page>>(), Arg.Any<CancellationToken>())
            .Returns(page);
        _versionRepo.ListAsync(Arg.Any<ISpecification<PageVersion>>(), Arg.Any<CancellationToken>())
            .Returns(new List<PageVersion>());

        var result = await _sut.UpdateAsync("DEV", 10, "New Title", "New Content", null, "actor");

        result.IsSuccess.Should().BeTrue();
        result.Value.Title.Should().Be("New Title");
        await _versionRepo.Received(1).AddAsync(Arg.Any<PageVersion>(), Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_SpaceNotFound_ReturnsFailure()
    {
        _spaceRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<Space>>(), Arg.Any<CancellationToken>())
            .Returns((Space?)null);

        var result = await _sut.UpdateAsync("NOPE", 1, "Title", "Content", null, null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Space.NotFound");
    }

    [Fact]
    public async Task UpdateAsync_PageNotFound_ReturnsFailure()
    {
        SetupSpace();
        _pageRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<Page>>(), Arg.Any<CancellationToken>())
            .Returns((Page?)null);

        var result = await _sut.UpdateAsync("DEV", 999, "Title", "Content", null, null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Page.NotFound");
    }

    [Fact]
    public async Task UpdateAsync_PageNotInSpace_ReturnsFailure()
    {
        SetupSpace(id: 1);
        var page = EntityTestHelper.CreatePageWithId(10, spaceId: 99);
        _pageRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<Page>>(), Arg.Any<CancellationToken>())
            .Returns(page);

        var result = await _sut.UpdateAsync("DEV", 10, "Title", "Content", null, null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Page.NotFound");
    }

    // ── DeleteAsync ──

    [Fact]
    public async Task DeleteAsync_ExistingPage_SoftDeletes()
    {
        var space = SetupSpace();
        var page = EntityTestHelper.CreatePageWithId(10, space.Id);
        _pageRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<Page>>(), Arg.Any<CancellationToken>())
            .Returns(page);

        var result = await _sut.DeleteAsync("DEV", 10, "actor");

        result.IsSuccess.Should().BeTrue();
        page.DeletedAt.Should().NotBeNull();
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_PageNotFound_ReturnsFailure()
    {
        SetupSpace();
        _pageRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<Page>>(), Arg.Any<CancellationToken>())
            .Returns((Page?)null);

        var result = await _sut.DeleteAsync("DEV", 999, null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Page.NotFound");
    }

    // ── MoveAsync ──

    [Fact]
    public async Task MoveAsync_ToAnotherSpace_Succeeds()
    {
        var source = EntityTestHelper.CreateSpaceWithId(1, "SRC");
        var target = EntityTestHelper.CreateSpaceWithId(2, "TGT");
        var page = EntityTestHelper.CreatePageWithId(10, source.Id);

        _spaceRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<Space>>(), Arg.Any<CancellationToken>())
            .Returns(ci =>
            {
                // first call = source space, second call = target space
                var spec = ci.Arg<ISpecification<Space>>();
                return spec switch
                {
                    _ when _spaceCallCount++ == 0 => source,
                    _ => target
                };
            });
        _pageRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<Page>>(), Arg.Any<CancellationToken>())
            .Returns(page);

        var result = await _sut.MoveAsync("SRC", 10, "TGT", null, "actor");

        result.IsSuccess.Should().BeTrue();
        result.Value.SpaceId.Should().Be(target.Id);
    }

    private int _spaceCallCount;

    [Fact]
    public async Task MoveAsync_SourceSpaceNotFound_ReturnsFailure()
    {
        _spaceRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<Space>>(), Arg.Any<CancellationToken>())
            .Returns((Space?)null);

        var result = await _sut.MoveAsync("NOPE", 10, "TGT", null, null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Space.NotFound");
    }

    // ── CopyAsync ──

    [Fact]
    public async Task CopyAsync_CreatesCopyWithPrefix()
    {
        var space = EntityTestHelper.CreateSpaceWithId(1, "DEV");
        var page = EntityTestHelper.CreatePageWithId(10, space.Id, "Original");

        _spaceRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<Space>>(), Arg.Any<CancellationToken>())
            .Returns(ci => space); // same space for simplicity
        _pageRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<Page>>(), Arg.Any<CancellationToken>())
            .Returns(page);

        var result = await _sut.CopyAsync("DEV", 10, "DEV", null, "actor");

        result.IsSuccess.Should().BeTrue();
        result.Value.Title.Should().Be("[Copy] Original");
        await _pageRepo.Received(1).AddAsync(Arg.Any<Page>(), Arg.Any<CancellationToken>());
    }

    // ── GetBySpaceKeyAsync ──

    [Fact]
    public async Task GetBySpaceKeyAsync_ReturnsPages()
    {
        SetupSpace();
        var pages = new List<Page> { EntityTestHelper.CreatePageWithId(1), EntityTestHelper.CreatePageWithId(2) };
        _pageRepo.ListAsync(Arg.Any<ISpecification<Page>>(), Arg.Any<CancellationToken>())
            .Returns(pages);

        var result = await _sut.GetBySpaceKeyAsync("DEV");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetBySpaceKeyAsync_SpaceNotFound_ReturnsFailure()
    {
        _spaceRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<Space>>(), Arg.Any<CancellationToken>())
            .Returns((Space?)null);

        var result = await _sut.GetBySpaceKeyAsync("NOPE");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Space.NotFound");
    }

    // ── GetByIdAsync ──

    [Fact]
    public async Task GetByIdAsync_ExistingPage_ReturnsPage()
    {
        var space = SetupSpace();
        var page = EntityTestHelper.CreatePageWithId(10, space.Id);
        _pageRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<Page>>(), Arg.Any<CancellationToken>())
            .Returns(page);

        var result = await _sut.GetByIdAsync("DEV", 10);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(10);
    }

    [Fact]
    public async Task GetByIdAsync_PageNotFound_ReturnsFailure()
    {
        SetupSpace();
        _pageRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<Page>>(), Arg.Any<CancellationToken>())
            .Returns((Page?)null);

        var result = await _sut.GetByIdAsync("DEV", 999);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Page.NotFound");
    }
}
