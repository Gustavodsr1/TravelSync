using TravelSync.SharedKernel.Abstractions;
using TravelSync.Trip.API.Contracts;

namespace TravelSync.Trip.API.Features.CreateTrip;

public sealed record CreateTripCommand(
    Guid OwnerUserId,
    string Name,
    DateOnly StartDate,
    DateOnly EndDate,
    IReadOnlyList<CreateTripDestinationDto> Destinations) : ICommand<TripResponse>;

public sealed record CreateTripDestinationDto(
    string Country,
    string City,
    decimal? Latitude,
    decimal? Longitude,
    int VisitOrder);
