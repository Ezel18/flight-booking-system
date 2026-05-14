namespace FlightBooking.API.DTOs;
public record ApiResponse(bool Success, string Message);
public record RegisterRequest(string FullName, string Email, string Password);
public record LoginRequest(string Email, string Password);
public record LoginResponse(string Token, string Role, string FullName);
public record FlightDto(int Id, string FlightNumber, string Airline, string Origin, string Destination, DateTime DepartureTime, DateTime ArrivalTime, decimal BasePrice, int AvailableSeats, string Status);
public record BookingRequest(int FlightId, int SeatId, string PassengerName, string PassengerEmail, decimal Amount, string PaymentMethod);
public record BookingDto(int Id, string BookingReference, string FlightNumber, string SeatNumber, string PassengerName, string Status, string PaymentStatus, DateTime BookedAt);
public record CreateFlightRequest(string FlightNumber, int AirlineId, int RouteId, DateTime DepartureTime, DateTime ArrivalTime, decimal BasePrice, int SeatRows);
