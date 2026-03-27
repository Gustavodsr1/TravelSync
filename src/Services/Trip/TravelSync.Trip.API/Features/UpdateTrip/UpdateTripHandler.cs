using Microsoft.EntityFrameworkCore;
using TravelSync.SharedKernel.Abstractions;
using TravelSync.SharedKernel.Results;
using TravelSync.Trip.API.Domain;
using TravelSync.Trip.API.Infrastructure.Persistence;

namespace TravelSync.Trip.API.Features.UpdateTrip;

public sealed class UpdateTripHandler(TripDbContext dbContext)
    : ICommandHandler<UpdateTripCommand>
{
    public async Task<Result> Handle(UpdateTripCommand request, CancellationToken cancellationToken)
    {
        var trip = await dbContext.Trips
            .FirstOrDefaultAsync(t => t.Id == request.TripId, cancellationToken);

        if (trip is null)
            return Result.Failure(TripErrors.NotFound);

        if (!trip.IsOwner(request.RequestingUserId))
            return Result.Failure(TripErrors.NotOwner);

        if (trip.IsCancelled)
            return Result.Failure(TripErrors.AlreadyCancelled);

        trip.Update(request.Name, request.StartDate, request.EndDate);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
