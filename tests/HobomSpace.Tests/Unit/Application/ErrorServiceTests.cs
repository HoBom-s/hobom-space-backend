using Ardalis.Specification;
using FluentAssertions;
using HobomSpace.Application.Ports;
using HobomSpace.Application.Services;
using HobomSpace.Domain.Entities;
using HobomSpace.Tests.Unit.Helpers;
using NSubstitute;

namespace HobomSpace.Tests.Unit.Application;

public class ErrorServiceTests
{
    private readonly IRepository<ErrorEvent> _errorRepo = Substitute.For<IRepository<ErrorEvent>>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly IErrorService _sut;

    public ErrorServiceTests() => _sut = new ErrorService(_errorRepo, _uow);

    // ── CaptureAsync ──

    [Fact]
    public async Task CaptureAsync_WithValidArgs_ReturnsErrorEvent()
    {
        var result = await _sut.CaptureAsync("error msg", "stack", "/home", "CLIENT_LOGIC", "Chrome", "user1");

        result.IsSuccess.Should().BeTrue();
        result.Value.Message.Should().Be("error msg");
        result.Value.Screen.Should().Be("/home");
        await _errorRepo.Received(1).AddAsync(Arg.Any<ErrorEvent>(), Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    // ── GetAllAsync ──

    [Fact]
    public async Task GetAllAsync_ReturnsPaged()
    {
        var items = new List<ErrorEvent> { EntityTestHelper.CreateErrorEventWithId(1) };
        _errorRepo.ListAsync(Arg.Any<ISpecification<ErrorEvent>>(), Arg.Any<CancellationToken>())
            .Returns(items);
        _errorRepo.CountAsync(Arg.Any<ISpecification<ErrorEvent>>(), Arg.Any<CancellationToken>())
            .Returns(1);

        var result = await _sut.GetAllAsync(0, 10, null, null);

        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
    }

    // ── GetByIdAsync ──

    [Fact]
    public async Task GetByIdAsync_ExistingEvent_ReturnsEvent()
    {
        var errorEvent = EntityTestHelper.CreateErrorEventWithId(1);
        _errorRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<ErrorEvent>>(), Arg.Any<CancellationToken>())
            .Returns(errorEvent);

        var result = await _sut.GetByIdAsync(1);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ReturnsFailure()
    {
        _errorRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<ErrorEvent>>(), Arg.Any<CancellationToken>())
            .Returns((ErrorEvent?)null);

        var result = await _sut.GetByIdAsync(999);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("ErrorEvent.NotFound");
    }
}
