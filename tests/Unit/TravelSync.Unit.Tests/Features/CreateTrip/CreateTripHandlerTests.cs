using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TravelSync.Trip.API.Domain;
using TravelSync.Trip.API.Features.CreateTrip;
using TravelSync.Trip.API.Infrastructure.Persistence;
using Xunit;

namespace TravelSync.Unit.Tests.Features.CreateTrip;

public class CreateTripHandlerTests
{
    private static TripDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<TripDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TripDbContext(options);
    }

    [Fact]
    public async Task Handle_ShouldCreateTrip_AndReturnSuccessResult()
    {
        using var dbContext = CreateInMemoryDbContext();
        var handler = new CreateTripHandler(dbContext);
        var ownerId = Guid.NewGuid();

        var command = new CreateTripCommand(
            ownerId,
            "Europe Adventure",
            new DateOnly(2025, 6, 1),
            new DateOnly(2025, 6, 30),
            [new CreateTripDestinationDto("France", "Paris", 48.8566m, 2.3522m, 1)]);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Europe Adventure");
        result.Value.OwnerUserId.Should().Be(ownerId);
        result.Value.Destinations.Should().HaveCount(1);
        result.Value.Destinations.First().Country.Should().Be("France");

        var tripInDb = await dbContext.Trips.Include(t => t.Members).FirstOrDefaultAsync();
        tripInDb.Should().NotBeNull();
        tripInDb!.Members.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_ShouldAddOwnerAsMember_WhenTripIsCreated()
    {
        using var dbContext = CreateInMemoryDbContext();
        var handler = new CreateTripHandler(dbContext);
        var ownerId = Guid.NewGuid();

        var command = new CreateTripCommand(ownerId, "My Trip", new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 10), []);
        await handler.Handle(command, CancellationToken.None);

        var member = await dbContext.TripMembers.FirstOrDefaultAsync();
        member.Should().NotBeNull();
        member!.UserId.Should().Be(ownerId);
        member.Role.Should().Be(MemberRole.Owner);
        member.MembershipStatus.Should().Be(MembershipStatus.Accepted);
    }
}
