using TravelSync.SharedKernel.Abstractions;
using TravelSync.Trip.API.Contracts;

namespace TravelSync.Trip.API.Features.ListMyTrips;

public sealed record ListMyTripsQuery(Guid UserId) : IQuery<IReadOnlyList<TripResponse>>;
