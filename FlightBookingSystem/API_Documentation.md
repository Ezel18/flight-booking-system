# API Documentation
Base URL: http://localhost:5000
Swagger: http://localhost:5000/swagger

## Auth
POST /api/auth/register
POST /api/auth/login

## Flights
GET /api/flights?origin=Manila&destination=Cebu&date=2026-06-01
GET /api/flights/{id}
POST /api/flights - Admin only
PUT /api/flights/{id} - Admin only
DELETE /api/flights/{id} - Admin only

## Bookings
GET /api/bookings?userId=2
POST /api/bookings
PUT /api/bookings/{id}/cancel
GET /api/bookings/{id}/ticket

## Admin
GET /api/admin/dashboard
GET/POST /api/admin/airports
GET/POST /api/admin/airlines
GET /api/admin/users
GET /api/admin/payments
