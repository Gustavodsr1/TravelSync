using Carter;
using MediatR;

namespace TravelSync.Trip.API.Features.GetTripMembers;

public class GetTripMembersEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/trips/{tripId:guid}/members", async (
            Guid tripId,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var userId = httpContext.GetUserId();
            if (userId is null)
                return Results.Unauthorized();

            var query = new GetTripMembersQuery(tripId, userId.Value);
            var result = await mediator.Send(query, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.Error.Code == "Trip.NotFound"
                    ? Results.NotFound(result.Error.Description)
                    : result.Error.Code == "Trip.AccessDenied"
                        ? Results.Forbid()
                        : Results.Problem(result.Error.Description, statusCode: StatusCodes.Status400BadRequest);
        })
        .RequireAuthorization()
        .WithName("GetTripMembers")
        .WithTags("Trips")
        .WithOpenApi();
    }
}
