# 🍰 RecipeApp Server

שרת ה-**Backend** של אפליקציית שיתוף המתכונים — **ASP.NET Core 9.0 Web API** בארכיטקטורת שכבות (Layered N-Tier), עם אימות JWT, צ'אט AI בזמן אמת דרך SignalR, וחיפוש מתכונים מתקדם.

🔗 **קליינט (Frontend):** [RecipeApp-Client](https://github.com/TEHILA5/RecipeApp-Client) — [דמו חי](https://recipe-app-client-eta.vercel.app)

---

## 🏗️ ארכיטקטורה

הפרויקט בנוי כ-**Clean Layered N-Tier Architecture** עם הפרדה ברורה בין שכבות:

```
RecipeApp                 → Web API: Controllers, SignalR Hub, wwwroot
      ↓
RecipeApp.Services         → לוגיקה עסקית: Services, Validators, AutoMapper
      ↓
RecipeApp.Repository        → הפשטת גישה לנתונים: Repositories, Entities
      ↓
RecipeApp.DataContext         → EF Core: DbContext, Migrations
      ↓
SQL Server (dev) / PostgreSQL (prod)
      ↑
RecipeApp.Common            → משותף: DTOs, Enums
```

| שכבה | אחריות |
|---|---|
| **RecipeApp** | קונטרולרים HTTP, SignalR Hub לצ'אט, מדיניות CORS לקליינט React |
| **Services** | לוגיקה עסקית, ולידציה, מיפוי DTO↔Entity עם AutoMapper |
| **Repository** | גישה גנרית לנתונים דרך `IRepository<T>` (GetAll, GetById, AddItem, UpdateItem, DeleteItem) |
| **DataContext** | מודלים של EF Core, `RecipeDbContext`, Migrations |
| **Common** | DTOs (וריאנטים Create/Update/Read), Enums (RecipeCategory, UserActionType, IngredientImportance) |

---

## 🧱 מחסנית טכנולוגית

| טכנולוגיה | גרסה | תפקיד |
|---|---|---|
| ASP.NET Core | 9.0 | Web framework |
| Entity Framework Core | 9.0 | ORM + Migrations |
| SQL Server / PostgreSQL (Npgsql) | — | בסיס נתונים (SQL Server בפיתוח, PostgreSQL בייצור) |
| SignalR | 9.0 | צ'אט בזמן אמת |
| JWT Bearer | מובנה | אימות |
| AutoMapper | 12.0.1 | מיפוי DTO↔Entity |
| FluentValidation | 12.1.1 | ולידציית קלט |
| Swashbuckle (Swagger) | 9.0.6 | תיעוד API |

---

## 📁 מבנה הפרויקט

```
RecipeApp/                     # Web API
├── Controllers/                 # UserController, RecipeController, IngredientController...
├── Hubs/ChatHub.cs               # SignalR Hub לצ'אט
└── Program.cs                     # הגדרת ה-DI, Middleware ו-Pipeline

RecipeApp.Services/             # לוגיקה עסקית
├── Services/                     # AuthService, RecipeService, ChatService...
├── Validators/                    # FluentValidation validators
└── Mapping/MappingProfile.cs       # פרופיל AutoMapper

RecipeApp.Repository/           # גישה לנתונים
├── Entities/                     # User, Recipe, Ingredient, RecipeIngredient, UserAction, Conversion
└── Repositories/                  # מימושי IRepository<T> לכל ישות

RecipeApp.DataContext/          # EF Core
├── RecipeDbContext.cs
└── Migrations/                     # היסטוריית סכימה (5 migrations, כולל מעבר ל-Postgres)

RecipeApp.Common/                # משותף
└── DTOs/                            # UserDto, RecipeDto, IngredientDto, ChatDtos...
```

---

## 🚀 התקנה והרצה מקומית

### דרישות מקדימות
- .NET SDK 9.0
- SQL Server (LocalDB / SQLEXPRESS) לפיתוח מקומי
- (אופציונלי) Docker

### שלבים

```bash
git clone https://github.com/TEHILA5/RecipeApp-Server.git
cd RecipeApp-Server
```

עדכנו את `appsettings.Development.json` עם:
- `ConnectionStrings:DefaultConnection` — מחרוזת חיבור ל-SQL Server
- `Jwt:Key` (מינימום 32 תווים), `Jwt:Issuer`, `Jwt:Audience`
- הגדרות SMTP לשליחת מיילים (Gmail App Password)
- מפתח Gemini API לתכונות ה-AI

```bash
# שחזור חבילות ובנייה
dotnet build

# הרצת migrations ליצירת בסיס הנתונים
dotnet ef database update -p RecipeApp.DataContext

# הרצת השרת
dotnet run --project RecipeApp
# או עם auto-reload:
dotnet watch run --project RecipeApp
```

השרת יעלה בכתובת `https://localhost:5001`, ותיעוד ה-API הזמין דרך Swagger: `https://localhost:5001/swagger`

### הרצה עם Docker

הפרויקט כולל `Dockerfile` מבוסס `.NET 9.0` (SDK לבנייה, ASP.NET לריצה), חושף פורט `8080`:

```bash
docker build -t recipeapp-server .
docker run -p 8080:8080 recipeapp-server
```

---

## 📡 סקירת ה-API העיקרי

| Controller | נתיב בסיס | עיקרי הפעולות |
|---|---|---|
| `UserController` | `/api/user` | הרשמה, התחברות, איפוס סיסמה, פרופיל אישי, ניהול משתמשים (Admin) |
| `RecipeController` | `/api/recipe` | CRUD מתכונים, חיפוש לפי קטגוריה/רכיבים/תגיות, מתכונים מומלצים |
| `IngredientController` | `/api/ingredient` | CRUD רכיבים, חיפוש לפי שם |
| `ConversionController` | `/api/conversion` | ניהול יחסי המרה בין רכיבים |
| `SearchController` | `/api/search` | ניתוח טקסט חופשי וחיפוש מתקדם (מבוסס AI) |
| `UserActionController` | `/api/useraction` | תגובות, שמירת מתכונים, היסטוריה, העדפות אישיות |
| `ChatController` / `ChatHub` | `/api/chat`, `/hubs/chat` | צ'אט AI (REST + SignalR בזמן אמת) |
| `ContactController` | `/api/contact` | טופס יצירת קשר ומענה מנהל |
| `NewsletterController` | `/api/newsletter` | הרשמה לניוזלטר |

> כל הקונטרולרים משתמשים ב-`[Route("api/[controller]")]`, מחזירים DTOs (לא Entities), ומוגנים לפי הצורך עם `[Authorize]` / `[Authorize(Roles = "Admin")]`.

---

## 🗄️ מודל הנתונים (עיקרי)

- **User** — Email (ייחודי), שם, טלפון, סיסמה מוצפנת → UserActions
- **Recipe** — שם, תיאור, הוראות הכנה, קטגוריה, רמת קושי (1–5), זמני הכנה, תמונה, תגיות (JSON) → RecipeIngredients, UserActions
- **Ingredient** — שם (ייחודי) ↔ Conversions, RecipeIngredients
- **RecipeIngredient** — Recipe + Ingredient + כמות + יחידה + חשיבות (חיוני/מומלץ/אופציונלי), עם מחיקה מדורגת (cascade) יחד עם המתכון
- **UserAction** — משתמש + מתכון + סוג פעולה (צפייה/דירוג/שמירה) + חותמת זמן, עם אילוץ ייחודיות על (UserId, RecipeId) לשמירות
- **Conversion** — קישור בין שני רכיבים עם יחס המרה

לפירוט מלא ראו את קבצי ה-[Migrations](RecipeApp.DataContext/Migrations/).

---

## ⚠️ נקודות חשובות

- מפתח JWT (`Jwt:Key`) חייב להיות **32 תווים לפחות** עבור אלגוריתם HS256.
- אימייל נשלח דרך **סיסמת אפליקציה של Gmail**, לא סיסמת החשבון הרגילה.
- Hub הצ'אט מחובר בנתיב `/hubs/chat` — יש להגדיר את הקליינט בהתאם.
- מחיקת מתכון גוררת מחיקה מדורגת (cascade) של רכיבי המתכון; המרות (Conversions) מוגנות מפני מחיקה (Restrict) כדי למנוע רשומות יתומות.
- פעולות עדכון/מחיקה בכמות גדולה (`ExecuteUpdateAsync`/`ExecuteDeleteAsync`) עוקפות את מנגנון ה-Change Tracking — מיועדות לפעולות bulk בלבד.
- Swagger פעיל בסביבת Development בלבד; HTTPS נאכף תמיד.
- ה-CORS מוגבל לכתובות הקליינט: `localhost:5173`, `localhost:3000`, ו-`recipe-app-client-eta.vercel.app`.

לפירוט טכני מלא (דפוסי קוד, הוספת פיצ'ר חדש, טיפים למפתחים) ראו את [`AGENTS.md`](AGENTS.md) בריפו.

---

## 🌐 סביבת ייצור

בייצור השרת עובר אוטומטית ל-**PostgreSQL** (במקום SQL Server) דרך אותה מחרוזת חיבור, בזכות זיהוי סביבה ב-`Program.cs`.
