using Ardalis.Specification;
using FluentAssertions;
using HobomSpace.Application.Ports;
using HobomSpace.Application.Services;
using HobomSpace.Domain.Entities;
using HobomSpace.Tests.Unit.Helpers;
using NSubstitute;

namespace HobomSpace.Tests.Unit.Application;

public class LabelServiceTests
{
    private readonly IRepository<Space> _spaceRepo = Substitute.For<IRepository<Space>>();
    private readonly IRepository<Page> _pageRepo = Substitute.For<IRepository<Page>>();
    private readonly IRepository<Label> _labelRepo = Substitute.For<IRepository<Label>>();
    private readonly IRepository<PageLabel> _pageLabelRepo = Substitute.For<IRepository<PageLabel>>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ILabelService _sut;

    public LabelServiceTests() => _sut = new LabelService(_spaceRepo, _pageRepo, _labelRepo, _pageLabelRepo, _uow);

    private Space SetupSpace(long id = 1, string key = "DEV")
    {
        var space = EntityTestHelper.CreateSpaceWithId(id, key);
        _spaceRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<Space>>(), Arg.Any<CancellationToken>())
            .Returns(space);
        return space;
    }

    // ── CreateAsync ──

    [Fact]
    public async Task CreateAsync_WithValidArgs_ReturnsLabel()
    {
        SetupSpace();

        var result = await _sut.CreateAsync("DEV", "Bug", "#FF0000");

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Bug");
        await _labelRepo.Received(1).AddAsync(Arg.Any<Label>(), Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_InvalidColor_ReturnsFailure()
    {
        SetupSpace();

        var result = await _sut.CreateAsync("DEV", "Bug", "invalid");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("HexColor");
    }

    [Fact]
    public async Task CreateAsync_SpaceNotFound_ReturnsFailure()
    {
        _spaceRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<Space>>(), Arg.Any<CancellationToken>())
            .Returns((Space?)null);

        var result = await _sut.CreateAsync("NOPE", "Bug", "#FF0000");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Space.NotFound");
    }

    // ── UpdateAsync ──

    [Fact]
    public async Task UpdateAsync_ExistingLabel_Updates()
    {
        var space = SetupSpace();
        var label = EntityTestHelper.CreateLabelWithId(1, space.Id);
        _labelRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<Label>>(), Arg.Any<CancellationToken>())
            .Returns(label);

        var result = await _sut.UpdateAsync("DEV", 1, "Updated", "#00FF00");

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Updated");
    }

    [Fact]
    public async Task UpdateAsync_LabelNotInSpace_ReturnsFailure()
    {
        SetupSpace(id: 1);
        var label = EntityTestHelper.CreateLabelWithId(1, spaceId: 99);
        _labelRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<Label>>(), Arg.Any<CancellationToken>())
            .Returns(label);

        var result = await _sut.UpdateAsync("DEV", 1, "Updated", "#00FF00");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Label.NotFound");
    }

    // ── DeleteAsync ──

    [Fact]
    public async Task DeleteAsync_ExistingLabel_Succeeds()
    {
        var space = SetupSpace();
        var label = EntityTestHelper.CreateLabelWithId(1, space.Id);
        _labelRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<Label>>(), Arg.Any<CancellationToken>())
            .Returns(label);

        var result = await _sut.DeleteAsync("DEV", 1);

        result.IsSuccess.Should().BeTrue();
        await _labelRepo.Received(1).DeleteAsync(label, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_LabelNotFound_ReturnsFailure()
    {
        SetupSpace();
        _labelRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<Label>>(), Arg.Any<CancellationToken>())
            .Returns((Label?)null);

        var result = await _sut.DeleteAsync("DEV", 999);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Label.NotFound");
    }

    // ── AddToPageAsync ──

    [Fact]
    public async Task AddToPageAsync_WithValidArgs_ReturnsPageLabel()
    {
        var space = SetupSpace();
        var page = EntityTestHelper.CreatePageWithId(10, space.Id);
        var label = EntityTestHelper.CreateLabelWithId(1, space.Id);
        _pageRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<Page>>(), Arg.Any<CancellationToken>())
            .Returns(page);
        _labelRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<Label>>(), Arg.Any<CancellationToken>())
            .Returns(label);
        _pageLabelRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<PageLabel>>(), Arg.Any<CancellationToken>())
            .Returns((PageLabel?)null);

        var result = await _sut.AddToPageAsync("DEV", 10, 1);

        result.IsSuccess.Should().BeTrue();
        await _pageLabelRepo.Received(1).AddAsync(Arg.Any<PageLabel>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddToPageAsync_AlreadyAssigned_ReturnsFailure()
    {
        var space = SetupSpace();
        var page = EntityTestHelper.CreatePageWithId(10, space.Id);
        var label = EntityTestHelper.CreateLabelWithId(1, space.Id);
        var existing = EntityTestHelper.CreatePageLabelWithId(1, 10, 1);
        _pageRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<Page>>(), Arg.Any<CancellationToken>())
            .Returns(page);
        _labelRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<Label>>(), Arg.Any<CancellationToken>())
            .Returns(label);
        _pageLabelRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<PageLabel>>(), Arg.Any<CancellationToken>())
            .Returns(existing);

        var result = await _sut.AddToPageAsync("DEV", 10, 1);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Label.AlreadyAssigned");
    }

    // ── RemoveFromPageAsync ──

    [Fact]
    public async Task RemoveFromPageAsync_Existing_Succeeds()
    {
        var space = SetupSpace();
        var page = EntityTestHelper.CreatePageWithId(10, space.Id);
        var pageLabel = EntityTestHelper.CreatePageLabelWithId(1, 10, 1);
        _pageRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<Page>>(), Arg.Any<CancellationToken>())
            .Returns(page);
        _pageLabelRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<PageLabel>>(), Arg.Any<CancellationToken>())
            .Returns(pageLabel);

        var result = await _sut.RemoveFromPageAsync("DEV", 10, 1);

        result.IsSuccess.Should().BeTrue();
        await _pageLabelRepo.Received(1).DeleteAsync(pageLabel, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RemoveFromPageAsync_NotAssigned_ReturnsFailure()
    {
        var space = SetupSpace();
        var page = EntityTestHelper.CreatePageWithId(10, space.Id);
        _pageRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<Page>>(), Arg.Any<CancellationToken>())
            .Returns(page);
        _pageLabelRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<PageLabel>>(), Arg.Any<CancellationToken>())
            .Returns((PageLabel?)null);

        var result = await _sut.RemoveFromPageAsync("DEV", 10, 1);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Label.NotAssigned");
    }

    // ── GetBySpaceKeyAsync ──

    [Fact]
    public async Task GetBySpaceKeyAsync_ReturnsLabels()
    {
        var space = SetupSpace();
        var labels = new List<Label> { EntityTestHelper.CreateLabelWithId(1, space.Id) };
        _labelRepo.ListAsync(Arg.Any<ISpecification<Label>>(), Arg.Any<CancellationToken>())
            .Returns(labels);

        var result = await _sut.GetBySpaceKeyAsync("DEV");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }

    // ── GetPagesByLabelAsync ──

    [Fact]
    public async Task GetPagesByLabelAsync_ReturnsPagesWithLabel()
    {
        var space = SetupSpace();
        var label = EntityTestHelper.CreateLabelWithId(1, space.Id);
        var pageLabel = EntityTestHelper.CreatePageLabelWithId(1, 10, 1);
        var page = EntityTestHelper.CreatePageWithId(10, space.Id);

        _labelRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<Label>>(), Arg.Any<CancellationToken>())
            .Returns(label);
        _pageLabelRepo.ListAsync(Arg.Any<ISpecification<PageLabel>>(), Arg.Any<CancellationToken>())
            .Returns(new List<PageLabel> { pageLabel });
        _pageRepo.ListAsync(Arg.Any<ISpecification<Page>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Page> { page });

        var result = await _sut.GetPagesByLabelAsync("DEV", 1);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }
}
