using Ardalis.Specification;
using FluentAssertions;
using HobomSpace.Application.Ports;
using HobomSpace.Application.Services;
using HobomSpace.Domain.Entities;
using HobomSpace.Tests.Unit.Helpers;
using NSubstitute;

namespace HobomSpace.Tests.Unit.Application;

public class SpaceServiceTests
{
    private readonly IRepository<Space> _spaceRepo = Substitute.For<IRepository<Space>>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ISpaceService _sut;

    public SpaceServiceTests() => _sut = new SpaceService(_spaceRepo, _uow);

    // ── CreateAsync ──

    [Fact]
    public async Task CreateAsync_WithValidKey_ReturnsSpace()
    {
        _spaceRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<Space>>(), Arg.Any<CancellationToken>())
            .Returns((Space?)null);

        var result = await _sut.CreateAsync("DEV", "Development", "desc");

        result.IsSuccess.Should().BeTrue();
        result.Value.Key.Should().Be("DEV");
        result.Value.Name.Should().Be("Development");
        await _spaceRepo.Received(1).AddAsync(Arg.Any<Space>(), Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_DuplicateKey_ReturnsFailure()
    {
        var existing = EntityTestHelper.CreateSpaceWithId(1, "DEV");
        _spaceRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<Space>>(), Arg.Any<CancellationToken>())
            .Returns(existing);

        var result = await _sut.CreateAsync("DEV", "Development", null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Space.AlreadyExists");
    }

    [Fact]
    public async Task CreateAsync_InvalidKey_ReturnsFailure()
    {
        var result = await _sut.CreateAsync("", "Name", null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Space.KeyEmpty");
    }

    [Fact]
    public async Task CreateAsync_InvalidName_ReturnsFailure()
    {
        _spaceRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<Space>>(), Arg.Any<CancellationToken>())
            .Returns((Space?)null);

        var result = await _sut.CreateAsync("DEV", "", null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Space.NameEmpty");
    }

    // ── UpdateAsync ──

    [Fact]
    public async Task UpdateAsync_ExistingSpace_UpdatesAndReturns()
    {
        var space = EntityTestHelper.CreateSpaceWithId(1, "DEV", "Old");
        _spaceRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<Space>>(), Arg.Any<CancellationToken>())
            .Returns(space);

        var result = await _sut.UpdateAsync("DEV", "New Name", "New Desc");

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("New Name");
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_NotFound_ReturnsFailure()
    {
        _spaceRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<Space>>(), Arg.Any<CancellationToken>())
            .Returns((Space?)null);

        var result = await _sut.UpdateAsync("NOPE", "Name", null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Space.NotFound");
    }

    [Fact]
    public async Task UpdateAsync_InvalidName_ReturnsFailure()
    {
        var space = EntityTestHelper.CreateSpaceWithId(1, "DEV");
        _spaceRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<Space>>(), Arg.Any<CancellationToken>())
            .Returns(space);

        var result = await _sut.UpdateAsync("DEV", "", null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Space.NameEmpty");
    }

    // ── DeleteAsync ──

    [Fact]
    public async Task DeleteAsync_ExistingSpace_Succeeds()
    {
        var space = EntityTestHelper.CreateSpaceWithId(1, "DEV");
        _spaceRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<Space>>(), Arg.Any<CancellationToken>())
            .Returns(space);

        var result = await _sut.DeleteAsync("DEV");

        result.IsSuccess.Should().BeTrue();
        await _spaceRepo.Received(1).DeleteAsync(space, Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_NotFound_ReturnsFailure()
    {
        _spaceRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<Space>>(), Arg.Any<CancellationToken>())
            .Returns((Space?)null);

        var result = await _sut.DeleteAsync("NOPE");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Space.NotFound");
    }

    // ── GetAllAsync ──

    [Fact]
    public async Task GetAllAsync_ReturnsPaginatedResult()
    {
        var spaces = new List<Space> { EntityTestHelper.CreateSpaceWithId(1) };
        _spaceRepo.ListAsync(Arg.Any<ISpecification<Space>>(), Arg.Any<CancellationToken>())
            .Returns(spaces);
        _spaceRepo.CountAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await _sut.GetAllAsync(0, 10);

        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
        result.Offset.Should().Be(0);
        result.Limit.Should().Be(10);
    }

    // ── GetByKeyAsync ──

    [Fact]
    public async Task GetByKeyAsync_ExistingSpace_ReturnsSpace()
    {
        var space = EntityTestHelper.CreateSpaceWithId(1, "DEV");
        _spaceRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<Space>>(), Arg.Any<CancellationToken>())
            .Returns(space);

        var result = await _sut.GetByKeyAsync("DEV");

        result.IsSuccess.Should().BeTrue();
        result.Value.Key.Should().Be("DEV");
    }

    [Fact]
    public async Task GetByKeyAsync_NotFound_ReturnsFailure()
    {
        _spaceRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<Space>>(), Arg.Any<CancellationToken>())
            .Returns((Space?)null);

        var result = await _sut.GetByKeyAsync("NOPE");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Space.NotFound");
    }
}
