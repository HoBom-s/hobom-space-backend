using Ardalis.Specification;
using FluentAssertions;
using HobomSpace.Application.Ports;
using HobomSpace.Application.Services;
using HobomSpace.Domain.Entities;
using HobomSpace.Tests.Unit.Helpers;
using NSubstitute;

namespace HobomSpace.Tests.Unit.Application;

public class VersionServiceTests
{
    private readonly IRepository<Page> _pageRepo = Substitute.For<IRepository<Page>>();
    private readonly IRepository<PageVersion> _versionRepo = Substitute.For<IRepository<PageVersion>>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly IVersionService _sut;

    public VersionServiceTests() => _sut = new VersionService(_pageRepo, _versionRepo, _uow);

    /// <summary>
    /// PageVersion.Create(pageId, prevVersion, ...) 는 Version = prevVersion + 1 로 저장한다.
    /// 따라서 resultVersion = prevVersion + 1.
    /// </summary>
    private static PageVersion CreateVersionWithId(long id, long pageId, int prevVersion, string title = "Title", string content = "Content")
    {
        var v = PageVersion.Create(pageId, prevVersion, title, content, null).Value;
        typeof(PageVersion).GetProperty(nameof(PageVersion.Id))!.SetValue(v, id);
        return v;
    }

    // ── GetHistoryAsync ──

    [Fact]
    public async Task GetHistoryAsync_ReturnsPaginatedResult()
    {
        var versions = new List<PageVersion> { CreateVersionWithId(1, 10, 0) };
        _versionRepo.ListAsync(Arg.Any<ISpecification<PageVersion>>(), Arg.Any<CancellationToken>())
            .Returns(versions);
        _versionRepo.CountAsync(Arg.Any<ISpecification<PageVersion>>(), Arg.Any<CancellationToken>())
            .Returns(1);

        var result = await _sut.GetHistoryAsync(10, 0, 10);

        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
    }

    // ── GetVersionAsync ──

    [Fact]
    public async Task GetVersionAsync_ExistingVersion_ReturnsVersion()
    {
        var version = CreateVersionWithId(1, 10, 0);
        _versionRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<PageVersion>>(), Arg.Any<CancellationToken>())
            .Returns(version);

        var result = await _sut.GetVersionAsync(10, 0);

        result.IsSuccess.Should().BeTrue();
        result.Value.Version.Should().Be(1);
    }

    [Fact]
    public async Task GetVersionAsync_NotFound_ReturnsFailure()
    {
        _versionRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<PageVersion>>(), Arg.Any<CancellationToken>())
            .Returns((PageVersion?)null);

        var result = await _sut.GetVersionAsync(10, 99);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PageVersion.NotFound");
    }

    // ── RestoreVersionAsync ──

    [Fact]
    public async Task RestoreVersionAsync_ValidVersion_RestoresPage()
    {
        var version = CreateVersionWithId(1, 10, 0, "Old Title", "Old Content");
        var page = EntityTestHelper.CreatePageWithId(10);

        _versionRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<PageVersion>>(), Arg.Any<CancellationToken>())
            .Returns(version);
        _pageRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<Page>>(), Arg.Any<CancellationToken>())
            .Returns(page);

        var result = await _sut.RestoreVersionAsync(10, 0);

        result.IsSuccess.Should().BeTrue();
        result.Value.Title.Should().Be("Old Title");
        result.Value.Content.Should().Be("Old Content");
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RestoreVersionAsync_VersionNotFound_ReturnsFailure()
    {
        _versionRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<PageVersion>>(), Arg.Any<CancellationToken>())
            .Returns((PageVersion?)null);

        var result = await _sut.RestoreVersionAsync(10, 99);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PageVersion.NotFound");
    }

    [Fact]
    public async Task RestoreVersionAsync_PageNotFound_ReturnsFailure()
    {
        var version = CreateVersionWithId(1, 10, 0);
        _versionRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<PageVersion>>(), Arg.Any<CancellationToken>())
            .Returns(version);
        _pageRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<Page>>(), Arg.Any<CancellationToken>())
            .Returns((Page?)null);

        var result = await _sut.RestoreVersionAsync(10, 0);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Page.NotFound");
    }

    // ── DiffVersionsAsync ──

    [Fact]
    public async Task DiffVersionsAsync_TwoVersions_ReturnsDiff()
    {
        var from = CreateVersionWithId(1, 10, 0, content: "line1\nline2");
        var to = CreateVersionWithId(2, 10, 1, content: "line1\nline3");

        var callCount = 0;
        _versionRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<PageVersion>>(), Arg.Any<CancellationToken>())
            .Returns(ci => callCount++ == 0 ? from : to);

        var result = await _sut.DiffVersionsAsync(10, 0, 1);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
    }

    [Fact]
    public async Task DiffVersionsAsync_FromVersionNotFound_ReturnsFailure()
    {
        _versionRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<PageVersion>>(), Arg.Any<CancellationToken>())
            .Returns((PageVersion?)null);

        var result = await _sut.DiffVersionsAsync(10, 0, 1);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PageVersion.NotFound");
    }
}
