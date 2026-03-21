using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelSync.Trip.API.Domain;

namespace TravelSync.Trip.API.Infrastructure.Persistence.Configurations;

public class TripDestinationConfiguration : IEntityTypeConfiguration<TripDestination>
{
    public void Configure(EntityTypeBuilder<TripDestination> builder)
    {
        builder.ToTable("TripDestinations");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Country)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(d => d.City)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(d => d.Latitude)
            .HasPrecision(10, 7);

        builder.Property(d => d.Longitude)
            .HasPrecision(10, 7);
    }
}
