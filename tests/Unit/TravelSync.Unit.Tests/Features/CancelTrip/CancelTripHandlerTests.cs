using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TravelSync.Trip.API.Domain;
using TravelSync.Trip.API.Features.CancelTrip;
using TravelSync.Trip.API.Infrastructure.Persistence;
using Xunit;

namespace TravelSync.Unit.Tests.Features.CancelTrip;

public class CancelTripHandlerTests
{
    private static TripDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<TripDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TripDbContext(options);
    }

    [Fact]
    public async Task Handle_ShouldCancelTrip_WhenOwnerRequests()
    {
        using var dbContext = CreateInMemoryDbContext();
        var ownerId = Guid.NewGuid();
        var trip = TravelSync.Trip.API.Domain.Trip.Create(ownerId, "My Trip", new DateOnly(2025, 6, 1), new DateOnly(2025, 6, 15));
        dbContext.Trips.Add(trip);
        await dbContext.SaveChangesAsync();

        var handler = new CancelTripHandler(dbContext);
        var result = await handler.Handle(new CancelTripCommand(trip.Id, ownerId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var updatedTrip = await dbContext.Trips.FirstAsync(t => t.Id == trip.Id);
        updatedTrip.Status.Should().Be(TripStatus.Cancelled);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenTripNotFound()
    {
        using var dbContext = CreateInMemoryDbContext();
        var handler = new CancelTripHandler(dbContext);

        var result = await handler.Handle(new CancelTripCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Trip.NotFound");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenNonOwnerRequests()
    {
        using var dbContext = CreateInMemoryDbContext();
        var ownerId = Guid.NewGuid();
        var nonOwnerId = Guid.NewGuid();
        var trip = TravelSync.Trip.API.Domain.Trip.Create(ownerId, "My Trip", new DateOnly(2025, 6, 1), new DateOnly(2025, 6, 15));
        dbContext.Trips.Add(trip);
        await dbContext.SaveChangesAsync();

        var handler = new CancelTripHandler(dbContext);
        var result = await handler.Handle(new CancelTripCommand(trip.Id, nonOwnerId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Trip.NotOwner");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenTripAlreadyCancelled()
    {
        using var dbContext = CreateInMemoryDbContext();
        var ownerId = Guid.NewGuid();
        var trip = TravelSync.Trip.API.Domain.Trip.Create(ownerId, "My Trip", new DateOnly(2025, 6, 1), new DateOnly(2025, 6, 15));
        trip.Cancel();
        dbContext.Trips.Add(trip);
        await dbContext.SaveChangesAsync();

        var handler = new CancelTripHandler(dbContext);
        var result = await handler.Handle(new CancelTripCommand(trip.Id, ownerId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Trip.AlreadyCancelled");
    }
}
