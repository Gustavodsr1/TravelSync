using TravelSync.SharedKernel.Primitives;

namespace TravelSync.Trip.API.Domain;

public class TripDestination : Entity
{
    public Guid TripId { get; private set; }
    public string Country { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public decimal? Latitude { get; private set; }
    public decimal? Longitude { get; private set; }
    public int VisitOrder { get; private set; }

    private TripDestination() { }

    internal static TripDestination Create(Guid tripId, string country, string city, decimal? latitude, decimal? longitude, int visitOrder) =>
        new()
        {
            TripId = tripId,
            Country = country,
            City = city,
            Latitude = latitude,
            Longitude = longitude,
            VisitOrder = visitOrder
        };
}
