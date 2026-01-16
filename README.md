# GestionBiblio

## Overview
GestionBiblio is a comprehensive Library Management System built with ASP.NET Core MVC. It is designed to facilitate the management of library resources, including books, members, and reservations.

## Features
- **Book Management**: Add, update, and remove books from the catalog.
- **Member Management**: Manage library members/users.
- **Reservations**: Handle book reservations and availability.
- **Borrowing System**: Track borrowed books and due dates.

## Technologies Used
- **Framework**: ASP.NET Core MVC
- **Language**: C#
- **ORM**: Entity Framework Core
- **Database**: SQL Server (implied by typical .NET setup)

## Getting Started
1. Clone the repository.
2. Navigate to the `GestionBiblio` directory.
3. Configure the connection string in `appsettings.json`.
4. Run migrations if necessary:
   ```bash
   dotnet ef database update
   ```
5. Run the application:
   ```bash
   dotnet run
   ```

## License
This project is licensed under the MIT License.
