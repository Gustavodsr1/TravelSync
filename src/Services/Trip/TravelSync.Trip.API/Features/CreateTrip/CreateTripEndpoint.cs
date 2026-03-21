using Carter;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace TravelSync.Trip.API.Features.CreateTrip;

public class CreateTripEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/trips", async (
            [FromBody] CreateTripRequest request,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var userId = httpContext.GetUserId();
            if (userId is null)
                return Results.Unauthorized();

            var command = new CreateTripCommand(
                userId.Value,
                request.Name,
                request.StartDate,
                request.EndDate,
                request.Destinations?.Select(d => new CreateTripDestinationDto(
                    d.Country, d.City, d.Latitude, d.Longitude, d.VisitOrder)).ToList() ?? []);

            var result = await mediator.Send(command, cancellationToken);
            return result.IsSuccess
                ? Results.Created($"/trips/{result.Value.Id}", result.Value)
                : Results.Problem(result.Error.Description, statusCode: StatusCodes.Status400BadRequest);
        })
        .RequireAuthorization()
        .WithName("CreateTrip")
        .WithTags("Trips")
        .WithOpenApi();
    }
}

public sealed record CreateTripRequest(
    string Name,
    DateOnly StartDate,
    DateOnly EndDate,
    IReadOnlyList<CreateTripDestinationRequest>? Destinations);

public sealed record CreateTripDestinationRequest(
    string Country,
    string City,
    decimal? Latitude,
    decimal? Longitude,
    int VisitOrder);
