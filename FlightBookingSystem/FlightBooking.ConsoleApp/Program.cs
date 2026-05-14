using System.Net.Http.Headers;
using System.Net.Http.Json;

const string ApiBaseUrl = "http://localhost:5001";
using var http = new HttpClient { BaseAddress = new Uri(ApiBaseUrl) };
string? token = null;

while (true)
{
    Console.WriteLine("\n=== FLIGHT BOOKING CONSOLE APP ===");
    Console.WriteLine("1. Register");
    Console.WriteLine("2. Login");
    Console.WriteLine("3. Search Flights");
    Console.WriteLine("4. View Flight Details and Seats");
    Console.WriteLine("5. Book Flight");
    Console.WriteLine("6. My Bookings");
    Console.WriteLine("7. Cancel Booking");
    Console.WriteLine("0. Exit");
    Console.Write("Choose: ");
    var choice = Console.ReadLine();
    try
    {
        if (choice == "0") break;
        if (choice == "1") await Register();
        else if (choice == "2") await Login();
        else if (choice == "3") await SearchFlights();
        else if (choice == "4") await FlightDetails();
        else if (choice == "5") await BookFlight();
        else if (choice == "6") await MyBookings();
        else if (choice == "7") await CancelBooking();
    }
    catch (Exception ex) { Console.WriteLine("Error: " + ex.Message); }
}

async Task Register()
{
    Console.Write("Full name: "); var name = Console.ReadLine();
    Console.Write("Email: "); var email = Console.ReadLine();
    Console.Write("Password: "); var pass = Console.ReadLine();
    var res = await http.PostAsJsonAsync("/api/auth/register", new { FullName = name, Email = email, Password = pass });
    Console.WriteLine(await res.Content.ReadAsStringAsync());
}

async Task Login()
{
    Console.Write("Email: "); var email = Console.ReadLine();
    Console.Write("Password: "); var pass = Console.ReadLine();
    var res = await http.PostAsJsonAsync("/api/auth/login", new { Email = email, Password = pass });
    if (!res.IsSuccessStatusCode) { Console.WriteLine("Invalid login"); return; }
    var login = await res.Content.ReadFromJsonAsync<LoginResponse>();
    token = login!.Token;
    http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    Console.WriteLine($"Welcome {login.FullName}. Role: {login.Role}");
}

async Task SearchFlights()
{
    Console.Write("Origin code/city: "); var origin = Console.ReadLine();
    Console.Write("Destination code/city: "); var destination = Console.ReadLine();
    Console.Write("Date yyyy-mm-dd or blank: "); var date = Console.ReadLine();
    var url = $"/api/flights?origin={origin}&destination={destination}" + (string.IsNullOrWhiteSpace(date) ? "" : $"&date={date}");
    var flights = await http.GetFromJsonAsync<List<FlightDto>>(url);
    foreach (var f in flights ?? []) Console.WriteLine($"#{f.Id} {f.FlightNumber} {f.Airline} {f.Origin}->{f.Destination} {f.DepartureTime:g} Price:{f.BasePrice} Seats:{f.AvailableSeats}");
}

async Task FlightDetails()
{
    Console.Write("Flight ID: "); var id = Console.ReadLine();
    Console.WriteLine(await http.GetStringAsync($"/api/flights/{id}"));
}

async Task BookFlight()
{
    if (token == null) { Console.WriteLine("Please login first."); return; }
    Console.Write("Flight ID: "); var flightId = int.Parse(Console.ReadLine()!);
    Console.Write("Seat ID: "); var seatId = int.Parse(Console.ReadLine()!);
    Console.Write("Passenger name: "); var name = Console.ReadLine();
    Console.Write("Passenger email: "); var email = Console.ReadLine();
    Console.Write("Amount: "); var amount = decimal.Parse(Console.ReadLine()!);
    var res = await http.PostAsJsonAsync("/api/bookings", new { FlightId = flightId, SeatId = seatId, PassengerName = name, PassengerEmail = email, Amount = amount, PaymentMethod = "Cash" });
    Console.WriteLine(await res.Content.ReadAsStringAsync());
}

async Task MyBookings()
{
    if (token == null) { Console.WriteLine("Please login first."); return; }
    Console.WriteLine(await http.GetStringAsync("/api/bookings/my"));
}

async Task CancelBooking()
{
    if (token == null) { Console.WriteLine("Please login first."); return; }
    Console.Write("Booking ID: "); var id = Console.ReadLine();
    var res = await http.PutAsync($"/api/bookings/{id}/cancel", null);
    Console.WriteLine(await res.Content.ReadAsStringAsync());
}

record LoginResponse(string Token, string Role, string FullName);
record FlightDto(int Id, string FlightNumber, string Airline, string Origin, string Destination, DateTime DepartureTime, DateTime ArrivalTime, decimal BasePrice, int AvailableSeats, string Status);
