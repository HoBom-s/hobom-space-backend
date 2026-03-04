using FluentAssertions;
using HobomSpace.Application.Ports;
using HobomSpace.Application.Services;
using HobomSpace.Domain.Entities;
using HobomSpace.Domain.Exceptions;
using HobomSpace.Tests.Unit.Helpers;
using NSubstitute;

namespace HobomSpace.Tests.Unit.Application;

public class SearchServiceTests
{
    private readonly IPageRepository _pageRepo = Substitute.For<IPageRepository>();
    private readonly ISpaceRepository _spaceRepo = Substitute.For<ISpaceRepository>();
    private readonly SearchService _sut;

    public SearchServiceTests()
    {
        _sut = new SearchService(_pageRepo, _spaceRepo);
    }

    [Fact]
    public async Task SearchPagesAsync_WithValidQuery_ReturnsPaginatedResult()
    {
        var pages = new List<Page> { EntityTestHelper.CreatePageWithId(1, title: "Hello") };
        _pageRepo.SearchAsync("Hello", 0, 20).Returns(pages);
        _pageRepo.SearchCountAsync("Hello").Returns(1);

        var result = await _sut.SearchPagesAsync("Hello", 0, 20);

        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task SearchPagesAsync_TrimsQuery()
    {
        _pageRepo.SearchAsync("Hello", 0, 20).Returns(new List<Page>());
        _pageRepo.SearchCountAsync("Hello").Returns(0);

        await _sut.SearchPagesAsync("  Hello  ", 0, 20);

        await _pageRepo.Received(1).SearchAsync("Hello", 0, 20, Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SearchPagesAsync_WithInvalidQuery_ThrowsArgumentException(string? query)
    {
        var act = () => _sut.SearchPagesAsync(query!, 0, 20);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task SearchPagesInSpaceAsync_WithExistingSpace_ReturnsPaginatedResult()
    {
        var space = EntityTestHelper.CreateSpaceWithId(1, "DEV");
        _spaceRepo.GetByKeyAsync("DEV").Returns(space);
        var pages = new List<Page> { EntityTestHelper.CreatePageWithId(1) };
        _pageRepo.SearchBySpaceIdAsync(1, "test", 0, 20).Returns(pages);
        _pageRepo.SearchBySpaceIdCountAsync(1, "test").Returns(1);

        var result = await _sut.SearchPagesInSpaceAsync("DEV", "test", 0, 20);

        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task SearchPagesInSpaceAsync_WithNonExistentSpace_ThrowsNotFoundException()
    {
        _spaceRepo.GetByKeyAsync("NONE").Returns((Space?)null);

        var act = () => _sut.SearchPagesInSpaceAsync("NONE", "test", 0, 20);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SearchPagesInSpaceAsync_WithInvalidQuery_ThrowsArgumentException(string? query)
    {
        var act = () => _sut.SearchPagesInSpaceAsync("DEV", query!, 0, 20);

        await act.Should().ThrowAsync<ArgumentException>();
    }
}
