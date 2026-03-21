using Microsoft.EntityFrameworkCore;
using TravelSync.SharedKernel.Abstractions;
using TravelSync.SharedKernel.Results;
using TravelSync.Trip.API.Infrastructure.Persistence;

namespace TravelSync.Trip.API.Features.CancelTrip;

public sealed class CancelTripHandler(TripDbContext dbContext)
    : ICommandHandler<CancelTripCommand>
{
    private static readonly Error TripNotFound = new("Trip.NotFound", "Trip not found.");
    private static readonly Error NotOwner = new("Trip.NotOwner", "Only the trip owner can cancel this trip.");

    public async Task<Result> Handle(CancelTripCommand request, CancellationToken cancellationToken)
    {
        var trip = await dbContext.Trips
            .FirstOrDefaultAsync(t => t.Id == request.TripId, cancellationToken);

        if (trip is null)
            return Result.Failure(TripNotFound);

        if (!trip.IsOwner(request.RequestingUserId))
            return Result.Failure(NotOwner);

        trip.Cancel();
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
