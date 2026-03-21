using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelSync.Trip.API.Domain;

namespace TravelSync.Trip.API.Infrastructure.Persistence.Configurations;

public class TripMemberConfiguration : IEntityTypeConfiguration<TripMember>
{
    public void Configure(EntityTypeBuilder<TripMember> builder)
    {
        builder.ToTable("TripMembers");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Role)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(m => m.MembershipStatus)
            .HasConversion<int>()
            .IsRequired();
    }
}
