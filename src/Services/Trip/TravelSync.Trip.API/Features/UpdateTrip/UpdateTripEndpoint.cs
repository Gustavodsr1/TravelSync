using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace TravelSync.Trip.API.Features.UpdateTrip;

public class UpdateTripEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/trips/{tripId:guid}", async (
            Guid tripId,
            [FromBody] UpdateTripRequest request,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var userId = httpContext.GetUserId();
            if (userId is null)
                return Results.Unauthorized();

            var command = new UpdateTripCommand(tripId, userId.Value, request.Name, request.StartDate, request.EndDate);
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
        .WithName("UpdateTrip")
        .WithTags("Trips")
        .WithOpenApi();
    }
}

public sealed record UpdateTripRequest(string Name, DateOnly StartDate, DateOnly EndDate);
