# Minimal API Demo

This repository contains a minimal API implementation using ASP.NET Core, featuring endpoints for managing coupons and user authentication. The project demonstrates CRUD operations for coupons and user management functionalities including login, registration, token refresh, and token revocation.

## Table of Contents

- [Features](#features)
- [Coupon Endpoints](#coupon-endpoints)
- [Authentication Endpoints](#authentication-endpoints)
- [Usage](#usage)
- [Setup](#setup)
- [Contributing](#contributing)
- [License](#license)

## Features

### Coupon Management
- **CRUD Operations:** Create, Retrieve, Update, and Delete coupons.
- **Validation:** Uses FluentValidation for input validation.
- **Authorization:** Requires authorization for all coupon-related endpoints.

### Authentication
- **Login:** Authenticate users and issue access tokens.
- **Registration:** Register new users with unique email addresses.
- **Token Refresh:** Refresh access tokens using valid refresh tokens.
- **Token Revocation:** Revoke refresh tokens to invalidate them.

## Coupon Endpoints

### Get All Coupons

- **URL:** `/api/getCoupons`
- **Method:** `GET`
- **Authorization:** Required (`AdminOnly`)
- **Responses:**
  - `200 OK`: Returns a list of all coupons.
  - `401 Unauthorized`: If the user is not authorized.

### Get Coupon By ID

- **URL:** `/api/getCouponById/{id:int}`
- **Method:** `GET`
- **Authorization:** Required
- **Responses:**
  - `200 OK`: Returns the coupon with the specified ID.
  - `401 Unauthorized`: If the user is not authorized.

### Create Coupon

- **URL:** `/api/createCoupon`
- **Method:** `POST`
- **Authorization:** Required
- **Request Body:** `CouponCreateDTO` (JSON)
- **Responses:**
  - `201 Created`: Coupon created successfully.
  - `400 Bad Request`: Validation error or duplicate coupon name.
  - `401 Unauthorized`: If the user is not authorized.
  - `404 Not Found`: If the resource is not found.

### Update Coupon

- **URL:** `/api/updateCoupon`
- **Method:** `PUT`
- **Authorization:** Required
- **Request Body:** `CouponUpdateDTO` (JSON)
- **Responses:**
  - `200 OK`: Coupon updated successfully.
  - `400 Bad Request`: Validation error.
  - `404 Not Found`: Coupon not found.
  - `401 Unauthorized`: If the user is not authorized.

### Delete Coupon

- **URL:** `/api/deleteCoupon/{id:int}`
- **Method:** `DELETE`
- **Authorization:** Required
- **Responses:**
  - `204 No Content`: Coupon deleted successfully.
  - `404 Not Found`: Coupon not found.
  - `401 Unauthorized`: If the user is not authorized.

## Authentication Endpoints

### Login

- **URL:** `/api/login`
- **Method:** `POST`
- **Request Body:** `LoginRequestDto` (JSON)
- **Responses:**
  - `200 OK`: Returns an access token if login is successful.
  - `400 Bad Request`: If validation fails or credentials are incorrect.
  - `401 Unauthorized`: If authentication is not authorized.

### Register

- **URL:** `/api/register`
- **Method:** `POST`
- **Request Body:** `RegistrationRequestDto` (JSON)
- **Responses:**
  - `201 Created`: Returns user information if registration is successful.
  - `400 Bad Request`: If the email already exists or registration fails.

### Refresh Token

- **URL:** `/api/refreshToken`
- **Method:** `POST`
- **Request Body:** `TokenDto` (JSON)
- **Responses:**
  - `200 OK`: Returns a new access token if the refresh token is valid.
  - `400 Bad Request`: If the refresh token is invalid or validation fails.

### Revoke Refresh Token

- **URL:** `/api/revokeRefreshToken`
- **Method:** `POST`
- **Request Body:** `TokenDto` (JSON)
- **Responses:**
  - `200 OK`: Indicates that the refresh token has been revoked successfully.
  - `400 Bad Request`: If validation fails.

## Usage

1. **Run the application** using your preferred method (e.g., Visual Studio or CLI).
2. **Make HTTP requests** to the endpoints using tools like Postman or CURL.
3. **Ensure authorization** headers are provided for endpoints requiring authentication.

## Setup

1. **Clone the repository:**

    ```bash
    git clone https://github.com/yourusername/MinimalAPIDemo.git
    ```

2. **Navigate to the project directory:**

    ```bash
    cd MinimalAPIDemo
    ```

3. **Install dependencies** using .NET CLI:

    ```bash
    dotnet restore
    ```

4. **Build the project:**

    ```bash
    dotnet build
    ```

5. **Run the application:**

    ```bash
    dotnet run
    ```

## Contributing

Contributions are welcome! Please fork the repository and submit a pull request with your changes. Ensure that your code adheres to the project's coding standards and includes appropriate tests.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

Feel free to customize the URL and repository details to fit your actual project. This README provides a comprehensive overview of the available endpoints and their usage.
