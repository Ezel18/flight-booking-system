using FlightAPI.Data; using FlightAPI.Models; using Microsoft.AspNetCore.Authorization; using Microsoft.AspNetCore.Mvc; using Microsoft.EntityFrameworkCore;
namespace FlightAPI.Controllers;
[ApiController, Route("api/admin"), Authorize(Roles="Admin")]
public class AdminController:ControllerBase{private readonly AppDbContext _db; public AdminController(AppDbContext db){_db=db;}
[HttpGet("dashboard")] public async Task<IActionResult> Dashboard(){var revenue=await _db.Payments.Where(p=>p.Status=="Paid").SumAsync(p=>p.Amount); return Ok(new{flights=await _db.Flights.CountAsync(),bookings=await _db.Bookings.CountAsync(),users=await _db.Users.CountAsync(),revenue});}
[HttpGet("airports")] public async Task<IActionResult> Airports()=>Ok(await _db.Airports.ToListAsync()); [HttpPost("airports")] public async Task<IActionResult> AddAirport(Airport a){_db.Airports.Add(a); await _db.SaveChangesAsync(); return Ok(a);} 
[HttpGet("airlines")] public async Task<IActionResult> Airlines()=>Ok(await _db.Airlines.ToListAsync()); [HttpPost("airlines")] public async Task<IActionResult> AddAirline(Airline a){_db.Airlines.Add(a); await _db.SaveChangesAsync(); return Ok(a);} 
[HttpGet("users")] public async Task<IActionResult> Users()=>Ok(await _db.Users.ToListAsync()); [HttpGet("payments")] public async Task<IActionResult> Payments()=>Ok(await _db.Payments.Include(p=>p.Booking).ToListAsync());}
