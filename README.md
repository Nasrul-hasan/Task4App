# Task4App (Task 4 - C#)

A simple ASP.NET Core MVC web application for user management.

## Features

- User Registration
- Email Confirmation (manual simulation)
- Login with validation
- Block / Unblock users
- Multi-select users
- Block all users (including current user → auto logout)
- Session-based authentication
- SQLite database
- Indexed database (Email & IsBlocked)

##  Functionality Overview

- Only confirmed users can log in
- Blocked users cannot log in
- Admin can:
  - Select multiple users
  - Block / Unblock users
  - Block themselves (auto logout)

##  Database

- SQLite database (`app.db`)
- Indexes:
  - `IX_Users_Email` (unique)
  - `IX_Users_IsBlocked`

## Technologies Used

- ASP.NET Core MVC (.NET 8)
- Entity Framework Core
- SQLite
- Bootstrap 5
- Docker (for deployment)

## 🌐 Live Demo

👉 https://task4app.onrender.com

## 📂 Source Code

👉 https://github.com/Nasrul-hasan/Task4App

## ▶️ How to Run Locally

```bash
git clone https://github.com/Nasrul-hasan/Task4App
cd Task4App
dotnet run
