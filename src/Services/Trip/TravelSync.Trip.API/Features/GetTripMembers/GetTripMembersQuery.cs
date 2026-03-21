using TravelSync.SharedKernel.Abstractions;
using TravelSync.Trip.API.Contracts;

namespace TravelSync.Trip.API.Features.GetTripMembers;

public sealed record GetTripMembersQuery(Guid TripId, Guid RequestingUserId) : IQuery<IReadOnlyList<TripMemberResponse>>;
