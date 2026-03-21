using FluentAssertions;
using TravelSync.Trip.API.Features.CreateTrip;
using Xunit;

namespace TravelSync.Unit.Tests.Features.CreateTrip;

public class CreateTripValidatorTests
{
    private readonly CreateTripValidator _validator = new();

    [Fact]
    public async Task Validate_ShouldPass_WhenCommandIsValid()
    {
        var command = new CreateTripCommand(
            Guid.NewGuid(),
            "Europe Adventure",
            new DateOnly(2025, 6, 1),
            new DateOnly(2025, 6, 30),
            []);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenNameIsEmpty()
    {
        var command = new CreateTripCommand(
            Guid.NewGuid(),
            string.Empty,
            new DateOnly(2025, 6, 1),
            new DateOnly(2025, 6, 30),
            []);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Name");
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenEndDateIsBeforeStartDate()
    {
        var command = new CreateTripCommand(
            Guid.NewGuid(),
            "Trip Name",
            new DateOnly(2025, 6, 30),
            new DateOnly(2025, 6, 1),
            []);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "EndDate");
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenStartDateEqualsEndDate()
    {
        var command = new CreateTripCommand(
            Guid.NewGuid(),
            "Day Trip",
            new DateOnly(2025, 6, 1),
            new DateOnly(2025, 6, 1),
            []);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenNameExceeds200Characters()
    {
        var command = new CreateTripCommand(
            Guid.NewGuid(),
            new string('A', 201),
            new DateOnly(2025, 6, 1),
            new DateOnly(2025, 6, 30),
            []);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Name");
    }
}
