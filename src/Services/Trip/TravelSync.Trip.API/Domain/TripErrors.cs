using TravelSync.SharedKernel.Results;

namespace TravelSync.Trip.API.Domain;

public static class TripErrors
{
    public static readonly Error NotFound = new("Trip.NotFound", "Trip not found.");
    public static readonly Error NotOwner = new("Trip.NotOwner", "Only the trip owner can perform this action.");
    public static readonly Error AlreadyCancelled = new("Trip.AlreadyCancelled", "This trip has already been cancelled.");
    public static readonly Error AccessDenied = new("Trip.AccessDenied", "You do not have access to this trip.");
}
