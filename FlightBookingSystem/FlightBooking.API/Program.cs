using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FlightBooking.API.Data;
using FlightBooking.API.Models;
using FlightBooking.API.DTOs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Flight Booking API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() }
    });
});

var jwtKey = builder.Configuration["Jwt:Key"]!;
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = key
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddCors(o => o.AddPolicy("AllowClients", p => p.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin()));

var app = builder.Build();
app.UseCors("AllowClients");
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();

string CreateToken(User user)
{
    var claims = new[]
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.FullName),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Role, user.Role)
    };
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    var token = new JwtSecurityToken(builder.Configuration["Jwt:Issuer"], builder.Configuration["Jwt:Audience"], claims, expires: DateTime.UtcNow.AddHours(8), signingCredentials: creds);
    return new JwtSecurityTokenHandler().WriteToken(token);
}

int CurrentUserId(ClaimsPrincipal user) => int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

app.MapPost("/api/auth/register", async (RegisterRequest req, AppDbContext db) =>
{
    if (await db.Users.AnyAsync(x => x.Email == req.Email)) return Results.BadRequest(new ApiResponse(false, "Email already exists"));
    var user = new User { FullName = req.FullName, Email = req.Email, PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password), Role = "User" };
    db.Users.Add(user);
    await db.SaveChangesAsync();
    return Results.Ok(new ApiResponse(true, "Registration successful"));
}).WithTags("Authentication");

app.MapPost("/api/auth/login", async (LoginRequest req, AppDbContext db) =>
{
    var user = await db.Users.FirstOrDefaultAsync(x => x.Email == req.Email);
    if (user == null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash)) return Results.Unauthorized();
    return Results.Ok(new LoginResponse(CreateToken(user), user.Role, user.FullName));
}).WithTags("Authentication");

app.MapGet("/api/flights", async (string? origin, string? destination, DateTime? date, decimal? maxPrice, int? airlineId, AppDbContext db) =>
{
    var query = db.Flights.Include(f => f.Airline).Include(f => f.Route).ThenInclude(r => r.OriginAirport).Include(f => f.Route).ThenInclude(r => r.DestinationAirport).Include(f => f.Seats).AsQueryable();
    if (!string.IsNullOrWhiteSpace(origin)) query = query.Where(f => f.Route.OriginAirport.Code.Contains(origin) || f.Route.OriginAirport.City.Contains(origin));
    if (!string.IsNullOrWhiteSpace(destination)) query = query.Where(f => f.Route.DestinationAirport.Code.Contains(destination) || f.Route.DestinationAirport.City.Contains(destination));
    if (date.HasValue) query = query.Where(f => f.DepartureTime.Date == date.Value.Date);
    if (maxPrice.HasValue) query = query.Where(f => f.BasePrice <= maxPrice.Value);
    if (airlineId.HasValue) query = query.Where(f => f.AirlineId == airlineId.Value);
    var data = await query.Select(f => new FlightDto(f.Id, f.FlightNumber, f.Airline.Name, f.Route.OriginAirport.Code, f.Route.DestinationAirport.Code, f.DepartureTime, f.ArrivalTime, f.BasePrice, f.Seats.Count(s => !s.IsBooked), f.Status)).ToListAsync();
    return Results.Ok(data);
}).WithTags("Flights");

app.MapGet("/api/flights/{id:int}", async (int id, AppDbContext db) =>
{
    var f = await db.Flights.Include(x => x.Airline).Include(x => x.Route).ThenInclude(x => x.OriginAirport).Include(x => x.Route).ThenInclude(x => x.DestinationAirport).Include(x => x.Seats).FirstOrDefaultAsync(x => x.Id == id);
    if (f == null) return Results.NotFound(new ApiResponse(false, "Flight not found"));
    return Results.Ok(new { f.Id, f.FlightNumber, Airline = f.Airline.Name, Origin = f.Route.OriginAirport.Code, Destination = f.Route.DestinationAirport.Code, f.DepartureTime, f.ArrivalTime, f.BasePrice, Seats = f.Seats.Select(s => new { s.Id, s.SeatNumber, s.SeatClass, s.IsBooked }) });
}).WithTags("Flights");

app.MapPost("/api/flights", [Authorize(Roles="Admin")] async (CreateFlightRequest req, AppDbContext db) =>
{
    var flight = new Flight { FlightNumber = req.FlightNumber, AirlineId = req.AirlineId, RouteId = req.RouteId, DepartureTime = req.DepartureTime, ArrivalTime = req.ArrivalTime, BasePrice = req.BasePrice, Status = "Scheduled" };
    db.Flights.Add(flight);
    await db.SaveChangesAsync();
    for (int row = 1; row <= req.SeatRows; row++) foreach (var letter in new[] { "A", "B", "C", "D" }) db.Seats.Add(new Seat { FlightId = flight.Id, SeatNumber = $"{row}{letter}", SeatClass = "Economy" });
    await db.SaveChangesAsync();
    return Results.Ok(new ApiResponse(true, "Flight created"));
}).WithTags("Flights");

app.MapGet("/api/airports", async (AppDbContext db) => Results.Ok(await db.Airports.ToListAsync())).WithTags("Airports");
app.MapGet("/api/airlines", async (AppDbContext db) => Results.Ok(await db.Airlines.ToListAsync())).WithTags("Airlines");
app.MapGet("/api/routes", async (AppDbContext db) => Results.Ok(await db.Routes.Include(r => r.OriginAirport).Include(r => r.DestinationAirport).ToListAsync())).WithTags("Routes");

app.MapPost("/api/bookings", [Authorize] async (BookingRequest req, ClaimsPrincipal principal, AppDbContext db) =>
{
    using var tx = await db.Database.BeginTransactionAsync();
    var seat = await db.Seats.FirstOrDefaultAsync(s => s.Id == req.SeatId && s.FlightId == req.FlightId);
    if (seat == null) return Results.NotFound(new ApiResponse(false, "Seat not found"));
    if (seat.IsBooked || await db.Bookings.AnyAsync(b => b.FlightId == req.FlightId && b.SeatId == req.SeatId && b.Status != "Cancelled")) return Results.Conflict(new ApiResponse(false, "Seat is already booked"));
    seat.IsBooked = true;
    var booking = new Booking { BookingReference = "FB" + DateTime.UtcNow.ToString("yyyyMMddHHmmss") + Random.Shared.Next(100,999), UserId = CurrentUserId(principal), FlightId = req.FlightId, SeatId = req.SeatId, PassengerName = req.PassengerName, PassengerEmail = req.PassengerEmail, Status = "Confirmed" };
    db.Bookings.Add(booking);
    await db.SaveChangesAsync();
    db.Payments.Add(new Payment { BookingId = booking.Id, Amount = req.Amount, Method = req.PaymentMethod, Status = req.Amount > 0 ? "Paid" : "Pending", PaidAt = req.Amount > 0 ? DateTime.UtcNow : null });
    await db.SaveChangesAsync();
    await tx.CommitAsync();
    return Results.Ok(new ApiResponse(true, $"Booking successful. Reference: {booking.BookingReference}"));
}).WithTags("Bookings");

app.MapGet("/api/bookings/my", [Authorize] async (ClaimsPrincipal principal, AppDbContext db) =>
{
    int userId = CurrentUserId(principal);
    var data = await db.Bookings.Include(b => b.Flight).Include(b => b.Seat).Include(b => b.Payment).Where(b => b.UserId == userId).OrderByDescending(b => b.BookedAt).Select(b => new BookingDto(b.Id, b.BookingReference, b.Flight.FlightNumber, b.Seat.SeatNumber, b.PassengerName, b.Status, b.Payment != null ? b.Payment.Status : "Pending", b.BookedAt)).ToListAsync();
    return Results.Ok(data);
}).WithTags("Bookings");

app.MapPut("/api/bookings/{id:int}/cancel", [Authorize] async (int id, ClaimsPrincipal principal, AppDbContext db) =>
{
    int userId = CurrentUserId(principal);
    var booking = await db.Bookings.Include(b => b.Seat).FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);
    if (booking == null) return Results.NotFound(new ApiResponse(false, "Booking not found"));
    booking.Status = "Cancelled";
    booking.Seat.IsBooked = false;
    await db.SaveChangesAsync();
    return Results.Ok(new ApiResponse(true, "Booking cancelled"));
}).WithTags("Bookings");

app.MapGet("/api/admin/dashboard", [Authorize(Roles="Admin")] async (AppDbContext db) =>
{
    return Results.Ok(new {
        TotalFlights = await db.Flights.CountAsync(),
        TotalBookings = await db.Bookings.CountAsync(),
        CancelledBookings = await db.Bookings.CountAsync(b => b.Status == "Cancelled"),
        TotalSales = await db.Payments.Where(p => p.Status == "Paid").SumAsync(p => p.Amount),
        OccupiedSeats = await db.Seats.CountAsync(s => s.IsBooked),
        TotalSeats = await db.Seats.CountAsync()
    });
}).WithTags("Admin Reports");

app.Run();
