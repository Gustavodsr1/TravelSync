using TravelSync.SharedKernel.Abstractions;

namespace TravelSync.Trip.API.Features.UpdateTrip;

public sealed record UpdateTripCommand(
    Guid TripId,
    Guid RequestingUserId,
    string Name,
    DateOnly StartDate,
    DateOnly EndDate) : ICommand;
