# RecipeApp - AI Agent Instructions

## Project Overview
RecipeApp is an **ASP.NET Core 9.0 REST API** with a **layered N-tier architecture**. It's a recipe management system with real-time chat, user authentication, and advanced recipe search features.

## Architecture

```
RecipeApp (Web API - Controllers, Hubs, wwwroot)
  ↓
RecipeApp.Services (Business Logic - Services, Validators, Mapping)
  ↓
RecipeApp.Repository (Data Abstraction - Repositories, Entities)
  ↓
RecipeApp.DataContext (ORM & Migrations - Entity Framework Core)
  ↓
SQL Server
  ↑
RecipeApp.Common (Shared - DTOs, Enums)
```

### Layer Responsibilities
- **RecipeApp**: HTTP controllers, SignalR hub for chat, CORS policy for React (`localhost:5173`, `localhost:3000`)
- **Services**: Business logic, validation, AutoMapper DTO↔Entity mapping, cross-entity operations
- **Repository**: Abstract data access with generic `IRepository<T>` (GetAll, GetById, AddItem, UpdateItem, DeleteItem)
- **DataContext**: EF Core models, `RecipeDbContext`, migrations with 4 versions
- **Common**: DTOs (Create/Update/Read variants), enums (RecipeCategory, UserActionType, IngredientImportance)

## Core Technologies
| Tech | Version | Role |
|------|---------|------|
| ASP.NET Core | 9.0 | Web framework |
| Entity Framework Core | 9.0 | ORM, migrations |
| SQL Server | Local | Database (SQLEXPRESS) |
| SignalR | 9.0 | Real-time chat |
| JWT | Built-in | Authentication |
| AutoMapper | 12.0.1 | DTO mapping |
| FluentValidation | 12.1.1 | Input validation |
| Swagger | 9.0.6 | API documentation |

## Essential Commands

```bash
# Build & Run
dotnet build
dotnet run --project RecipeApp
dotnet watch run --project RecipeApp          # Auto-reload on changes

# Database Migrations
dotnet ef migrations add MigrationName -p RecipeApp.DataContext
dotnet ef database update -p RecipeApp.DataContext

# View API
https://localhost:5001/swagger
```

## Key Conventions & Patterns

### Adding a New Feature
1. **Create DTO** in `RecipeApp.Common/DTOs/` with validator in `RecipeApp.Services/Validators/`
2. **Create Repository** in `RecipeApp.Repository/Repositories/` if new entity
3. **Create Service** in `RecipeApp.Services/Services/` with business logic
4. **Create Controller** in `RecipeApp/Controllers/` with `[Route("api/[controller]")]` and error handling
5. **Add AutoMapper profile** in `RecipeApp.Services/Mapping/MappingProfile.cs`
6. **Register in DI** in `Program.cs`

### Repository Pattern
- All data access goes through `IRepository<T>` methods
- Use `.Include()` for eager loading relationships
- Bulk updates use `ExecuteUpdateAsync()` for performance
- Bulk deletes use `ExecuteDeleteAsync()`

### Service Layer
- Inject repositories and other services
- Handle validation (call validators before DB operations)
- Map DTOs using AutoMapper: `_mapper.Map<RecipeDto>(recipe)`
- Return DTOs from public methods, not entities

### Controllers
- Attribute routing: `[Route("api/[controller]")]`
- Authorization: `[Authorize]` or `[Authorize(Roles = "Admin")]`
- Consistent error handling with try-catch returning `BadRequest()`, `NotFound()`, `Ok()`
- Return DTOs, not entities

## Domain Model Quick Reference

**User**: Email (unique), Name, Phone, Password (hashed), CreatedAt → UserActions

**Recipe**: Name, Description, Instructions, Category, Level (1-5), PrepTime, TotalTime, ImageUrl, Tags (JSON array) → RecipeIngredients, UserActions

**Ingredient**: Name (unique) ↔ Conversions, RecipeIngredients

**RecipeIngredient**: Recipe + Ingredient + Quantity + Unit + Importance (Essential/Important/Optional) — **cascade delete on recipe**

**UserAction**: User + Recipe + ActionType (View/Rate/Book) + Timestamp — **unique constraint on (UserId, RecipeId) for bookmarks**

**Conversion**: Links two ingredients with a conversion factor for unit conversion

See [Migrations](RecipeApp.DataContext/Migrations/) for latest schema.

## Configuration

**appsettings.json** contains:
- **JWT**: `Jwt:Key` (min 32 chars), Issuer, Audience
- **Database**: `ConnectionStrings:DefaultConnection` (SQL Server SQLEXPRESS)
- **Email**: SMTP settings (Gmail app password, not account password)
- **Gemini API**: Key for AI features
- **Admin Email**: Default admin account setup

**CORS**: `AllowReactApp` policy restricted to React dev ports

**Environment**: Swagger enabled in Development only; HTTPS always enforced

## Important Gotchas

1. **Database**: Requires SQL Server SQLEXPRESS. Update connection string if different instance.
2. **JWT Secret**: Must be ≥32 characters for HS256 algorithm
3. **Email**: Uses Gmail app-specific password, not account password
4. **SignalR**: Chat hub connects at `/hubs/chat` - configure client appropriately
5. **Cascade Deletes**: Recipe deletion cascades to RecipeIngredients; Conversions use Restrict to prevent orphans
6. **Entity Tracking**: `ExecuteDeleteAsync()` / `ExecuteUpdateAsync()` bypass change tracking—use for bulk operations only
7. **Tags**: Stored as JSON string in Recipe.Tags—serialize/deserialize carefully
8. **Unique Indexes**: Email (Users), Ingredient.Name, UserId+RecipeId (UserActions bookmarks only)

## Code Navigation

- **Endpoints**: [Controllers/](RecipeApp/Controllers/)
- **Business Logic**: [Services/](RecipeApp.Services/Services/)
- **Data Schemas**: [Migrations/](RecipeApp.DataContext/Migrations/)
- **DTOs & Contracts**: [Common/DTOs/](RecipeApp.Common/DTOs/)
- **DI & Pipeline**: [Program.cs](RecipeApp/Program.cs)
- **Swagger API Docs**: `https://localhost:5001/swagger` (when running)

## Developer Tips

- Use `.http` files in [RecipeApp.http](RecipeApp/RecipeApp.http) to test endpoints
- View migrations to understand schema evolution
- FluentValidation automatically validates DTOs via middleware
- Add Hebrew or English comments as needed—team uses both
- Test in Swagger UI before integrating with frontend

---

**Last Updated**: May 2026 | **Framework**: .NET 9.0 | **Architecture**: Clean Layered N-Tier
