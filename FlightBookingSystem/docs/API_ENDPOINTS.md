# API Endpoint Documentation

## Authentication

- POST `/api/auth/register` - Create user account
- POST `/api/auth/login` - Login and receive JWT token

## Flights

- GET `/api/flights` - Search/filter flights
- GET `/api/flights/{id}` - View flight details and seats
- POST `/api/flights` - Admin creates flight with generated seats

## Reference Data

- GET `/api/airports`
- GET `/api/airlines`
- GET `/api/routes`

## Bookings

- POST `/api/bookings` - Create booking and payment record
- GET `/api/bookings/my` - View current user's booking history
- PUT `/api/bookings/{id}/cancel` - Cancel a booking and release the seat

## Reports

- GET `/api/admin/dashboard` - Admin dashboard metrics

## Security

Protected endpoints require this header:

```text
Authorization: Bearer YOUR_JWT_TOKEN
```
