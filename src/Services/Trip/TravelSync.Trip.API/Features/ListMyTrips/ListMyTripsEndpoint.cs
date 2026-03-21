using Carter;
using MediatR;

namespace TravelSync.Trip.API.Features.ListMyTrips;

public class ListMyTripsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/trips", async (
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var userId = httpContext.GetUserId();
            if (userId is null)
                return Results.Unauthorized();

            var query = new ListMyTripsQuery(userId.Value);
            var result = await mediator.Send(query, cancellationToken);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(result.Error.Description, statusCode: StatusCodes.Status400BadRequest);
        })
        .RequireAuthorization()
        .WithName("ListMyTrips")
        .WithTags("Trips")
        .WithOpenApi();
    }
}
