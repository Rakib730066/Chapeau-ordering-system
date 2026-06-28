# Chapeau Restaurant Ordering System

A full-stack restaurant point-of-sale (POS) web application built with ASP.NET Core MVC and Microsoft SQL Server. The system supports the complete workflow of a restaurant — from table management and order taking through kitchen/bar preparation to payment.

## Features

### Restaurant Overview
- Interactive floor plan showing all tables and their real-time status (Free, Occupied, Reserved, Cleaning)
- One-click navigation to start or load an order for any occupied table

### Taking Orders (Waiter)
- Browse the full menu filtered by type (Food / Drinks), card (Lunch / Dinner / Drinks), and course (Starter, Entremet, Main, Dessert)
- Add items to an order, adjust quantities, and add per-item comments (e.g. "no salt")
- Stock tracking — out-of-stock items are disabled; low-stock items show a warning badge
- Send order to kitchen/bar with a confirmation prompt
- Multiple rounds — start a new round for the same table after the first is sent
- Order history panel showing all sent items and their live preparation status
- Mark individual items as served once they reach the table
- Running table total across all rounds with VAT breakdown per rate

### Bar & Kitchen
- View all active orders split by role (Bar sees drinks, Kitchen sees food)
- Course-based grouping for food items
- Update item status: Ordered → Being Prepared → Ready to Be Served
- Bulk actions: Start All / Ready All for an order, or per-course controls
- Finished Today view showing completed orders

### Payment
- View the full bill for a table
- Split payment across multiple guests
- Supports Cash, Debit, and Credit payment methods
- Tip entry

### Management
- Employee management (add, edit, deactivate)
- Menu item management (add, edit, toggle active/inactive, update stock)
- Financial overview with daily revenue reports

### Account
- Role-based login: Waiter, Bar, Kitchen, Manager
- Each role is redirected to their relevant module on login
- Session-based authentication with route guards per role

## Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core 8 MVC |
| Language | C# |
| Database | Microsoft SQL Server |
| Data Access | ADO.NET (raw SQL, no ORM) |
| Frontend | Razor Views, Bootstrap 5 |
| Auth | ASP.NET Core Session |

## Architecture

The project follows a layered architecture with strict separation of concerns:

```
Controllers   →   Services   →   Repositories   →   Database
     ↓               ↓
 ViewModels        Models
```

- **Controllers** — handle HTTP requests, session, and redirects only
- **Services** — all business rules and domain logic
- **Repositories** — all SQL queries and database access
- **Models** — domain entities with computed properties (no logic leaking into views)
- **ViewModels** — shape data for the view; contain display-only computed properties
- **BaseController** — shared role guards (`WaiterGuard`, `ManagerGuard`, etc.) and TempData helpers

## Project Structure

```
Chapeau-ordering-system/
├── Controllers/
│   ├── BaseController.cs
│   ├── AccountController.cs
│   ├── OrderController.cs
│   ├── BarKitchenController.cs
│   ├── PaymentController.cs
│   ├── ManagementController.cs
│   └── RestaurantOverviewController.cs
├── Services/
│   ├── Interfaces/
│   └── Implementations/
├── Repositories/
│   ├── Interfaces/
│   └── Implementations/
├── Models/
├── ViewModels/
├── Views/
├── Infrastructure/
│   └── AppClock.cs
└── Mappers/
    ├── EmployeeMapper.cs
    └── MenuItemMapper.cs
```

## Database Setup

1. Create a SQL Server database named `ChapeauDatabase`
2. Run `SQLQuery1.sql` to seed the menu items
3. Run `SQLQuery2.sql` to create the schema (tables, constraints)
4. Update the connection string in `appsettings.json`:

```json
"ConnectionStrings": {
  "ChapeauDatabase": "Server=YOUR_SERVER;Database=ChapeauDatabase;Trusted_Connection=True;"
}
```

## Getting Started

1. Clone the repository
2. Set up the database (see above)
3. Open `Chapeau-ordering-system.sln` in Visual Studio
4. Build and run (`F5`)
5. Log in with your employee credentials

## Employee Roles

| Role | Access |
|---|---|
| Waiter | Restaurant Overview, Take Order |
| Bar | Bar view (drinks orders) |
| Kitchen | Kitchen view (food orders) |
| Manager | Management dashboard, financial reports |

## Menu Cards

| Card | Description |
|---|---|
| Lunch | Starter, Main, Dessert — lunch menu |
| Dinner | Starter, Entremet, Main, Dessert — dinner menu |
| Drinks | All beverages (soft drinks, beer, wine, spirits) |

VAT rates: 9% on food and non-alcoholic drinks, 21% on alcoholic drinks.

## Contributors

6 contributors — Group 5, IT1C
