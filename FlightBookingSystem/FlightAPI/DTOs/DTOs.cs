namespace FlightAPI.DTOs;
public record RegisterDto(string FullName,string Email,string Password);
public record LoginDto(string Email,string Password);
public record FlightDto(string FlightNumber,int AirlineId,int RouteId,DateTime DepartureTime,DateTime ArrivalTime,decimal Price,string Status);
public record BookingDto(int UserId,int FlightId,int SeatId,string PassengerName,string PaymentMethod);
