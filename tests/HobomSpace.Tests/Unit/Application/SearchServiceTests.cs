using Ardalis.Specification;
using FluentAssertions;
using HobomSpace.Application.Ports;
using HobomSpace.Application.Services;
using HobomSpace.Domain.Entities;
using HobomSpace.Tests.Unit.Helpers;
using NSubstitute;

namespace HobomSpace.Tests.Unit.Application;

public class SearchServiceTests
{
    private readonly IReadRepository<Space> _spaceRepo = Substitute.For<IReadRepository<Space>>();
    private readonly IReadRepository<Page> _pageRepo = Substitute.For<IReadRepository<Page>>();
    private readonly ISearchService _sut;

    public SearchServiceTests() => _sut = new SearchService(_spaceRepo, _pageRepo);

    // ── SearchPagesAsync ──

    [Fact]
    public async Task SearchPagesAsync_ReturnsPaginatedResult()
    {
        var pages = new List<Page> { EntityTestHelper.CreatePageWithId(1) };
        _pageRepo.ListAsync(Arg.Any<ISpecification<Page>>(), Arg.Any<CancellationToken>())
            .Returns(pages);
        _pageRepo.CountAsync(Arg.Any<ISpecification<Page>>(), Arg.Any<CancellationToken>())
            .Returns(1);

        var result = await _sut.SearchPagesAsync("test", 0, 10);

        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
    }

    // ── SearchPagesInSpaceAsync ──

    [Fact]
    public async Task SearchPagesInSpaceAsync_ExistingSpace_ReturnsResult()
    {
        var space = EntityTestHelper.CreateSpaceWithId(1, "DEV");
        _spaceRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<Space>>(), Arg.Any<CancellationToken>())
            .Returns(space);
        _pageRepo.ListAsync(Arg.Any<ISpecification<Page>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Page>());
        _pageRepo.CountAsync(Arg.Any<ISpecification<Page>>(), Arg.Any<CancellationToken>())
            .Returns(0);

        var result = await _sut.SearchPagesInSpaceAsync("DEV", "query", 0, 10);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchPagesInSpaceAsync_SpaceNotFound_ReturnsFailure()
    {
        _spaceRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<Space>>(), Arg.Any<CancellationToken>())
            .Returns((Space?)null);

        var result = await _sut.SearchPagesInSpaceAsync("NOPE", "query", 0, 10);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Space.NotFound");
    }
}
