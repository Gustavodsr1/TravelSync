using Carter;
using MediatR;

namespace TravelSync.Trip.API.Features.CancelTrip;

public class CancelTripEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/trips/{tripId:guid}", async (
            Guid tripId,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var userId = httpContext.GetUserId();
            if (userId is null)
                return Results.Unauthorized();

            var command = new CancelTripCommand(tripId, userId.Value);
            var result = await mediator.Send(command, cancellationToken);

            return result.IsSuccess
                ? Results.NoContent()
                : result.Error.Code == "Trip.NotFound"
                    ? Results.NotFound(result.Error.Description)
                    : result.Error.Code == "Trip.NotOwner"
                        ? Results.Forbid()
                        : Results.Problem(result.Error.Description, statusCode: StatusCodes.Status400BadRequest);
        })
        .RequireAuthorization()
        .WithName("CancelTrip")
        .WithTags("Trips")
        .WithOpenApi();
    }
}
