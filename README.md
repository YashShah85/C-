# Currency Exchange API

A RESTful API for currency conversion to Danish Krone (DKK) with real-time exchange rates from Nationalbanken (Danmarks Nationalbank).

## Features

- ✅ Fetch and store currency exchange rates from Nationalbanken API
- ✅ Automatic rate updates every 60 minutes using Quartz background jobs
- ✅ Convert any supported currency to DKK
- ✅ Store and query conversion history with filtering
- ✅ JWT authentication for secure API access
- ✅ Comprehensive Swagger/OpenAPI documentation
- ✅ Structured logging with Serilog
- ✅ Clean Architecture with SOLID principles
- ✅ Unit tests with xUnit, Moq, and FluentAssertions
- ✅ Entity Framework Core with SQL Server

## Architecture

The solution follows **Clean Architecture** principles with clear separation of concerns:

```
CurrencyExchange/
├── src/
│   ├── CurrencyExchange.Domain/          # Core business entities and interfaces
│   ├── CurrencyExchange.Application/     # Business logic and DTOs
│   ├── CurrencyExchange.Infrastructure/  # Data access, external services, background jobs
│   └── CurrencyExchange.API/             # REST API controllers and configuration
└── tests/
    └── CurrencyExchange.Tests/           # Unit tests
```

### Design Patterns Used

- **Repository Pattern**: Data access abstraction
- **Dependency Injection**: Loose coupling between layers
- **Factory Pattern**: Background job creation
- **DTO Pattern**: Data transfer objects for API contracts

## Prerequisites

- .NET 9.0 SDK or later
- SQL Server 2019 or later (or use Docker)
- Visual Studio 2022 / VS Code / Rider (optional)

## Quick Start

### 1. Clone the Repository

```bash
git clone <repository-url>
cd CurrencyExchange
```

### 2. Start SQL Server with Docker

```bash
docker-compose up -d
```

This will start SQL Server 2022 on `localhost:1433` with the following credentials:
- **Username**: `sa`
- **Password**: `YourStrong@Passw0rd`
- **Database**: `CurrencyExchangeDb` (auto-created)

### 3. Update Connection String (Optional)

If using a different SQL Server instance, update `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=CurrencyExchangeDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;"
  }
}
```

### 4. Run Database Migrations

The API automatically applies migrations on startup. Alternatively, run manually:

```bash
cd src/CurrencyExchange.Infrastructure
dotnet ef database update --startup-project ../CurrencyExchange.API/CurrencyExchange.API.csproj
```

### 5. Run the API

```bash
cd src/CurrencyExchange.API
dotnet run
```

The API will be available at:
- **HTTP**: `http://localhost:5000`
- **HTTPS**: `https://localhost:5001`
- **Swagger UI**: `https://localhost:5001/` (root path)

## API Endpoints

### Authentication

#### Login (Get JWT Token)
```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "password123"
}

Response:
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "expiration": "2025-11-22T14:30:00Z"
}
```

> **Note**: This is a demo authentication. In production, implement proper user management with hashed passwords.

### Currency Rates

#### Get All Currency Rates
```http
GET /api/currencyrates
Authorization: Bearer {token}

Response:
[
  {
    "id": 1,
    "currencyCode": "USD",
    "currencyName": "US Dollar",
    "rateToDKK": 6.95,
    "lastUpdated": "2025-11-22T12:00:00Z"
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
  "id": 1,
  "currencyCode": "USD",
  "currencyName": "US Dollar",
  "rateToDKK": 6.95,
  "lastUpdated": "2025-11-22T12:00:00Z"
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
  "toCurrency": "DKK",
  "amount": 100
}

Response:
{
  "fromCurrency": "USD",
  "toCurrency": "DKK",
  "amount": 100,
  "convertedAmount": 695.00,
  "rate": 6.95,
  "timestamp": "2025-11-22T12:45:00Z"
}
```

#### Get Conversion History
```http
GET /api/conversions/history?fromCurrency=USD&pageNumber=1&pageSize=20
Authorization: Bearer {token}

Response:
[
  {
    "id": 1,
    "fromCurrency": "USD",
    "toCurrency": "DKK",
    "amount": 100,
    "convertedAmount": 695.00,
    "rate": 6.95,
    "timestamp": "2025-11-22T12:45:00Z"
  },
  ...
]
```

Query Parameters:
- `fromCurrency` (optional): Filter by source currency
- `toCurrency` (optional): Filter by target currency
- `fromDate` (optional): Filter from date (ISO 8601)
- `toDate` (optional): Filter to date (ISO 8601)
- `pageNumber` (default: 1): Page number for pagination
- `pageSize` (default: 20): Items per page

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
- ✅ 14 unit tests
- ✅ Domain entity validation
- ✅ Currency rate service operations
- ✅ Currency conversion service logic
- ✅ Edge cases and error handling

## Database Schema

### CurrencyRates Table
| Column | Type | Description |
|--------|------|-------------|
| Id | int | Primary key |
| CurrencyCode | nvarchar(3) | ISO 4217 currency code (unique) |
| CurrencyName | nvarchar(100) | Full currency name |
| RateToDKK | decimal(18,6) | Exchange rate to DKK |
| LastUpdated | datetime2 | Last rate update timestamp |
| CreatedAt | datetime2 | Record creation timestamp |

**Indexes:**
- Unique index on `CurrencyCode`
- Index on `LastUpdated`

### CurrencyConversions Table
| Column | Type | Description |
|--------|------|-------------|
| Id | int | Primary key |
| FromCurrency | nvarchar(3) | Source currency code |
| ToCurrency | nvarchar(3) | Target currency code |
| Amount | decimal(18,2) | Original amount |
| ConvertedAmount | decimal(18,2) | Converted amount |
| Rate | decimal(18,6) | Exchange rate used |
| Timestamp | datetime2 | Conversion timestamp |

**Indexes:**
- Composite index on `FromCurrency, ToCurrency`
- Index on `FromCurrency`
- Index on `Timestamp`

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

## Troubleshooting

### Database Connection Issues

1. Verify SQL Server is running:
   ```bash
   docker ps
   ```

2. Test connection:
   ```bash
   docker exec -it sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'YourStrong@Passw0rd'
   ```

### Migration Issues

Reset database:
```bash
cd src/CurrencyExchange.Infrastructure
dotnet ef database drop --startup-project ../CurrencyExchange.API/CurrencyExchange.API.csproj
dotnet ef database update --startup-project ../CurrencyExchange.API/CurrencyExchange.API.csproj
```

### Background Job Not Running

Check logs for errors:
- Console output
- `logs/currency-exchange-{Date}.txt`

Manually trigger rate update:
```http
POST /api/currencyrates/update
Authorization: Bearer {token}
```

## Security Considerations

⚠️ **Production Checklist**:

1. **JWT Secret**: Generate a strong, random secret key (minimum 32 characters)
2. **HTTPS**: Enforce HTTPS in production
3. **Authentication**: Implement proper user management with ASP.NET Core Identity
4. **Password Hashing**: Use bcrypt or Argon2 for password hashing
5. **API Rate Limiting**: Add rate limiting middleware
6. **CORS**: Configure CORS policy for allowed origins
7. **SQL Injection**: Already protected via Entity Framework parameterization
8. **Secrets Management**: Use Azure Key Vault or AWS Secrets Manager

## Technology Stack

- **Framework**: .NET 9.0
- **ORM**: Entity Framework Core 9.0
- **Database**: SQL Server 2022
- **Background Jobs**: Quartz.NET 3.15.1
- **Authentication**: JWT Bearer Tokens
- **Logging**: Serilog 9.0
- **API Documentation**: Swagger/Swashbuckle 6.9
- **Testing**: xUnit 2.9.2, Moq 4.20.72, FluentAssertions 7.0.1

## License

This project is created for educational purposes as part of a .NET assignment.

## Author

Developed as part of a Currency Exchange API assignment demonstrating Clean Architecture, SOLID principles, and modern .NET development practices.

## Support

For issues or questions:
1. Check the logs in `logs/` directory
2. Review Swagger documentation at the root URL
3. Verify database connectivity
4. Ensure all NuGet packages are restored: `dotnet restore`
