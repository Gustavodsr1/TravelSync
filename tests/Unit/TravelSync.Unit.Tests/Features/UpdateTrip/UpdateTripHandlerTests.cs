using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TravelSync.Trip.API.Domain;
using TravelSync.Trip.API.Features.UpdateTrip;
using TravelSync.Trip.API.Infrastructure.Persistence;
using Xunit;

namespace TravelSync.Unit.Tests.Features.UpdateTrip;

public class UpdateTripHandlerTests
{
    private static TripDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<TripDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TripDbContext(options);
    }

    [Fact]
    public async Task Handle_ShouldUpdateTrip_WhenOwnerRequests()
    {
        using var dbContext = CreateInMemoryDbContext();
        var ownerId = Guid.NewGuid();
        var trip = TravelSync.Trip.API.Domain.Trip.Create(ownerId, "Old Name", new DateOnly(2025, 6, 1), new DateOnly(2025, 6, 15));
        dbContext.Trips.Add(trip);
        await dbContext.SaveChangesAsync();

        var handler = new UpdateTripHandler(dbContext);
        var command = new UpdateTripCommand(trip.Id, ownerId, "New Name", new DateOnly(2025, 7, 1), new DateOnly(2025, 7, 15));
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var updated = await dbContext.Trips.FirstAsync(t => t.Id == trip.Id);
        updated.Name.Should().Be("New Name");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenTripNotFound()
    {
        using var dbContext = CreateInMemoryDbContext();
        var handler = new UpdateTripHandler(dbContext);
        var command = new UpdateTripCommand(Guid.NewGuid(), Guid.NewGuid(), "Name", new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 5));

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Trip.NotFound");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenNonOwnerRequests()
    {
        using var dbContext = CreateInMemoryDbContext();
        var ownerId = Guid.NewGuid();
        var trip = TravelSync.Trip.API.Domain.Trip.Create(ownerId, "Trip", new DateOnly(2025, 6, 1), new DateOnly(2025, 6, 15));
        dbContext.Trips.Add(trip);
        await dbContext.SaveChangesAsync();

        var handler = new UpdateTripHandler(dbContext);
        var command = new UpdateTripCommand(trip.Id, Guid.NewGuid(), "New Name", new DateOnly(2025, 7, 1), new DateOnly(2025, 7, 15));
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Trip.NotOwner");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenTripIsCancelled()
    {
        using var dbContext = CreateInMemoryDbContext();
        var ownerId = Guid.NewGuid();
        var trip = TravelSync.Trip.API.Domain.Trip.Create(ownerId, "Trip", new DateOnly(2025, 6, 1), new DateOnly(2025, 6, 15));
        trip.Cancel();
        dbContext.Trips.Add(trip);
        await dbContext.SaveChangesAsync();

        var handler = new UpdateTripHandler(dbContext);
        var command = new UpdateTripCommand(trip.Id, ownerId, "New Name", new DateOnly(2025, 7, 1), new DateOnly(2025, 7, 15));
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Trip.AlreadyCancelled");
    }
}
