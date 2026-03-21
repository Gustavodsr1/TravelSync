using TravelSync.SharedKernel.Abstractions;

namespace TravelSync.Trip.API.Features.CancelTrip;

public sealed record CancelTripCommand(Guid TripId, Guid RequestingUserId) : ICommand;
