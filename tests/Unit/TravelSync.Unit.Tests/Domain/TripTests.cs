using FluentAssertions;
using TravelSync.Trip.API.Domain;
using Xunit;

namespace TravelSync.Unit.Tests.Domain;

public class TripTests
{
    [Fact]
    public void Create_ShouldSetOwnerAndAddOwnerMember()
    {
        var ownerId = Guid.NewGuid();
        var trip = TravelSync.Trip.API.Domain.Trip.Create(ownerId, "Test Trip", new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 10));

        trip.OwnerUserId.Should().Be(ownerId);
        trip.Name.Should().Be("Test Trip");
        trip.Status.Should().Be(TripStatus.Active);
        trip.Members.Should().HaveCount(1);
        trip.Members.First().UserId.Should().Be(ownerId);
        trip.Members.First().Role.Should().Be(MemberRole.Owner);
    }

    [Fact]
    public void Cancel_ShouldChangeTripStatusToCancelled()
    {
        var trip = TravelSync.Trip.API.Domain.Trip.Create(Guid.NewGuid(), "Trip", new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 5));
        trip.Cancel();
        trip.Status.Should().Be(TripStatus.Cancelled);
    }

    [Fact]
    public void CanAccess_ShouldReturnTrue_ForOwner()
    {
        var ownerId = Guid.NewGuid();
        var trip = TravelSync.Trip.API.Domain.Trip.Create(ownerId, "Trip", new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 5));
        trip.CanAccess(ownerId).Should().BeTrue();
    }

    [Fact]
    public void CanAccess_ShouldReturnFalse_ForRandomUser()
    {
        var trip = TravelSync.Trip.API.Domain.Trip.Create(Guid.NewGuid(), "Trip", new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 5));
        trip.CanAccess(Guid.NewGuid()).Should().BeFalse();
    }

    [Fact]
    public void AddDestination_ShouldAddDestinationToList()
    {
        var trip = TravelSync.Trip.API.Domain.Trip.Create(Guid.NewGuid(), "Trip", new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 5));
        trip.AddDestination("France", "Paris", 48.8566m, 2.3522m, 1);
        trip.Destinations.Should().HaveCount(1);
        trip.Destinations.First().Country.Should().Be("France");
    }

    [Fact]
    public void Update_ShouldChangeNameAndDates()
    {
        var trip = TravelSync.Trip.API.Domain.Trip.Create(Guid.NewGuid(), "Old Name", new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 5));
        trip.Update("New Name", new DateOnly(2025, 2, 1), new DateOnly(2025, 2, 10));
        trip.Name.Should().Be("New Name");
        trip.StartDate.Should().Be(new DateOnly(2025, 2, 1));
        trip.EndDate.Should().Be(new DateOnly(2025, 2, 10));
    }
}
