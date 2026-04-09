using Microsoft.EntityFrameworkCore;
using TravelSync.SharedKernel.Abstractions;
using TravelSync.SharedKernel.Results;
using TravelSync.Trip.API.Domain;
using TravelSync.Trip.API.Infrastructure.Persistence;

namespace TravelSync.Trip.API.Features.CancelTrip;

public sealed class CancelTripHandler(TripDbContext dbContext)
    : ICommandHandler<CancelTripCommand>
{
    public async Task<Result> Handle(CancelTripCommand request, CancellationToken cancellationToken)
    {
        var trip = await dbContext.Trips
            .FirstOrDefaultAsync(t => t.Id == request.TripId, cancellationToken);

        if (trip is null)
            return Result.Failure(TripErrors.NotFound);

        if (!trip.IsOwner(request.RequestingUserId))
            return Result.Failure(TripErrors.NotOwner);

        if (trip.IsCancelled)
            return Result.Failure(TripErrors.AlreadyCancelled);

        trip.Cancel();
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
