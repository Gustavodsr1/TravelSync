using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TravelSync.Trip.API.Features.GetTripById;
using TravelSync.Trip.API.Infrastructure.Persistence;
using Xunit;

namespace TravelSync.Unit.Tests.Features.GetTripById;

public class GetTripByIdHandlerTests
{
    private static TripDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<TripDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TripDbContext(options);
    }

    [Fact]
    public async Task Handle_ShouldReturnTrip_WhenOwnerRequests()
    {
        using var dbContext = CreateInMemoryDbContext();
        var ownerId = Guid.NewGuid();
        var trip = TravelSync.Trip.API.Domain.Trip.Create(ownerId, "My Trip", new DateOnly(2025, 6, 1), new DateOnly(2025, 6, 15));
        dbContext.Trips.Add(trip);
        await dbContext.SaveChangesAsync();

        var handler = new GetTripByIdHandler(dbContext);
        var result = await handler.Handle(new GetTripByIdQuery(trip.Id, ownerId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(trip.Id);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenTripNotFound()
    {
        using var dbContext = CreateInMemoryDbContext();
        var handler = new GetTripByIdHandler(dbContext);

        var result = await handler.Handle(new GetTripByIdQuery(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Trip.NotFound");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserHasNoAccess()
    {
        using var dbContext = CreateInMemoryDbContext();
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var trip = TravelSync.Trip.API.Domain.Trip.Create(ownerId, "My Trip", new DateOnly(2025, 6, 1), new DateOnly(2025, 6, 15));
        dbContext.Trips.Add(trip);
        await dbContext.SaveChangesAsync();

        var handler = new GetTripByIdHandler(dbContext);
        var result = await handler.Handle(new GetTripByIdQuery(trip.Id, otherUserId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Trip.AccessDenied");
    }
}
