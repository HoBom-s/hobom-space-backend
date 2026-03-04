using FluentAssertions;
using HobomSpace.Application.Ports;
using HobomSpace.Application.Services;
using HobomSpace.Domain.Entities;
using HobomSpace.Domain.Exceptions;
using HobomSpace.Tests.Unit.Helpers;
using NSubstitute;

namespace HobomSpace.Tests.Unit.Application;

public class SpaceServiceTests
{
    private readonly ISpaceRepository _repo = Substitute.For<ISpaceRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly SpaceService _sut;

    public SpaceServiceTests()
    {
        _sut = new SpaceService(_repo, _uow);
    }

    [Fact]
    public async Task CreateAsync_WithNewKey_CreatesAndReturnsSpace()
    {
        _repo.GetByKeyAsync("DEV").Returns((Space?)null);

        var result = await _sut.CreateAsync("dev", "Development", null);

        result.Key.Should().Be("DEV");
        await _repo.Received(1).AddAsync(result, Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_WithExistingKey_ThrowsConflictException()
    {
        var existing = EntityTestHelper.CreateSpaceWithId(1, "DEV", "Existing");
        _repo.GetByKeyAsync("dev").Returns(existing);

        var act = () => _sut.CreateAsync("dev", "Development", null);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsPaginatedResult()
    {
        var spaces = new List<Space> { EntityTestHelper.CreateSpaceWithId(1) };
        _repo.GetAllAsync(0, 20).Returns(spaces);
        _repo.CountAsync().Returns(1);

        var result = await _sut.GetAllAsync(0, 20);

        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetByKeyAsync_WithExistingKey_ReturnsSpace()
    {
        var space = EntityTestHelper.CreateSpaceWithId(1, "DEV");
        _repo.GetByKeyAsync("DEV").Returns(space);

        var result = await _sut.GetByKeyAsync("DEV");

        result.Should().Be(space);
    }

    [Fact]
    public async Task GetByKeyAsync_WithNonExistentKey_ThrowsNotFoundException()
    {
        _repo.GetByKeyAsync("NONE").Returns((Space?)null);

        var act = () => _sut.GetByKeyAsync("NONE");

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateAsync_WithExistingKey_UpdatesAndReturns()
    {
        var space = EntityTestHelper.CreateSpaceWithId(1, "DEV", "Old");
        _repo.GetByKeyAsync("DEV").Returns(space);

        var result = await _sut.UpdateAsync("DEV", "New Name", "New Desc");

        result.Name.Should().Be("New Name");
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistentKey_ThrowsNotFoundException()
    {
        _repo.GetByKeyAsync("NONE").Returns((Space?)null);

        var act = () => _sut.UpdateAsync("NONE", "Name", null);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_WithExistingKey_RemovesAndSaves()
    {
        var space = EntityTestHelper.CreateSpaceWithId(1, "DEV");
        _repo.GetByKeyAsync("DEV").Returns(space);

        await _sut.DeleteAsync("DEV");

        _repo.Received(1).Remove(space);
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentKey_ThrowsNotFoundException()
    {
        _repo.GetByKeyAsync("NONE").Returns((Space?)null);

        var act = () => _sut.DeleteAsync("NONE");

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
