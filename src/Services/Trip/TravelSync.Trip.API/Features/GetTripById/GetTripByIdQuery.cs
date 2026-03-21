using TravelSync.SharedKernel.Abstractions;
using TravelSync.Trip.API.Contracts;

namespace TravelSync.Trip.API.Features.GetTripById;

public sealed record GetTripByIdQuery(Guid TripId, Guid UserId) : IQuery<TripResponse>;
