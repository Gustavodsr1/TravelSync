using FluentAssertions;
using FluentValidation;
using TravelSync.Trip.API.Features.CreateTrip;
using TravelSync.Trip.API.Infrastructure.Behaviors;
using Xunit;

namespace TravelSync.Unit.Tests.Infrastructure;

public class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_ShouldReturnValidationFailure_WhenCommandIsInvalid()
    {
        var validators = new List<IValidator<CreateTripCommand>> { new CreateTripValidator() };
        var behavior = new ValidationBehavior<CreateTripCommand, TravelSync.SharedKernel.Results.Result<TravelSync.Trip.API.Contracts.TripResponse>>(validators);

        var invalidCommand = new CreateTripCommand(
            Guid.NewGuid(),
            string.Empty,
            new DateOnly(2025, 6, 30),
            new DateOnly(2025, 6, 1),
            []);

        var result = await behavior.Handle(invalidCommand, () => Task.FromResult(
            TravelSync.SharedKernel.Results.Result.Success(new TravelSync.Trip.API.Contracts.TripResponse(
                Guid.NewGuid(), Guid.NewGuid(), "x", DateOnly.MinValue, DateOnly.MaxValue, "Active", DateTime.UtcNow, []))),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Validation.Failed");
    }

    [Fact]
    public async Task Handle_ShouldCallNext_WhenCommandIsValid()
    {
        var validators = new List<IValidator<CreateTripCommand>> { new CreateTripValidator() };
        var behavior = new ValidationBehavior<CreateTripCommand, TravelSync.SharedKernel.Results.Result<TravelSync.Trip.API.Contracts.TripResponse>>(validators);

        var validCommand = new CreateTripCommand(
            Guid.NewGuid(),
            "Valid Trip",
            new DateOnly(2025, 6, 1),
            new DateOnly(2025, 6, 30),
            []);

        var nextWasCalled = false;
        var expectedResponse = TravelSync.SharedKernel.Results.Result.Success(new TravelSync.Trip.API.Contracts.TripResponse(
            Guid.NewGuid(), Guid.NewGuid(), "Valid Trip", new DateOnly(2025, 6, 1), new DateOnly(2025, 6, 30), "Active", DateTime.UtcNow, []));

        var result = await behavior.Handle(validCommand, () =>
        {
            nextWasCalled = true;
            return Task.FromResult(expectedResponse);
        }, CancellationToken.None);

        nextWasCalled.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldCallNext_WhenNoValidatorsRegistered()
    {
        var behavior = new ValidationBehavior<CreateTripCommand, TravelSync.SharedKernel.Results.Result<TravelSync.Trip.API.Contracts.TripResponse>>(
            Enumerable.Empty<IValidator<CreateTripCommand>>());

        var nextWasCalled = false;
        var command = new CreateTripCommand(Guid.NewGuid(), string.Empty, DateOnly.MinValue, DateOnly.MinValue, []);

        await behavior.Handle(command, () =>
        {
            nextWasCalled = true;
            return Task.FromResult(TravelSync.SharedKernel.Results.Result.Failure<TravelSync.Trip.API.Contracts.TripResponse>(
                new TravelSync.SharedKernel.Results.Error("Test.Error", "test")));
        }, CancellationToken.None);

        nextWasCalled.Should().BeTrue();
    }
}
