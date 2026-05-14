# Flight Booking System

A complete OJT/capstone-style system integration project with:

- System 1: C# Console Application
- System 2: ASP.NET Core MVC Web Application
- Backend: ASP.NET Core Web API with Swagger
- Database: MySQL using XAMPP

Both client systems communicate only with the API. The API is the only system that connects to MySQL.

## Setup

1. Open XAMPP and start Apache and MySQL.
2. Open phpMyAdmin and import `Database/flight_booking.sql`.
3. Run the API:

```bash
cd FlightAPI
dotnet restore
dotnet run --urls http://localhost:5000
```

4. Open Swagger:

```text
http://localhost:5000/swagger
```

5. Run the Web App:

```bash
cd WebApp
dotnet restore
dotnet run --urls http://localhost:5001
```

Open:

```text
http://localhost:5001
```

6. Run the Console App:

```bash
cd ConsoleApp
dotnet restore
dotnet run
```

## Sample Accounts

Admin: admin@flight.com / password123
User: user@flight.com / password123

## Features

User: register, login, search flights, filter flights, seat selection, book flight, cancel booking, booking history, e-ticket, payment tracking.

Admin: dashboard, manage flights, airports, airlines, users, bookings, payments, reports.

System: real-time seat availability, duplicate booking prevention, API-based database access, synchronized data between Console App and Web App.

Security: JWT authentication, bcrypt password hashing, role-based access, protected admin endpoints, input validation.
