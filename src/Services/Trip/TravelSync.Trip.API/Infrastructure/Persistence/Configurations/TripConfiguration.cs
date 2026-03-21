using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TravelSync.Trip.API.Infrastructure.Persistence.Configurations;

public class TripConfiguration : IEntityTypeConfiguration<Domain.Trip>
{
    public void Configure(EntityTypeBuilder<Domain.Trip> builder)
    {
        builder.ToTable("Trips");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.HasMany(t => t.Members)
            .WithOne()
            .HasForeignKey(m => m.TripId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Destinations)
            .WithOne()
            .HasForeignKey(d => d.TripId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
