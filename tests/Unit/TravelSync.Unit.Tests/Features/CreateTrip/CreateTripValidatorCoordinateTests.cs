using FluentAssertions;
using TravelSync.Trip.API.Features.CreateTrip;
using Xunit;

namespace TravelSync.Unit.Tests.Features.CreateTrip;

public class CreateTripValidatorCoordinateTests
{
    private readonly CreateTripValidator _validator = new();

    [Theory]
    [InlineData(-90.0)]
    [InlineData(0.0)]
    [InlineData(90.0)]
    public async Task Validate_ShouldPass_WhenLatitudeIsInRange(double latitude)
    {
        var command = new CreateTripCommand(
            Guid.NewGuid(), "Trip", new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 5),
            [new CreateTripDestinationDto("France", "Paris", (decimal)latitude, 2.3m, 1)]);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(-90.1)]
    [InlineData(90.1)]
    public async Task Validate_ShouldFail_WhenLatitudeIsOutOfRange(double latitude)
    {
        var command = new CreateTripCommand(
            Guid.NewGuid(), "Trip", new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 5),
            [new CreateTripDestinationDto("France", "Paris", (decimal)latitude, 2.3m, 1)]);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Latitude"));
    }

    [Theory]
    [InlineData(-180.0)]
    [InlineData(0.0)]
    [InlineData(180.0)]
    public async Task Validate_ShouldPass_WhenLongitudeIsInRange(double longitude)
    {
        var command = new CreateTripCommand(
            Guid.NewGuid(), "Trip", new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 5),
            [new CreateTripDestinationDto("France", "Paris", 48.8m, (decimal)longitude, 1)]);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(-180.1)]
    [InlineData(180.1)]
    public async Task Validate_ShouldFail_WhenLongitudeIsOutOfRange(double longitude)
    {
        var command = new CreateTripCommand(
            Guid.NewGuid(), "Trip", new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 5),
            [new CreateTripDestinationDto("France", "Paris", 48.8m, (decimal)longitude, 1)]);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Longitude"));
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenCoordinatesAreNull()
    {
        var command = new CreateTripCommand(
            Guid.NewGuid(), "Trip", new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 5),
            [new CreateTripDestinationDto("France", "Paris", null, null, 1)]);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }
}
