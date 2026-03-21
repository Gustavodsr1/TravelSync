using Microsoft.EntityFrameworkCore;
using TravelSync.SharedKernel.Abstractions;
using TravelSync.SharedKernel.Results;
using TravelSync.Trip.API.Contracts;
using TravelSync.Trip.API.Infrastructure.Persistence;

namespace TravelSync.Trip.API.Features.GetTripById;

public sealed class GetTripByIdHandler(TripDbContext dbContext)
    : IQueryHandler<GetTripByIdQuery, TripResponse>
{
    private static readonly Error TripNotFound = new("Trip.NotFound", "Trip not found.");
    private static readonly Error TripAccessDenied = new("Trip.AccessDenied", "You do not have access to this trip.");

    public async Task<Result<TripResponse>> Handle(GetTripByIdQuery request, CancellationToken cancellationToken)
    {
        var trip = await dbContext.Trips
            .Include(t => t.Members)
            .Include(t => t.Destinations)
            .FirstOrDefaultAsync(t => t.Id == request.TripId, cancellationToken);

        if (trip is null)
            return Result.Failure<TripResponse>(TripNotFound);

        if (!trip.CanAccess(request.UserId))
            return Result.Failure<TripResponse>(TripAccessDenied);

        return Result.Success(new TripResponse(
            trip.Id,
            trip.OwnerUserId,
            trip.Name,
            trip.StartDate,
            trip.EndDate,
            trip.Status.ToString(),
            trip.CreatedAt,
            trip.Destinations
                .OrderBy(d => d.VisitOrder)
                .Select(d => new TripDestinationResponse(d.Id, d.Country, d.City, d.Latitude, d.Longitude, d.VisitOrder))
                .ToList()
                .AsReadOnly()));
    }
}
