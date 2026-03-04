using FluentAssertions;
using HobomSpace.Application.Ports;
using HobomSpace.Application.Services;
using HobomSpace.Domain.Entities;
using HobomSpace.Domain.Exceptions;
using HobomSpace.Tests.Unit.Helpers;
using NSubstitute;

namespace HobomSpace.Tests.Unit.Application;

public class PageVersionServiceTests
{
    private readonly IPageRepository _pageRepo = Substitute.For<IPageRepository>();
    private readonly IPageVersionRepository _versionRepo = Substitute.For<IPageVersionRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly PageVersionService _sut;

    public PageVersionServiceTests()
    {
        _sut = new PageVersionService(_pageRepo, _versionRepo, _uow);
    }

    [Fact]
    public async Task SaveVersionAsync_WithNoHistory_CreatesFirstVersion()
    {
        var page = EntityTestHelper.CreatePageWithId(1, title: "Title", content: "Content");
        _pageRepo.GetByIdAsync(1).Returns(page);
        _versionRepo.GetByPageIdAsync(1).Returns(new List<PageVersion>());

        var result = await _sut.SaveVersionAsync(1);

        result.Version.Should().Be(1);
        result.Title.Should().Be("Title");
        result.Content.Should().Be("Content");
        await _versionRepo.Received(1).AddAsync(result, Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SaveVersionAsync_WithExistingHistory_IncrementsVersion()
    {
        var page = EntityTestHelper.CreatePageWithId(1, title: "Title", content: "Content");
        _pageRepo.GetByIdAsync(1).Returns(page);
        var existing = PageVersion.Create(1, 0, "Old", "Old content", null);
        _versionRepo.GetByPageIdAsync(1).Returns(new List<PageVersion> { existing });

        var result = await _sut.SaveVersionAsync(1);

        result.Version.Should().Be(2);
    }

    [Fact]
    public async Task SaveVersionAsync_WithNonExistentPage_ThrowsNotFoundException()
    {
        _pageRepo.GetByIdAsync(99).Returns((Page?)null);

        var act = () => _sut.SaveVersionAsync(99);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetHistoryAsync_ReturnsPaginatedResult()
    {
        var v1 = PageVersion.Create(1, 0, "V1", "Content1", null);
        var v2 = PageVersion.Create(1, 1, "V2", "Content2", null);
        _versionRepo.GetByPageIdAsync(1, 0, 20).Returns(new List<PageVersion> { v1, v2 });
        _versionRepo.CountByPageIdAsync(1).Returns(2);

        var result = await _sut.GetHistoryAsync(1, 0, 20);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetVersionAsync_WithExistingVersion_ReturnsVersion()
    {
        var version = PageVersion.Create(1, 0, "Title", "Content", null);
        _versionRepo.GetByVersionAsync(1, 1).Returns(version);

        var result = await _sut.GetVersionAsync(1, 1);

        result.Should().Be(version);
    }

    [Fact]
    public async Task GetVersionAsync_WithNonExistentVersion_ThrowsNotFoundException()
    {
        _versionRepo.GetByVersionAsync(1, 99).Returns((PageVersion?)null);

        var act = () => _sut.GetVersionAsync(1, 99);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task RestoreVersionAsync_UpdatesPageFromVersion()
    {
        var version = PageVersion.Create(1, 0, "Old Title", "Old Content", null);
        _versionRepo.GetByVersionAsync(1, 1).Returns(version);
        var page = EntityTestHelper.CreatePageWithId(1, title: "Current", content: "Current");
        _pageRepo.GetByIdAsync(1).Returns(page);

        var result = await _sut.RestoreVersionAsync(1, 1);

        result.Title.Should().Be("Old Title");
        result.Content.Should().Be("Old Content");
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RestoreVersionAsync_WithNonExistentVersion_ThrowsNotFoundException()
    {
        _versionRepo.GetByVersionAsync(1, 99).Returns((PageVersion?)null);

        var act = () => _sut.RestoreVersionAsync(1, 99);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task RestoreVersionAsync_WithNonExistentPage_ThrowsNotFoundException()
    {
        var version = PageVersion.Create(1, 0, "Title", "Content", null);
        _versionRepo.GetByVersionAsync(1, 1).Returns(version);
        _pageRepo.GetByIdAsync(1).Returns((Page?)null);

        var act = () => _sut.RestoreVersionAsync(1, 1);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
