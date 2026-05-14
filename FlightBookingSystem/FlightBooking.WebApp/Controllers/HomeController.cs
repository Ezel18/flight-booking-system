using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace FlightBooking.WebApp.Controllers;

public class HomeController : Controller
{
    private readonly IHttpClientFactory _factory;
    public HomeController(IHttpClientFactory factory) => _factory = factory;
    HttpClient Api() { var c = _factory.CreateClient("api"); var t = HttpContext.Session.GetString("token"); if (!string.IsNullOrEmpty(t)) c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", t); return c; }

    public async Task<IActionResult> Index(string? origin, string? destination, string? date)
    {
        var flights = await Api().GetFromJsonAsync<List<FlightDto>>($"/api/flights?origin={origin}&destination={destination}&date={date}") ?? new();
        return View(flights);
    }

    public IActionResult Login() => View();

    [HttpPost]
    public async Task<IActionResult> Login(string email, string password)
    {
        var res = await Api().PostAsJsonAsync("/api/auth/login", new { Email = email, Password = password });
        if (!res.IsSuccessStatusCode) { ViewBag.Error = "Invalid login"; return View(); }
        var login = await res.Content.ReadFromJsonAsync<LoginResponse>();
        HttpContext.Session.SetString("token", login!.Token);
        HttpContext.Session.SetString("name", login.FullName);
        HttpContext.Session.SetString("role", login.Role);
        return RedirectToAction("Index");
    }

    public IActionResult Logout() { HttpContext.Session.Clear(); return RedirectToAction("Index"); }

    public async Task<IActionResult> Details(int id)
    {
        ViewBag.Json = await Api().GetStringAsync($"/api/flights/{id}");
        return View();
    }

    public IActionResult Book(int flightId) { ViewBag.FlightId = flightId; return View(); }

    [HttpPost]
    public async Task<IActionResult> Book(int flightId, int seatId, string passengerName, string passengerEmail, decimal amount)
    {
        var res = await Api().PostAsJsonAsync("/api/bookings", new { FlightId = flightId, SeatId = seatId, PassengerName = passengerName, PassengerEmail = passengerEmail, Amount = amount, PaymentMethod = "Cash" });
        ViewBag.Message = await res.Content.ReadAsStringAsync();
        return View();
    }

    public async Task<IActionResult> MyBookings()
    {
        var json = await Api().GetStringAsync("/api/bookings/my");
        ViewBag.Json = json;
        return View();
    }

    public async Task<IActionResult> AdminDashboard()
    {
        ViewBag.Json = await Api().GetStringAsync("/api/admin/dashboard");
        return View();
    }
}

public record LoginResponse(string Token, string Role, string FullName);
public record FlightDto(int Id, string FlightNumber, string Airline, string Origin, string Destination, DateTime DepartureTime, DateTime ArrivalTime, decimal BasePrice, int AvailableSeats, string Status);
