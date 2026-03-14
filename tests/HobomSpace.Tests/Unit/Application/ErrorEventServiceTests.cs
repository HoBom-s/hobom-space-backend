using FluentAssertions;
using HobomSpace.Application.Ports;
using HobomSpace.Application.Services;
using HobomSpace.Domain.Entities;
using HobomSpace.Domain.Exceptions;
using HobomSpace.Tests.Unit.Helpers;
using NSubstitute;

namespace HobomSpace.Tests.Unit.Application;

public class ErrorEventServiceTests
{
    private readonly IErrorEventRepository _repo = Substitute.For<IErrorEventRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ErrorEventService _sut;

    public ErrorEventServiceTests()
    {
        _sut = new ErrorEventService(_repo, _uow);
    }

    [Fact]
    public async Task CaptureAsync_WithValidArgs_CreatesAndReturns()
    {
        var result = await _sut.CaptureAsync("error", "stack", "/home", "CLIENT_LOGIC", "Mozilla/5.0", "foxmon");

        result.Message.Should().Be("error");
        result.Screen.Should().Be("/home");
        result.ErrorType.Should().Be("CLIENT_LOGIC");
        await _repo.Received(1).AddAsync(result, Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CaptureAsync_WithInvalidErrorType_ThrowsArgumentException()
    {
        var act = () => _sut.CaptureAsync("error", null, "/home", "INVALID", null, null);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsPagedResult()
    {
        var items = new List<ErrorEvent> { EntityTestHelper.CreateErrorEventWithId(1) };
        _repo.GetAllAsync(0, 20, null, null, Arg.Any<CancellationToken>()).Returns(items);
        _repo.CountAsync(null, null, Arg.Any<CancellationToken>()).Returns(1);

        var result = await _sut.GetAllAsync(0, 20);

        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
        result.Page.Should().Be(0);
        result.Size.Should().Be(20);
    }

    [Fact]
    public async Task GetAllAsync_Page1_ComputesCorrectOffset()
    {
        _repo.GetAllAsync(20, 20, null, null, Arg.Any<CancellationToken>()).Returns([]);
        _repo.CountAsync(null, null, Arg.Any<CancellationToken>()).Returns(0);

        await _sut.GetAllAsync(1, 20);

        await _repo.Received(1).GetAllAsync(20, 20, null, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAllAsync_WithNegativePage_ThrowsArgumentException()
    {
        var act = () => _sut.GetAllAsync(-1, 20);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetAllAsync_WithZeroSize_ThrowsArgumentException()
    {
        var act = () => _sut.GetAllAsync(0, 0);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetAllAsync_WithSizeOver100_ClampsTo100()
    {
        _repo.GetAllAsync(0, 100, null, null, Arg.Any<CancellationToken>()).Returns([]);
        _repo.CountAsync(null, null, Arg.Any<CancellationToken>()).Returns(0);

        await _sut.GetAllAsync(0, 200);

        await _repo.Received(1).GetAllAsync(0, 100, null, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_Returns()
    {
        var errorEvent = EntityTestHelper.CreateErrorEventWithId(1);
        _repo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(errorEvent);

        var result = await _sut.GetByIdAsync(1);

        result.Should().Be(errorEvent);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentId_ThrowsNotFoundException()
    {
        _repo.GetByIdAsync(999, Arg.Any<CancellationToken>()).Returns((ErrorEvent?)null);

        var act = () => _sut.GetByIdAsync(999);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
