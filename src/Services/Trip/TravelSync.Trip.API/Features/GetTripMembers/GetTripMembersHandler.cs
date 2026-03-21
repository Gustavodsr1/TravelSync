using Microsoft.EntityFrameworkCore;
using TravelSync.SharedKernel.Abstractions;
using TravelSync.SharedKernel.Results;
using TravelSync.Trip.API.Contracts;
using TravelSync.Trip.API.Infrastructure.Persistence;

namespace TravelSync.Trip.API.Features.GetTripMembers;

public sealed class GetTripMembersHandler(TripDbContext dbContext)
    : IQueryHandler<GetTripMembersQuery, IReadOnlyList<TripMemberResponse>>
{
    private static readonly Error TripNotFound = new("Trip.NotFound", "Trip not found.");
    private static readonly Error TripAccessDenied = new("Trip.AccessDenied", "You do not have access to this trip.");

    public async Task<Result<IReadOnlyList<TripMemberResponse>>> Handle(GetTripMembersQuery request, CancellationToken cancellationToken)
    {
        var trip = await dbContext.Trips
            .Include(t => t.Members)
            .FirstOrDefaultAsync(t => t.Id == request.TripId, cancellationToken);

        if (trip is null)
            return Result.Failure<IReadOnlyList<TripMemberResponse>>(TripNotFound);

        if (!trip.CanAccess(request.RequestingUserId))
            return Result.Failure<IReadOnlyList<TripMemberResponse>>(TripAccessDenied);

        var members = trip.Members
            .Select(m => new TripMemberResponse(m.Id, m.UserId, m.Role.ToString(), m.MembershipStatus.ToString()))
            .ToList()
            .AsReadOnly();

        return Result.Success<IReadOnlyList<TripMemberResponse>>(members);
    }
}
