using Microsoft.EntityFrameworkCore;
using TravelSync.SharedKernel.Abstractions;
using TravelSync.SharedKernel.Results;
using TravelSync.Trip.API.Contracts;
using TravelSync.Trip.API.Domain;
using TravelSync.Trip.API.Infrastructure.Persistence;

namespace TravelSync.Trip.API.Features.GetTripById;

public sealed class GetTripByIdHandler(TripDbContext dbContext)
    : IQueryHandler<GetTripByIdQuery, TripResponse>
{
    public async Task<Result<TripResponse>> Handle(GetTripByIdQuery request, CancellationToken cancellationToken)
    {
        var trip = await dbContext.Trips
            .Include(t => t.Members)
            .Include(t => t.Destinations)
            .FirstOrDefaultAsync(t => t.Id == request.TripId, cancellationToken);

        if (trip is null)
            return Result.Failure<TripResponse>(TripErrors.NotFound);

        if (!trip.CanAccess(request.UserId))
            return Result.Failure<TripResponse>(TripErrors.AccessDenied);

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
