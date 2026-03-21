using Microsoft.EntityFrameworkCore;
using TravelSync.SharedKernel.Abstractions;
using TravelSync.SharedKernel.Results;
using TravelSync.Trip.API.Contracts;
using TravelSync.Trip.API.Infrastructure.Persistence;

namespace TravelSync.Trip.API.Features.ListMyTrips;

public sealed class ListMyTripsHandler(TripDbContext dbContext)
    : IQueryHandler<ListMyTripsQuery, IReadOnlyList<TripResponse>>
{
    public async Task<Result<IReadOnlyList<TripResponse>>> Handle(ListMyTripsQuery request, CancellationToken cancellationToken)
    {
        var trips = await dbContext.Trips
            .Include(t => t.Members)
            .Include(t => t.Destinations)
            .Where(t => t.Members.Any(m =>
                m.UserId == request.UserId &&
                (m.MembershipStatus == Domain.MembershipStatus.Accepted)))
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

        var response = trips.Select(t => new TripResponse(
            t.Id,
            t.OwnerUserId,
            t.Name,
            t.StartDate,
            t.EndDate,
            t.Status.ToString(),
            t.CreatedAt,
            t.Destinations
                .OrderBy(d => d.VisitOrder)
                .Select(d => new TripDestinationResponse(d.Id, d.Country, d.City, d.Latitude, d.Longitude, d.VisitOrder))
                .ToList()
                .AsReadOnly()))
            .ToList()
            .AsReadOnly();

        return Result.Success<IReadOnlyList<TripResponse>>(response);
    }
}
