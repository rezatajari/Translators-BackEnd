# Translators System Standard

## 1. Folder and naming rules

- All feature folders use plural names such as `Users`, `Orders`, `Payments`.
- Domain entities are inside the feature folder and use the suffix `Entity`.
- Example: `Translators.Domain/Users/UserEntity.cs`
- All primary keys must be `Guid`.

## 2. Layer conventions

### Domain
- Keep business entities and domain rules.
- Entity names: `UserEntity`, `RoleEntity`, etc.
- Files are placed under `Translators.Domain/<FeatureName>/`

### Application
- Keep business logic and FastEndpoints handlers.
- Handler file names use the suffix `Handler`.
- Examples:
  - `Translators.Application/Users/LoginHandler.cs`
  - `Translators.Application/Users/RegisterUserHandler.cs`

### Application Contracts
- Keep request/response DTOs for API boundaries.
- Contract suffixes:
  - `RequestContract` for incoming API contract
  - `ResponseContract` for outgoing API contract
- Examples:
  - `Translators.Application.Contracts/Users/LoginUserRequestContract.cs`
  - `Translators.Application.Contracts/Users/LoginUserResponseContract.cs`

### EntityFrameworkCore
- Keep DbContext and migrations under `Translators.EntityFrameworkCore/DbContexts/`.
- Example: `Translators.EntityFrameworkCore/DbContexts/TranslatorDbContext.cs`

### HttpApi
- Keep Swagger-facing API endpoints and fast endpoints.
- Endpoint file names use the suffix `Api`.
- Examples:
  - `Translators.HttpApi/Users/LoginApi.cs`
  - `Translators.HttpApi/Users/RegisterApi.cs`

### Tests
- Feature tests are stored under `Translators.Tests/ApiTests/<FeatureName>/`.
- Example:
  - `Translators.Tests/ApiTests/Users/LoginTests.cs`
  - `Translators.Tests/ApiTests/Users/RegisterTests.cs`

## 3. API contract naming prefixes

Use these prefixes for contracts when applicable:

- Create
- Update
- Delete
- Paginated
- Lookup

Examples:
- `CreateUserRequestContract`
- `UpdateUserRequestContract`
- `PaginatedUsersResponseContract`

## 4. PostgreSQL configuration standard

- Connection string must be stored in `appsettings.json` or environment variables.
- Default PostgreSQL connection naming: `DefaultConnection`.
- Provider: `Npgsql.EntityFrameworkCore.PostgreSQL`.
- Example:

  Host=localhost;Database=translators;Username=postgres;Password=postgres;Port=5432

## 5. Nullable and coding style

- Disable nullable reference types in all projects.
- Prefer `Guid` for IDs and `DateTime.UtcNow` for timestamps.
- Use plural folder names for feature grouping.

## 6. Operational rule

- PostgreSQL is configured in code for production; tests can override the database with InMemory for reliable execution without a local database service.
- Swagger is enabled for endpoint testing in development mode.
