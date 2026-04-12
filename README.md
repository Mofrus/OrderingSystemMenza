# 🍴 UTB.Minute — Ordering System for University Canteen

Semester project for **Application Frameworks** course at UTB Zlín.

## 📂 Architecture & Project Responsibilities

```
UTB.Minute (Solution)
│
├── UTB.Minute.AppHost             # .NET Aspire orchestration (PostgreSQL, service discovery)
│
├── UTB.Minute.Db                  # Database layer (EF Core entities, DbContext)
│   ├── Entities/
│   │   ├── Meal.cs
│   │   ├── MenuItem.cs
│   │   ├── Order.cs
│   │   └── OrderStatus.cs
│   └── MinuteDbContext.cs
│
├── UTB.Minute.Contracts           # DTO layer (no EF dependencies)
│   ├── Enums/OrderStatus.cs
│   ├── Meals/    (MealDto, CreateMealDto, UpdateMealDto)
│   ├── Menu/     (MenuItemDto, CreateMenuItemDto, UpdateMenuItemDto)
│   └── Orders/   (OrderDto, CreateOrderDto, UpdateOrderStatusDto)
│
├── UTB.Minute.WebApi              # Minimal API — returns DTOs only
│   ├── Endpoints/  (MealsEndpoints, MenuEndpoints, OrdersEndpoints)
│   ├── Mappers/    (MealMapper, MenuItemMapper, OrderMapper)
│   └── Program.cs
│
├── UTB.Minute.DbManager          # DB management API (reset + seed)
│   └── Program.cs                # POST /db/reset-seed
│
└── UTB.Minute.WebApi.Tests       # Integration tests (Aspire + PostgreSQL)
    ├── AspireFixture.cs
    ├── MealsTests.cs
    ├── MenuTests.cs
    └── OrdersTests.cs
```

## 🚀 Tech Stack

- **.NET 10** / ASP.NET Core Minimal API
- **Entity Framework Core 10** + **PostgreSQL**
- **.NET Aspire** for orchestration & service discovery
- **xUnit** integration tests via Aspire Testing

## 💾 Data Model

| Entity | Key Fields |
|--------|-----------|
| **Meal** | Id, Name, Description, Price, IsActive, CreatedAt, UpdatedAt |
| **MenuItem** | Id, Date, MealId (FK→Meal), AvailablePortions, CreatedAt, UpdatedAt |
| **Order** | Id, MenuItemId (FK→MenuItem), StudentIdentifier, Status (enum), CreatedAt, UpdatedAt, RowVersion |

**OrderStatus enum:** `Preparing`, `Ready`, `Cancelled`, `Completed`

## 📡 API Endpoints

### Meals
| Method | Route | Description |
|--------|-------|-------------|
| POST | `/meals` | Create a new meal (201) |
| GET | `/meals` | List all meals (200) |
| PUT | `/meals/{id}` | Update a meal (200 / 404) |
| PATCH | `/meals/{id}/deactivate` | Soft-deactivate a meal (204 / 404) |

### Menu Items
| Method | Route | Description |
|--------|-------|-------------|
| POST | `/menu-items` | Create a menu item (201) |
| GET | `/menu-items` | List all menu items (200) |
| PUT | `/menu-items/{id}` | Update a menu item (200 / 404) |
| DELETE | `/menu-items/{id}` | Delete a menu item (204 / 404) |

### Orders
| Method | Route | Description |
|--------|-------|-------------|
| POST | `/orders` | Create an order (201) |
| GET | `/orders` | List all orders (200) |
| PATCH | `/orders/{id}/status` | Update order status (200 / 404) |

### DbManager
| Method | Route | Description |
|--------|-------|-------------|
| POST | `/db/reset-seed` | Reset DB and seed test data (200) |

## 🎯 How to Run (Aspire)

### Prerequisites
- .NET 10 SDK
- Docker Desktop (running)

### Steps
1. Open `UTB.Minute.sln` in Visual Studio / Rider.
2. Set **UTB.Minute.AppHost** as the startup project.
3. Run (F5) — Aspire Dashboard opens automatically.
   - PostgreSQL starts via Docker.
   - WebApi and DbManager are registered via service discovery.
4. Seed the database: `POST http://<dbmanager-url>/db/reset-seed`

## ✅ How to Run Tests

```bash
dotnet test UTB.Minute.WebApi.Tests
```

Tests use **Aspire.Hosting.Testing** to start all services (including PostgreSQL) in a test context. No manual Docker or database setup is needed — Docker must be running.

## 👥 Team Contributions

| Member | Contribution |
|--------|-------------|
| Student 1 | 33 % |
| Student 2 | 33 % |
| Student 3 | 34 % |

---
*Created as a semester project for UTB Zlín.*
