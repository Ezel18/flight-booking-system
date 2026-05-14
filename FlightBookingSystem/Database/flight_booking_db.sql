CREATE DATABASE IF NOT EXISTS flight_booking_db;
USE flight_booking_db;

DROP TABLE IF EXISTS Payments;
DROP TABLE IF EXISTS Bookings;
DROP TABLE IF EXISTS Seats;
DROP TABLE IF EXISTS Flights;
DROP TABLE IF EXISTS Routes;
DROP TABLE IF EXISTS Airports;
DROP TABLE IF EXISTS Airlines;
DROP TABLE IF EXISTS Users;

CREATE TABLE Users (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    FullName VARCHAR(120) NOT NULL,
    Email VARCHAR(120) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    Role VARCHAR(20) NOT NULL DEFAULT 'User',
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE Airlines (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(120) NOT NULL,
    Code VARCHAR(10) NOT NULL UNIQUE
);

CREATE TABLE Airports (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Code VARCHAR(10) NOT NULL UNIQUE,
    Name VARCHAR(150) NOT NULL,
    City VARCHAR(100) NOT NULL,
    Country VARCHAR(100) NOT NULL
);

CREATE TABLE Routes (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    OriginAirportId INT NOT NULL,
    DestinationAirportId INT NOT NULL,
    FOREIGN KEY (OriginAirportId) REFERENCES Airports(Id),
    FOREIGN KEY (DestinationAirportId) REFERENCES Airports(Id)
);

CREATE TABLE Flights (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    FlightNumber VARCHAR(30) NOT NULL UNIQUE,
    AirlineId INT NOT NULL,
    RouteId INT NOT NULL,
    DepartureTime DATETIME NOT NULL,
    ArrivalTime DATETIME NOT NULL,
    BasePrice DECIMAL(10,2) NOT NULL,
    Status VARCHAR(30) NOT NULL DEFAULT 'Scheduled',
    FOREIGN KEY (AirlineId) REFERENCES Airlines(Id),
    FOREIGN KEY (RouteId) REFERENCES Routes(Id)
);

CREATE TABLE Seats (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    FlightId INT NOT NULL,
    SeatNumber VARCHAR(10) NOT NULL,
    SeatClass VARCHAR(30) NOT NULL DEFAULT 'Economy',
    IsBooked BOOLEAN NOT NULL DEFAULT FALSE,
    UNIQUE (FlightId, SeatNumber),
    FOREIGN KEY (FlightId) REFERENCES Flights(Id) ON DELETE CASCADE
);

CREATE TABLE Bookings (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    BookingReference VARCHAR(30) NOT NULL UNIQUE,
    UserId INT NOT NULL,
    FlightId INT NOT NULL,
    SeatId INT NOT NULL,
    PassengerName VARCHAR(120) NOT NULL,
    PassengerEmail VARCHAR(120) NOT NULL,
    Status VARCHAR(30) NOT NULL DEFAULT 'Pending',
    BookedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    FOREIGN KEY (FlightId) REFERENCES Flights(Id),
    FOREIGN KEY (SeatId) REFERENCES Seats(Id),
    UNIQUE (FlightId, SeatId)
);

CREATE TABLE Payments (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    BookingId INT NOT NULL UNIQUE,
    Amount DECIMAL(10,2) NOT NULL,
    Method VARCHAR(30) NOT NULL,
    Status VARCHAR(30) NOT NULL DEFAULT 'Pending',
    PaidAt DATETIME NULL,
    FOREIGN KEY (BookingId) REFERENCES Bookings(Id) ON DELETE CASCADE
);

INSERT INTO Users (FullName, Email, PasswordHash, Role) VALUES
('System Admin', 'admin@flight.com', '$2a$11$QJdq1yef0RXa7cjM2BMhEexFJ9OtPIH3I27mWSIpXo9H7fE7v5WRq', 'Admin'),
('Juan Dela Cruz', 'user@flight.com', '$2a$11$QJdq1yef0RXa7cjM2BMhEexFJ9OtPIH3I27mWSIpXo9H7fE7v5WRq', 'User');

INSERT INTO Airlines (Name, Code) VALUES
('Philippine Airlines', 'PR'),
('Cebu Pacific', '5J'),
('AirAsia Philippines', 'Z2');

INSERT INTO Airports (Code, Name, City, Country) VALUES
('MNL', 'Ninoy Aquino International Airport', 'Manila', 'Philippines'),
('CEB', 'Mactan-Cebu International Airport', 'Cebu', 'Philippines'),
('DVO', 'Francisco Bangoy International Airport', 'Davao', 'Philippines'),
('ILO', 'Iloilo International Airport', 'Iloilo', 'Philippines');

INSERT INTO Routes (OriginAirportId, DestinationAirportId) VALUES
(1, 2), (2, 1), (1, 3), (1, 4);

INSERT INTO Flights (FlightNumber, AirlineId, RouteId, DepartureTime, ArrivalTime, BasePrice, Status) VALUES
('PR1845', 1, 1, '2026-06-01 08:00:00', '2026-06-01 09:30:00', 3500.00, 'Scheduled'),
('5J553', 2, 2, '2026-06-01 14:00:00', '2026-06-01 15:25:00', 2800.00, 'Scheduled'),
('Z2611', 3, 3, '2026-06-02 10:15:00', '2026-06-02 12:00:00', 4200.00, 'Scheduled');

INSERT INTO Seats (FlightId, SeatNumber, SeatClass)
SELECT f.Id, CONCAT(row_num, seat_letter), 'Economy'
FROM Flights f
JOIN (
    SELECT 1 row_num UNION SELECT 2 UNION SELECT 3 UNION SELECT 4 UNION SELECT 5
) r
JOIN (
    SELECT 'A' seat_letter UNION SELECT 'B' UNION SELECT 'C' UNION SELECT 'D'
) s;
