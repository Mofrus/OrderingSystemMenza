# 🍴 Objednávací Systém pro Menzu - UTB.Minute

Semestrální projekt pro předmět **Aplikační Frameworky** (Half-semester Submission)

## 📋 Obsah Dokumentace

- [Technologický Stack](#-technologický-stack)
- [Projektová Struktura](#-projektová-struktura)
- [Datový Model](#-datový-model)
- [Spuštění Projektu](#-spuštění-projektu)
- [WebAPI Endpointy](#-webapi-endpointy)
- [Testy](#-testy)
- [Architektonická Rozhodnutí](#-architektonická-rozhodnutí)

## 🚀 Technologický Stack

- **Runtime**: .NET 10
- **Web Framework**: ASP.NET Core Minimal WebAPI (TypedResults)
- **ORM**: Entity Framework Core 10
- **Databáze**: PostgreSQL
- **Orchestrace**: .NET Aspire
- **Testování**: xUnit + WebApplicationFactory
- **Service Discovery**: Aspire

## 📂 Projektová Struktura

```
UTB.Minute (Solution)
│
├── UTB.Minute.AppHost                 # Aspire orchestrace
│   └── AppHost.cs                     # PostgreSQL + Service Discovery
│
├── UTB.Minute.Db                      # Database Layer
│   ├── Entities/
│   │   ├── Meal.cs
│   │   ├── MenuItem.cs
│   │   ├── Order.cs
│   │   └── OrderStatus.cs
│   └── MinuteDbContext.cs
│
├── UTB.Minute.Contracts               # DTO Layer (Single Source of Truth)
│   ├── Meals/
│   │   ├── MealDto.cs
│   │   ├── CreateMealDto.cs
│   │   └── UpdateMealDto.cs
│   ├── Menu/
│   │   ├── MenuItemDto.cs
│   │   ├── CreateMenuItemDto.cs
│   │   └── UpdateMenuItemDto.cs
│   ├── Orders/
│   │   ├── OrderDto.cs
│   │   ├── CreateOrderDto.cs
│   │   └── UpdateOrderStatusDto.cs
│   └── Enums/
│       └── OrderStatus.cs
│
├── UTB.Minute.WebApi                  # REST API
│   ├── Endpoints/
│   │   ├── MealsEndpoints.cs
│   │   ├── MenuEndpoints.cs
│   │   └── OrdersEndpoints.cs
│   ├── Mappers/
│   │   ├── MealMapper.cs
│   │   ├── MenuItemMapper.cs
│   │   └── OrderMapper.cs
│   ├── Program.cs
│   └── appsettings.json
│
├── UTB.Minute.DbManager               # Database Management
│   ├── Program.cs                     # POST /reset-db, Seed data
│   └── appsettings.json
│
├── UTB.Minute.WebApi.Tests            # Integration Tests
│   ├── MealsTests.cs
│   ├── MenuTests.cs
│   └── OrdersTests.cs
│
└── README.md                           # Tato dokumentace
```

## 💾 Datový Model

### Entita: Meal (Jídlo)

| Pole | Typ | Popis |
|------|-----|-------|
| `Id` | Guid | Primární klíč |
| `Name` | string | Název jídla |
| `Description` | string | Popis |
| `Price` | decimal(10,2) | Cena |
| `IsActive` | bool | Zda je aktivní (neodstraňuje se) |
| `CreatedAt` | DateTime | Čas vytvoření |
| `UpdatedAt` | DateTime | Čas poslední úpravy |

### Entita: MenuItem (Položka Menu)

| Pole | Typ | Popis |
|------|-----|-------|
| `Id` | Guid | Primární klíč |
| `Date` | DateOnly | Datum položky menu |
| `MealId` | Guid | FK na Meal |
| `AvailablePortions` | int | Počet dostupných porcí |
| `CreatedAt` | DateTime | Čas vytvoření |
| `UpdatedAt` | DateTime | Čas poslední úpravy |

### Entita: Order (Objednávka)

| Pole | Typ | Popis |
|------|-----|-------|
| `Id` | Guid | Primární klíč |
| `MenuItemId` | Guid | FK na MenuItem |
| `StudentIdentifier` | string | Identifikátor studenta |
| `Status` | OrderStatus | Stav objednávky (enum) |
| `CreatedAt` | DateTime | Čas vytvoření |
| `UpdatedAt` | DateTime | Čas poslední úpravy |
| `RowVersion` | byte[] | Concurrency token |

### OrderStatus (Enum)

```csharp
public enum OrderStatus
{
    Preparing = 0,    // Připravuje se
    Ready = 1,        // Hotová
    Cancelled = 2,    // Zrušená
    Completed = 3     // Dokončená
}
```

## 🎯 Spuštění Projektu

### Požadavky

- Visual Studio 2026
- .NET 10 SDK
- Docker (spuštěný v background)
- PostgreSQL port 5432 (přes Docker Aspire)

### Kroky Spuštění

1. **Otevřít Solution v Visual Studio**
   ```
   File → Open → UTB.Minute.sln
   ```

2. **Nastavit Startup Project**
   ```
   Solution Explorer → Solution → Right-click
   → Properties → Multiple Startup Projects
   → Vybrat: UTB.Minute.AppHost
   ```

3. **Spustit (F5 nebo Ctrl+F5)**
   - Aspire Dashboard se otevře: `http://localhost:18888`
   - PostgreSQL se automaticky spustí
   - WebAPI: `http://localhost:5000`
   - DbManager: `http://localhost:5001`

### Reset Databáze

```bash
# HTTP POST
curl -X POST http://localhost:5001/reset-db

# Odpověď:
{
  "message": "Database reset and seeded successfully"
}
```

## 📡 WebAPI Endpointy

### Meals (Jídla)

```
GET /meals
```
Vrací seznam všech jídel

```
POST /meals
Content-Type: application/json

{
  "name": "Guláš",
  "description": "Tradiční český guláš",
  "price": 150.00
}
```

```
PUT /meals/{id}
Content-Type: application/json

{
  "name": "Guláš",
  "description": "Tradiční český guláš",
  "price": 150.00,
  "isActive": true
}
```

### Menu Items (Položky Menu)

```
GET /menu
```
Vrací všechny položky menu

```
GET /menu/date/{date}
```
Parametr: `date` formát YYYY-MM-DD

```
POST /menu
Content-Type: application/json

{
  "date": "2025-01-20",
  "mealId": "550e8400-e29b-41d4-a716-446655440000",
  "availablePortions": 50
}
```

```
PUT /menu/{id}
Content-Type: application/json

{
  "date": "2025-01-20",
  "mealId": "550e8400-e29b-41d4-a716-446655440000",
  "availablePortions": 50
}
```

```
DELETE /menu/{id}
```

### Orders (Objednávky)

```
GET /orders
```
Vrací všechny objednávky

```
GET /orders/pending
```
Vrací pouze nedokončené objednávky

```
GET /orders/{id}
```

```
POST /orders
Content-Type: application/json

{
  "menuItemId": "550e8400-e29b-41d4-a716-446655440000"
}
```

```
PUT /orders/{id}/status
Content-Type: application/json

{
  "status": "Ready"
}
```
Možné stavy: `Preparing`, `Ready`, `Cancelled`, `Completed`

## ✅ Testy

### Spuštění Testů

```bash
cd UTB.Minute.WebApi.Tests
dotnet test
```

### Testovací Pokryti

✅ **Meals**
- GetMeals - vrací 200 OK
- CreateMeal - vytvoří nové jídlo (201 Created)
- UpdateMeal - aktualizuje jídlo (200 OK)

✅ **Menu Items**
- GetMenuItems - vrací 200 OK
- CreateMenuItem - vytvoří položku menu (201 Created)
- DeleteMenuItem - smaže položku (200 OK)

✅ **Orders**
- CreateOrder - vytvoří objednávku (201 Created)
- GetPendingOrders - vrací nedokončené (200 OK)
- UpdateOrderStatus - změní stav (200 OK)

### Testovací Databáze

- Testy používají PostgreSQL databázi `minute_test_db`
- Automatický setup: `EnsureDeleted()` → `EnsureCreated()`
- Bez ruční konfigurace

## 🏛️ Architektonická Rozhodnutí

### 1. DTO vs Entity Separation
**Důvod**: API nikdy neexponuje databázové entity  
**Implementace**: Dedikovaný `Contracts` projekt s mapperama

```csharp
// Entity
public class Meal { ... }

// DTO
public class MealDto { ... }

// Mapper
public static MealDto ToDto(this Meal meal) { ... }
```

### 2. Minimal WebAPI bez Kontrolerů
**Důvod**: Jednoduchost, typová bezpečnost, snadnější testování  
**Implementace**: Extension metody pro mapování endpointů

```csharp
app.MapMealsEndpoints();
app.MapMenuEndpoints();
app.MapOrdersEndpoints();
```

### 3. TypedResults místo IActionResult
**Důvod**: Silná typová kontrola, lepší IntelliSense  
**Implementace**: `TypedResults.Ok()`, `TypedResults.Created()`, apod.

### 4. Aspire Service Discovery
**Důvod**: Bez hardcodovaných IP adres, automatická orchestrace  
**Konfiguracija**:
```csharp
var postgres = builder.AddPostgres("postgres");
var database = postgres.AddDatabase("minute_db");
var webApi = builder.AddProject<UTB_Minute_WebApi>("webapi")
    .WithReference(database)
    .WaitFor(database);
```

### 5. Concurrency Control
**Důvod**: Bezpečné objednávání poslední porce  
**Implementace**: `RowVersion` (timestamp) na Order entitě

```csharp
public byte[] RowVersion { get; set; }
```

### 6. EF Core Best Practices
- Default values: `HasDefaultValueSql("CURRENT_TIMESTAMP")`
- Precision: `HasPrecision(10, 2)` na peněz
- Cascade delete pro orphaned records

## 📋 Checklist - Půlsemestrální Odevzdání (20 bodů)

### Projekty (3 body)
- ✅ Všechny projekty existují (UTB.Minute.Db, Contracts, WebApi, DbManager, Tests)
- ✅ Správné pojmenování
- ✅ Správné reference mezi projekty

### Datový Model (5 bodů)
- ✅ Entity a vazby odpovídají zadání
- ✅ DbContext správně nakonfigurován
- ✅ OrderStatus jako enum
- ✅ DTO v Contracts projektu
- ✅ WebAPI vrací DTO, ne entity

### WebAPI a Testy (6 bodů)
- ✅ Meals: Create, Read, Update (deaktivace)
- ✅ Menu: Create, Read, Update, Delete
- ✅ Orders: Create, Read, Change Status

### Aspire Integrace (4 body)
- ✅ PostgreSQL přes Aspire
- ✅ Http Command `/reset-db`
- ✅ Seed testovacích dat
- ✅ Service Discovery bez pevných adres

### Testy a Dokumentace (2 body)
- ✅ xUnit testy s reálnou databází
- ✅ README.md dokumentace

---

## 👥 Týmová práce

**Přispívající**:
- Student 1: 33%
- Student 2: 33%
- Student 3: 34%

---

**Projekt je připraven pro půlsemestrální odevzdání!**
5.  **Clean Code:** Použití `TypedResults` v Minimal API a nezávislost DTO na entitách.

---

## ✅ Checklist před odevzdáním

> [!CAUTION]
> **Důležité pravidlo:** Pokud se projekt nesestaví, nespustí nebo nebude splňovat verzi .NET 10 / angličtinu, je hodnocen **0 body** bez ohledu na implementaci.

### Funkcionalita
- [ ] Funkční Service Discovery (žádné hardcoded IP adresy).
- [ ] Implementován HTTP Command pro reset a seed databáze.
- [ ] SSE notifikace jsou doručovány studentům i kuchařkám.
- [ ] Keycloak správně chrání přístup k aplikacím.
- [ ] Entity Framework migrace fungují korektně.

### Architektura
- [ ] Minimal API používá `TypedResults`.
- [ ] DTO jsou striktně oddělena od databázových entit.
- [ ] Klienti přistupují k datům pouze přes API.

---

## 🏁 Jak začít

1.  Ujistěte se, že máte nainstalované **.NET 10 SDK** a **Docker Desktop** (pro kontejnery v Aspire).
2.  Klonujte repozitář: `git clone https://github.com/Mofrus/OrderingSystemMenza.git`
3.  Otevřete solution `OrderingSystemMenza.sln`.
4.  Nastavte projekt **UTB.Minute.AppHost** jako startovací projekt.
5.  Spusťte aplikaci (F5) – Aspire Dashboard se postará o zbytek.

---
*Vytvořeno jako semestrální projekt pro UTB Zlín.*
