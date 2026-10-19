# Translators System Standard

## Agent operating rules

- This file is the source of truth for architecture and operations. Read it before changing the project.
- The user expects implementation, not only advice.
- Whenever architecture, endpoints, auth, database, deploy, or hosting behavior changes, update this file in the same turn so later sessions do not lose the design.
- Production credentials are in the gitignored `secrets/` folder. Use those files when the user asks to deploy or connect to production. Never put secret values in scripts, source, or git.

## 1. Folder and naming rules

- All feature folders use plural names such as `Users`, `Orders`, `Payments`.
- Domain entities are inside the feature folder and use the suffix `Entity`.
- Example: `Translators.Domain/Users/UserEntity.cs`
- All primary keys must be `Guid`.

## 2. Layer conventions

### Domain
- Keep business entities and domain rules.
- Entity names: `UserEntity`, `RoleEntity`, etc.
- Default built-in roles: `Admin` (full access, `Permissions = "*"`) and `User` (read-only access: `GetByIdUserHandler,PaginatedUserHandler,LookupUserHandler`).
- User entities must include `RoleId` and a navigation to `RoleEntity` to support user-role queries and JWT role claims.
- Files are placed under `Translators.Domain/<FeatureName>/`

### Application
- Keep business logic and FastEndpoints handlers.
- Handler file names use the suffix `Handler`.
- Examples:
  - `Translators.Application/Users/LoginHandler.cs`
  - `Translators.Application/Users/RegisterUserHandler.cs`
  - `Translators.Application/Health/HealthHandler.cs`
  - `Translators.Application/Security/JwtTokenService.cs`

### Application Contracts
- Keep request/response DTOs for API boundaries.
- Contract suffixes:
  - `RequestContract` for incoming API contract
  - `ResponseContract` for outgoing API contract
- Examples:
  - `Translators.Application.Contracts/Users/LoginUserRequestContract.cs`
  - `Translators.Application.Contracts/Users/LoginUserResponseContract.cs`
  - `Translators.Application.Contracts/Health/HealthResponseContract.cs`
  - `Translators.Application.Contracts/Security/JwtOptions.cs`

### EntityFrameworkCore
- Keep DbContext and migrations under `Translators.EntityFrameworkCore/`.
- DbContext example: `Translators.EntityFrameworkCore/DbContexts/TranslatorDbContext.cs`
- Migrations live in `Translators.EntityFrameworkCore/Migrations/`.

### HttpApi
- Keep Swagger-facing API endpoints and fast endpoints.
- Endpoint file names use the suffix `Api`.
- Examples:
  - `Translators.HttpApi/Users/LoginApi.cs`
  - `Translators.HttpApi/Users/RegisterApi.cs`
  - `Translators.HttpApi/Health/HealthApi.cs`

### Tests
- Feature tests are stored under `Translators.Tests/ApiTests/<FeatureName>/`.
- Example:
  - `Translators.Tests/ApiTests/Users/LoginTests.cs`
  - `Translators.Tests/ApiTests/Users/RegisterTests.cs`
  - `Translators.Tests/ApiTests/Health/HealthTests.cs`

## 3. API contract naming prefixes and routing standards

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

### General API standard for the whole system
- Use action-style route names in PascalCase for entity operations, for example: `/api/users/GetById`, `/api/users/Paginated`, `/api/users/Lookup`.
- `GetById` endpoints must use `GET`, and query-string parameters must be passed directly; never require a request body for single-row fetches by id.
- `Paginated` and `Lookup` endpoints must use `POST` and accept a body contract; they are not `GET` endpoints.
- Named operations should be explicit and deterministic so Swagger and client code remain predictable.
- For single-entity fetches by id, the parameter name should be the primitive id name such as `guid` for `Guid` ids.
- For all features, keep the route pattern as `/api/{FeatureName}/{ActionName}` and avoid unstructured route names.

## 4. PostgreSQL configuration standard

- Connection string must be stored in `appsettings.json` or environment variables for local/dev.
- Production connection string is in `secrets/db.connection` and is written to the server as `appsettings.Production.json` by the publish script.
- Default PostgreSQL connection naming: `DefaultConnection`.
- Provider: `Npgsql.EntityFrameworkCore.PostgreSQL`.
- Example local value:

  Host=localhost;Database=translators;Username=postgres;Password=postgres;Port=5432

## 5. Nullable and coding style

- Disable nullable reference types in all projects.
- Prefer `Guid` for IDs and `DateTime.UtcNow` for timestamps.
- Use plural folder names for feature grouping.

## 6. Authentication and Swagger

- Login (`POST /api/users/login`) is anonymous and returns a JWT in `LoginUserResponseContract.Token`.
- Register (`POST /api/users/register`) is anonymous.
- Built-in role checks are modeled by `RoleEntity.HasAccess(string apiHandlerName)` and default permissions are assigned on registration.
- Admin role automatically has access to every API (`Permissions = "*"`).
- Standard user role is read-only by default and can access `GetByIdUserHandler`, `PaginatedUserHandler`, and `LookupUserHandler`.
- Protected endpoints use the Bearer JWT scheme (`AuthSchemes("Bearer")`).
- Health (`GET /api/health`) requires a valid JWT and returns the authenticated user when the token is valid.
- JWT options: `Jwt:Issuer`, `Jwt:Audience`, `Jwt:SigningKey`, `Jwt:ExpirationMinutes`.
- Local signing key lives in `appsettings.json`. Production signing key lives in `secrets/jwt.signing-key`.
- Host wires `AddAuthentication().AddJwtBearer()`, `UseAuthentication()`, then `UseAuthorization()`.
- Swagger (development and production) has a Bearer authorize button. Paste the login token there to call protected endpoints.

## 7. Database migrations

- Do not use `EnsureCreated()` for runtime schema.
- On every non-Testing startup, Host applies pending EF Core migrations with `Database.Migrate()`.
- Production therefore receives new migrations automatically after publish and process start.

## 8. Production deploy

- Repo `nuget.config` limits restore to nuget.org only. Without it, a slow machine-wide source (for example `I:\nuget\packages`) can make `dotnet restore -r linux-x64` look stuck for many minutes on `Determining projects to restore...`.
- Publish script: `scripts/Publish-Production.ps1`.
- Double-click helper: `scripts/Publish-Production.bat` runs the PowerShell script, keeps the window open (`-NoExit`), and leaves the PowerShell log visible.
- The script must read credentials from `secrets/` files. Secret values must never be hardcoded in the `.ps1`.
- Required secret files:
  - `secrets/ssh.host`
  - `secrets/ssh.user`
  - `secrets/ssh.password`
  - `secrets/remote.path`
  - `secrets/service.name`
  - `secrets/db.connection`
  - `secrets/jwt.issuer`
  - `secrets/jwt.audience`
  - `secrets/jwt.signing-key`
  - `secrets/jwt.expiration-minutes`
- The original notes file `secrets/server.txt` may still exist; structured files above are what the script reads.
- Publish output is self-contained `linux-x64` for `Translators.Host`, uploaded over SSH/SCP, then the systemd service is restarted.
- Remote shell steps run via `base64 | bash` so Windows CRLF line endings do not break bash/heredoc on Linux.
- File upload uses Posh-SSH SFTP (`Set-SFTPItem`). SCP on Windows concatenates `/tmp` with `C:\...` and fails.
- Default remote path: `/opt/translators`
- Default systemd unit: `translators.api`
- App listens on `http://0.0.0.0:5000` (Swagger: `http://<server-ip>:5000/swagger`). There is no HTTP listener on port 80.
- An older unit `translators.service` used `/usr/bin/dotnet /root/translators-app/Translators.Host.dll` on the same port 5000. Publish must stop/disable that unit and free port 5000 before starting `translators.api`, otherwise the new process ABRTs with address-already-in-use and the browser keeps serving the old app.

## 9. Panel (Blazor WebAssembly)

- `Translators.Panel` talks to the API **only** through the NSwag generated client in `Translators.Panel/Connected Services/Translators.GeneratedServices/OpenAPI.cs`.
- Never hand-write API calls in the panel (no `HttpClient`, `PostAsJsonAsync`, `ReadFromJsonAsync`, or per-endpoint wrapper services). Pages inject the generated `Client` and call its `Api...Async` methods.
- The panel must not reference `Translators.Application.Contracts` (or any other server-side project). DTOs come from the generated client; using server contracts is what allowed the hand-written client to exist.
- Generated namespace is `Translators.GeneratedServices`, not `Translators.Panel.Translators.GeneratedServices`. Nesting it under `Translators.Panel` creates a `Translators.Panel.Translators` namespace that shadows the root `Translators` namespace and breaks every `Translators.Panel.*` using directive in the project.
- Generation is part of the build (`OpenApiReference` item in `Translators.Panel.csproj`, generator `NSwagCSharp` from `NSwag.ApiDescription.Client`):
  - Target `RefreshPanelOpenApiSpec` downloads the live spec from `$(PanelOpenApiUrl)` (default `http://localhost:5000/swagger/v1/swagger.json`) before generation, so the panel client always matches the running API. The API must be reachable, otherwise the panel build fails.
  - Offline build from the checked-in spec: `dotnet build -p:PanelOpenApiUrl=`.
  - `Translators.Panel/OpenAPIs/swagger.json` is the checked-in copy of that spec and is refreshed automatically; it is only replaced when the API surface actually changed, so unchanged builds do not regenerate and recompile the panel.
  - Do not regenerate through the Unchase Visual Studio extension (`ConnectedService.json`, `OpenAPI.nswag`); those files are kept only as a reference and would reset the namespace.
- Auth in the panel: `AuthSession` holds the generated `LoginUserResponseContract`; `AuthenticationHeaderHandler` (a `DelegatingHandler`) adds the bearer token and signs the session out on 401.
- Request contracts that are bound from the query string must carry the FastEndpoints binding attribute (for example `[QueryParam]` on `GetByIdUserRequestContract.Guid`), otherwise Swagger documents a request body for a GET and the generated client method is unusable.

## 10. Operational rule

- PostgreSQL is configured in code for production; tests can override the database with InMemory for reliable execution without a local database service.
- Swagger is enabled for endpoint testing in development and production.
- The `secrets/` folder is gitignored and must stay that way.
