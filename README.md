# 📚 GestionBiblio - Library Management System (v1.0.0)

A comprehensive web application for library management, developed with ASP.NET Core MVC.

## 🌟 Features

### 👨‍💼 For Administrators
*   **Dashboard**: Overview with statistics (Books, Members, Active/Overdue Loans, Reservations) and charts.
*   **Book Management**: Add, edit, delete books with category management and image upload.
*   **Member Management**: Manage registrations and member profiles.
*   **Loan Tracking**: Record loans, manage returns, view overdue items.
*   **Fine Management**: Create and track payment of fines for delays.
*   **Data Export**: Export lists (e.g., fines) in CSV format.

### 👤 For Members
*   **Catalog**: Search and browse available books.
*   **Personal Space**: View current loans, history, and reservations.
*   **Reservation**: Reserve a book if it is currently out of stock.

## 🛠️ Tech Stack

*   **Framework**: ASP.NET Core 8.0 (MVC)
*   **Language**: C# 10+
*   **Database**: SQL Server (via Entity Framework Core 8)
*   **ORM**: Entity Framework Core (Code-First)
*   **Authentication**: ASP.NET Core Identity
*   **Frontend**:
    *   Razor Views (.cshtml)
    *   **Tailwind CSS** (via CDN) for modern styling.
    *   **Glassmorphism UI** (Custom design with transparency effects).
    *   Bootstrap Icons.
    *   Chart.js for data visualization.

## 🗄️ Database Design

The relational schema includes the following main entities:

*   **Book (Livre)**: Title, Author, ISBN, Image, Stock, Category.
*   **Member (Membre)**: Personal information (linked to Identity User).
*   **Loan (Emprunt)**: Link between Book and Member with Loan Date, Expected Return Date, and Actual Return Date.
*   **Reservation**: Queue for out-of-stock books.
*   **Fine (Amende)**: Financial penalties linked to overdue loans.
*   **Category (Categorie)**: Classification of books.

## 🚀 Installation and Configuration

### Prerequisites
*   [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
*   [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (Express or Developer)
*   Visual Studio 2022 or VS Code

### Installation Steps

1.  **Clone the repository**
    ```bash
    git clone https://github.com/aminecharro01/crimeAnalytics.git
    cd crimeAnalytics
    ```

2.  **Configure Database**
    Open `appsettings.json` and modify the `DefaultConnection` string if necessary to point to your local SQL Server instance.
    ```json
    "ConnectionStrings": {
      "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=LibraryDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
    }
    ```

3.  **Apply Migrations**
    Creates the database and necessary tables.
    ```bash
    dotnet ef database update
    ```

4.  **Run the Application**
    ```bash
    dotnet run
    ```
    The application will be accessible at `https://localhost:7152` (or the port indicated in the console).

5.  **Default Administrator Account**
    On first launch, an administrator account is seeded (see `Program.cs` for details):
    *   **Email**: `admin@gestionbiblio.com`
    *   **Password**: `Admin123!`

## 🎨 Design

The application uses a modern **Glassmorphism** interface:
*   Fixed immersive background.
*   Semi-transparent cards and containers with blur (`backdrop-blur`).
*   Polished typography ('Inter' font).
*   Consistent color palette (Indigo/Slate).

## 👤 Author

**Amine Charro**
[GitHub Profile](https://github.com/aminecharro01)
