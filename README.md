# 🍴 UTB.Minute — Ordering System for University Canteen

Semestrální projekt pro předmět **Aplikační frameworky** na UTB ve Zlíně. Projekt představuje kompletní řešení pro objednávání jídel v menze, využívající moderní přístupy ekosystému .NET.

## 🚀 Zvolený Tech Stack

- **.NET 10** (Minimal APIs pro backend, Blazor WebAssembly pro frontend)
- **Entity Framework Core 10** + **PostgreSQL** (Data layer)
- **.NET Aspire** pro orchestraci, Service Discovery a integraci Keycloaku i databáze na lokálním prostředí.
- **Keycloak** (nasazený přes Aspire) pro autentizaci a autorizaci, včetně řízení rolí (OIDC).
- **Server-Sent Events (SSE)** přes `System.Threading.Channels` pro real-time aktualizace UI.
- **xUnit** + **Aspire.Hosting.Testing** pro integrační testování s efemérní PostgreSQL databází.

## 📂 Architektura & Projekty

Řešení je logicky a fyzicky rozděleno do několika projektů:

```text
UTB.Minute (Solution)
│
├── UTB.Minute.AppHost             # Orchestrátor celého prostředí (DB, Keycloak, API, Klienti)
├── UTB.Minute.DbManager           # Pomocné API pro vyčištění a seedování databáze
│
├── Backend:
│   ├── UTB.Minute.Db              # Datová vrstva (DbContext, Entity s [ConcurrencyCheck])
│   ├── UTB.Minute.Contracts       # Sdílená knihovna DTO objektů (zajišťuje striktní izolaci od EF)
│   └── UTB.Minute.WebApi          # Hlavní Minimal API, REST, validace StateMachine, SSE notifikace
│
└── Frontend (Blazor WebAssembly):
    ├── UTB.Minute.AdminClient     # Klient pro vedení menzy (správa jídel a menu)
    └── UTB.Minute.CanteenClient   # Klient pro studenty (objednávání) a kuchaře (výdej a příprava)
```

## 🛠️ Implementovaná funkcionalita (dle rubriky)

### 1. Řešení souběžnosti (Concurrency)
- Vyřešeno pomocí vlastnosti `AvailablePortions` s anotací `[ConcurrencyCheck]`.
- V případě, že se dva studenti pokusí objednat poslední porci ve stejnou milisekundu, Entity Framework vyvolá `DbUpdateConcurrencyException`. 
- API tento stav zachytí a vrací chybový status `409 Conflict`, na který UI příslušně reaguje.

### 2. Autentizace a Autorizace (Keycloak)
- Keycloak kontejner s předpřipraveným realmem `menza` a datovým volumem běží přímo přes `.NET Aspire`.
- Blazor klienti používají standardní `OIDC` a `Microsoft.AspNetCore.Components.WebAssembly.Authentication`.
- Zavedeny role: **Admin** (Vedení menzy), **Student** a **Cook** (Kuchař).

### 3. Server-Sent Events (SSE) notifikace
- Ve `WebApi` implementován `NotificationService` založený na System.Threading.Channels.
- Endpoint `/notifications/stream` odesílá události pro kuchaře (vytvořena nová objednávka) a studenty (změna stavu jejich objednávky).
- UI se díky SSE automaticky aktualizuje bez nutnosti ručního obnovování (F5).

### 4. Role a Funkcionality v Blazor Klientech
- **Student (CanteenClient):** Vidí denní menu. U každého jídla vidí počet zbývajících porcí. Jakmile klesne na 0, zobrazí se vizuální označení `VYPRODÁNO` a tlačítko zmizí. Může objednávat jídla a na kartě Moje objednávky sledovat proces.
- **Kuchařka (CanteenClient):** Vidí nezpracované objednávky. Může měnit jejich stav.
  - Implementována pevná **State Machine validace stavů** na backendu, která blokuje neplatné přechody (např. nelze přepnout stav ze `Zrušeno` zpět na `Hotovo`).
- **Vedení menzy (AdminClient):** Tabulková a formulářová správa Jídel (`Meals`) a Denního menu (`MenuItems`). Obsahuje podporu pro deaktivaci jídel (Soft delete).

## 🎯 Jak to spustit (Local Development)

### Požadavky
- **.NET 10 SDK**
- **Docker Desktop** (musí běžet na pozadí)

### Spuštění projektu
Díky .NET Aspire je spuštění celého komplexního systému otázkou jediného příkazu. Není třeba psát žádné Dockerfiles ani docker-compose.

1. Běžte do složky orchestrátoru:
   ```bash
   cd UTB.Minute.AppHost
   dotnet run
   ```
2. Otevře se okno prohlížeče s **.NET Aspire Dashboard**. Zde uvidíte logy z:
   - kontejneru `postgres` (databáze)
   - kontejneru `keycloak` (autentizace)
   - API `utb-minute-webapi`
   - a obou klientů `utb-minute-adminclient` a `utb-minute-canteenclient`

### Seedování Databáze
Jakmile aplikace běží, je nutné vytvořit tabulky a nasypat základní data. V Aspire Dashboard najděte URL pro `utb-minute-dbmanager` a zavolejte jej:
```bash
curl -X POST http://<dbmanager-url>/db/reset-seed
```

### Testovací účty (Keycloak)
Následně si můžete otevřít adresy klientů a použít tyto údaje:
- **Vedení menzy:** login: `admin`, heslo: `admin` *(otevřít AdminClient)*
- **Student:** login: `student`, heslo: `student` *(otevřít CanteenClient)*
- **Kuchař:** login: `cook`, heslo: `cook` *(otevřít CanteenClient)*

## ✅ Spuštění integračních testů

Integrační testy plně ověřují validace, Minimal APIs a chování Entity Frameworku, a to automatickým vytvořením čisté testovací PostgreSQL přes Aspire testovací knihovnu.

```bash
cd UTB.Minute.WebApi.Tests
dotnet test
```

## 👥 Členové týmu a podíly

| Člen týmu | Podíl na projektu |
|-----------|-------------------|
| Student 1 | 33.33 % |
| Student 2 | 33.33 % |
| Student 3 | 33.33 % |

---
*Vytvořeno jako semestrální projekt pro UTB Zlín.*