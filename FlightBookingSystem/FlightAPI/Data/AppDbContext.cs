using FlightAPI.Models;
using Microsoft.EntityFrameworkCore;
namespace FlightAPI.Data;
public class AppDbContext : DbContext {
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}
    public DbSet<User> Users => Set<User>(); public DbSet<Airport> Airports => Set<Airport>(); public DbSet<Airline> Airlines => Set<Airline>();
    public DbSet<FlightAPI.Models.Route> Routes => Set<FlightAPI.Models.Route>(); public DbSet<Flight> Flights => Set<Flight>(); public DbSet<Seat> Seats => Set<Seat>();
    public DbSet<Booking> Bookings => Set<Booking>(); public DbSet<Payment> Payments => Set<Payment>();
    protected override void OnModelCreating(ModelBuilder b) {
        b.Entity<Booking>().HasIndex(x => new { x.FlightId, x.SeatId }).IsUnique();
        b.Entity<User>().HasIndex(x => x.Email).IsUnique();
    }
}
