# GestionBiblio

## Overview
GestionBiblio is a comprehensive Library Management System built with ASP.NET Core MVC (8.0). It is designed to facilitate the management of library resources, including books, members, and reservations, featuring a modern Glassmorphism UI.

## Features
- **Book Management**: Manage details like Title, Author, ISBN, Image, Stock, and Category.
- **Member Management**: User information management linked with ASP.NET Core Identity.
- **Loans (Emprunt)**: Link between Books and Members with borrow date, due date, and return date.
- **Reservations**: Queue system for out-of-stock books.
- **Fines**: Financial penalties for late returns.
- **Categories**: Classification of books.

## Technical Stack
- **Framework**: ASP.NET Core 8.0 (MVC)
- **Language**: C# 10+
- **Database**: SQL Server (via Entity Framework Core 8)
- **ORM**: Entity Framework Core (Code-First)
- **Authentication**: ASP.NET Core Identity
- **Frontend**:
  - Razor Views (.cshtml)
  - **Tailwind CSS** (via CDN) for modern styling.
  - **Glassmorphism UI** (Custom design with transparency effects).
  - Bootstrap Icons.
  - Chart.js for data visualization.

## Installation and Configuration

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (Express or Developer)
- Visual Studio 2022 or VS Code

### Installation Steps

1. **Clone the repository**
   ```bash
   git clone https://github.com/aminecharro01/GestionBiblioDotNet.git
   cd GestionBiblioDotNet
   ```

2. **Configure the Database**
   Open `appsettings.json` and modify the `DefaultConnection` string if necessary to point to your local SQL Server instance.
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=LibraryDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
   }
   ```

3. **Apply Migrations**
   Creates the database and necessary tables.
   ```bash
   dotnet ef database update
   ```

4. **Run the Application**
   ```bash
   dotnet run
   ```
   The application will be accessible at `https://localhost:7152` (or the port indicated in the console).

5. **Default Administrator Account**
   On first launch, an administrator account is created (see `Program.cs` for seeding details):
   - **Email**: `admin@gestionbiblio.com`
   - **Password**: `Admin123!`

## Design
The application uses a modern **Glassmorphism** interface:
- Immersive fixed background.
- Semi-transparent cards and containers with blur (`backdrop-blur`).
- Polished typography (Inter font).
- Consistent color palette (Indigo/Slate).

## Author
**Amine Charro**
- [GitHub Profile](https://github.com/aminecharro01)
- [LinkedIn Profile](https://www.linkedin.com/in/charroamine/)

## License
This project is licensed under the MIT License.
