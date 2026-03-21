using Microsoft.EntityFrameworkCore;
using TravelSync.Trip.API.Domain;

namespace TravelSync.Trip.API.Infrastructure.Persistence;

public class TripDbContext(DbContextOptions<TripDbContext> options) : DbContext(options)
{
    public DbSet<Domain.Trip> Trips => Set<Domain.Trip>();
    public DbSet<TripMember> TripMembers => Set<TripMember>();
    public DbSet<TripDestination> TripDestinations => Set<TripDestination>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TripDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
