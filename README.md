# Currency Exchange API

A RESTful API for currency conversion to Danish Krone (DKK) with real-time exchange rates from Nationalbanken (Danmarks Nationalbank).

## Prerequisites

- .NET 9.0 SDK or later
- SQL Server 2019 or later (or use Docker)
- Visual Studio 2022 / VS Code

## Quick Start

### 1. Start SQL Server with Docker (skip to next step if using SQL Server locally)

```bash
docker-compose up -d
```

This will start SQL Server 2022 on `localhost:1433` with the following credentials:
- **Username**: `sa`
- **Password**: `YourStrong@Passw0rd`
- **Database**: `CurrencyExchangeDb` (auto-created by EF)

### 2. Update Connection String (Optional)

If using a different SQL Server instance, update `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=CurrencyExchangeDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;"
  }
}
```

### 3. Run Database Migrations

The API automatically applies migrations on startup. Alternatively, run manually:

```bash
cd src/CurrencyExchange.Infrastructure
dotnet ef database update --startup-project ../CurrencyExchange.API/CurrencyExchange.API.csproj
```

### 4. Run the API

```bash
cd src/CurrencyExchange.API
dotnet run
```

The API will be available at:
- **HTTP**: `http://localhost:5129`
- **Swagger UI**: `http://localhost:5129/`

## API Endpoints

### Authentication

#### Login (Get JWT Token)
```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "admin123"
}

Response:
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "expiration": "2025-11-22T14:30:00Z"
}
```

### Currency Rates

#### Get All Currency Rates
```http
GET /api/currencyrates
Authorization: Bearer {token}

Response:
[
  {
    "currencyCode": "USD",
    "currencyName": "US Dollar",
    "rateToDKK": 6.95,
    "fetchedAt": "2025-11-22T12:00:00Z"
  },
  ...
]
```

#### Get Rate by Currency Code
```http
GET /api/currencyrates/USD
Authorization: Bearer {token}

Response:
{
  "currencyCode": "USD",
  "currencyName": "US Dollar",
  "rateToDKK": 6.95,
  "fetchedAt": "2025-11-22T12:00:00Z"
}
```

#### Update Currency Rates
```http
POST /api/currencyrates/update
Authorization: Bearer {token}

Response:
{
  "message": "Currency rates updated successfully. Total rates: 42",
  "timestamp": "2025-11-22T12:30:00Z"
}
```

### Currency Conversion

#### Convert Currency to DKK
```http
POST /api/conversions/convert
Authorization: Bearer {token}
Content-Type: application/json

{
  "fromCurrency": "USD",
  "amount": 100
}

Response:
{
  "fromCurrency": "USD",
  "toCurrency": "DKK",
  "originalAmount": 100,
  "convertedAmount": 695.00,
  "exchangeRate": 6.95,
  "conversionDate": "2025-11-22T12:45:00Z"
}
```

#### Get Conversion History
```http
GET /api/conversions/history?fromCurrency=USD&startDate=2025-11-23&endDate=2025-11-23
Authorization: Bearer {token}

Response:
[
  {
    "id": 1,
    "fromCurrency": "USD",
    "toCurrency": "DKK",
    "originalAmount": 100,
    "convertedAmount": 695.00,
    "exchangeRate": 6.95,
    "conversionDate": "2025-11-22T12:45:00Z"
  },
  ...
]
```

Query Parameters:
- `fromCurrency` (optional): Filter by source currency
- `fromDate` (optional): Filter from date (ISO 8601)
- `toDate` (optional): Filter to date (ISO 8601)

## Configuration

### JWT Settings

Edit `appsettings.json`:

```json
{
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "CurrencyExchangeAPI",
    "Audience": "CurrencyExchangeUsers",
    "ExpirationMinutes": 60
  }
}
```

### Background Job Schedule

The currency rate update job runs every 60 minutes. To modify the schedule, edit `DependencyInjection.cs`:

```csharp
.WithCronSchedule("0 0 * * * ?") // Change this cron expression
```

Cron expression examples:
- `0 0 * * * ?` - Every hour at minute 0
- `0 */30 * * * ?` - Every 30 minutes
- `0 0 */2 * * ?` - Every 2 hours

### Logging

Serilog is configured to write to:
- **Console**: All log levels
- **File**: `logs/currency-exchange-{Date}.txt` (rolling daily)

Modify `appsettings.json` to change log levels:

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    }
  }
}
```

## Running Tests

```bash
cd tests/CurrencyExchange.Tests
dotnet test
```

Test coverage:
- 14 unit tests
- Domain entity validation
- Currency rate service operations
- Currency conversion service logic
- Edge cases and error handling

## External API

The API fetches exchange rates from **Danmarks Nationalbank**:
- **URL**: `https://www.nationalbanken.dk/api/currencyratexmlapi`
- **Format**: XML
- **Update Frequency**: Daily (weekdays)

### Supported Currencies

All currencies published by Nationalbanken are supported. Common examples:
- USD (US Dollar)
- EUR (Euro)
- GBP (British Pound)
- JPY (Japanese Yen)
- CHF (Swiss Franc)
- SEK (Swedish Krona)
- NOK (Norwegian Krone)
- And many more...

### Migration Issues

Reset database:
```bash
cd src/CurrencyExchange.Infrastructure
dotnet ef database drop --startup-project ../CurrencyExchange.API/CurrencyExchange.API.csproj
dotnet ef database update --startup-project ../CurrencyExchange.API/CurrencyExchange.API.csproj
```

## Technology Stack

- **Framework**: .NET 9.0
- **ORM**: Entity Framework Core 9.0
- **Database**: SQL Server 2022
- **Background Jobs**: Quartz.NET 3.15.1
- **Authentication**: JWT Bearer Tokens
- **Logging**: Serilog 9.0
- **API Documentation**: Swagger/Swashbuckle 6.9
- **Testing**: xUnit 2.9.2, Moq 4.20.72, FluentAssertions 7.0.1