using TravelSync.SharedKernel.Abstractions;
using TravelSync.SharedKernel.Results;
using TravelSync.Trip.API.Contracts;
using TravelSync.Trip.API.Infrastructure.Persistence;

namespace TravelSync.Trip.API.Features.CreateTrip;

public sealed class CreateTripHandler(TripDbContext dbContext)
    : ICommandHandler<CreateTripCommand, TripResponse>
{
    public async Task<Result<TripResponse>> Handle(CreateTripCommand request, CancellationToken cancellationToken)
    {
        var trip = Domain.Trip.Create(
            request.OwnerUserId,
            request.Name,
            request.StartDate,
            request.EndDate);

        int visitOrder = 1;
        foreach (var destination in request.Destinations)
        {
            trip.AddDestination(
                destination.Country,
                destination.City,
                destination.Latitude,
                destination.Longitude,
                destination.VisitOrder > 0 ? destination.VisitOrder : visitOrder++);
        }

        dbContext.Trips.Add(trip);
        await dbContext.SaveChangesAsync(cancellationToken);

        var response = MapToResponse(trip);
        return Result.Success(response);
    }

    private static TripResponse MapToResponse(Domain.Trip trip) =>
        new(
            trip.Id,
            trip.OwnerUserId,
            trip.Name,
            trip.StartDate,
            trip.EndDate,
            trip.Status.ToString(),
            trip.CreatedAt,
            trip.Destinations
                .OrderBy(d => d.VisitOrder)
                .Select(d => new TripDestinationResponse(
                    d.Id, d.Country, d.City, d.Latitude, d.Longitude, d.VisitOrder))
                .ToList()
                .AsReadOnly());
}
