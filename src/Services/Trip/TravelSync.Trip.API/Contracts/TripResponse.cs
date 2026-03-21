namespace TravelSync.Trip.API.Contracts;

public sealed record TripResponse(
    Guid Id,
    Guid OwnerUserId,
    string Name,
    DateOnly StartDate,
    DateOnly EndDate,
    string Status,
    DateTime CreatedAt,
    IReadOnlyCollection<TripDestinationResponse> Destinations);

public sealed record TripDestinationResponse(
    Guid Id,
    string Country,
    string City,
    decimal? Latitude,
    decimal? Longitude,
    int VisitOrder);

public sealed record TripMemberResponse(
    Guid Id,
    Guid UserId,
    string Role,
    string MembershipStatus);
