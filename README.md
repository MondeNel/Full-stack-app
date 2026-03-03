# Full-Stack Authentication App

React 19 · ASP.NET Core 8.0 · PostgreSQL · Docker

---

## Architecture

```
React SPA  ──▶  ASP.NET Core API  ──▶  PostgreSQL
:5173              :5277                 :5432
```

The frontend communicates with the backend via REST using Axios. The API handles all auth logic, issues JWT tokens, and protects sensitive endpoints. The API and database run together in Docker Compose. The frontend runs on a local Vite dev server in development, or builds to a static bundle served by Nginx in production.

---

## Project Structure

```
Full-stack-app/
├── AuthBackend/
│   ├── Controllers/AuthController.cs        # HTTP layer — routes and responses
│   ├── Data/AppDbContext.cs                  # EF Core DbContext (extends IdentityDbContext)
│   ├── DTOs/LoginDto.cs                      # Login request shape + validation
│   ├── DTOs/RegisterDto.cs                   # Register request shape + validation
│   ├── Models/User.cs                        # Extends IdentityUser: FirstName, LastName, CreatedAt
│   ├── Services/IAuthenticationService.cs    # Service contract
│   ├── Services/AuthService.cs               # Business logic — register, login, JWT
│   ├── Program.cs                            # Bootstrap, DI, middleware pipeline
│   └── Dockerfile                            # API container definition
├── AuthBackend.Tests/
│   ├── AuthControllerTests.cs               # Unit tests — controller HTTP responses
│   ├── AuthenticationServiceTests.cs        # Unit tests — service business logic
│   └── UnitTest1.cs                         # Baseline sanity test
├── AuthFrontend/
│   ├── src/
│   │   ├── api/auth.js                       # Axios calls — register, login, getUserDetails
│   │   ├── components/NavBar.jsx             # Top nav — Login/Register or User/Logout
│   │   ├── components/ProtectedRoute.jsx     # Redirects to /login if no token found
│   │   ├── pages/HomePage.jsx                # Landing page with Login and Register buttons
│   │   ├── pages/LoginPage.jsx               # Login form with loading state and notifications
│   │   ├── pages/Register.jsx                # Register form with loading state and notifications
│   │   └── pages/UserDetails.jsx             # Protected — displays user info from JWT
│   ├── App.jsx                               # Router setup and route definitions
│   ├── Main.jsx                              # React entry point
│   └── Dockerfile                            # Multi-stage: Vite build + Nginx serve
├── docker-compose.yml                        # Orchestrates API + DB containers
└── build.sh                                  # Automated build and startup script
```

---

## Backend

### Request Flows

**Register — `POST /api/auth/register`**
1. Controller validates `RegisterDto` — required fields, valid email format, password min 6 chars
2. `AuthService.RegisterAsync()` checks for an existing user by email via `UserManager`
3. If unique, `UserManager.CreateAsync()` persists the user — Identity hashes the password automatically (PBKDF2)
4. Returns `200 OK` or `400 Bad Request` with error details

**Login — `POST /api/auth/login`**
1. Controller validates `LoginDto`
2. `AuthService.LoginAsync()` finds the user by email and verifies via `UserManager.CheckPasswordAsync()`
3. On success, builds a claims list — `NameIdentifier`, `Email`, `FirstName`, `LastName`
4. `GenerateJwtToken()` signs the token using HMAC-SHA256, valid for 3 hours
5. Returns `{ accessToken }` or `401 Unauthorized`

**Get current user — `GET /api/auth/me`**
1. `[Authorize]` blocks unauthenticated requests before the method is reached
2. User details are read directly from the validated JWT claims — no extra database call
3. Returns `id`, `email`, `firstName`, `lastName`

### Key Design Decisions

- **ASP.NET Identity for user management** — handles password hashing, storage, and validation. `User` extends `IdentityUser` to add `FirstName`, `LastName`, and `CreatedAt` without reinventing the wheel.
- **`AppDbContext` extends `IdentityDbContext`** — wires Identity's built-in tables into the same EF Core context, keeping migrations and schema setup unified.
- **JWT claims carry user data** — `FirstName` and `LastName` are embedded as custom claims at login time. This means `/api/auth/me` reads from the token directly, avoiding a database round-trip on every authenticated request.
- **Interface-driven service layer** — `AuthService` is registered against `IAuthenticationService`, making it easy to mock in tests and swap implementations without touching the controller.
- **Environment-aware database** — `Program.cs` switches between PostgreSQL (production) and an in-memory database (`Testing` environment), so integration tests run without a live database.

---

## Frontend

### Pages and Routing

| Route       | Component     | Protected | Description                               |
|-------------|---------------|-----------|-------------------------------------------|
| `/`         | `HomePage`    | No        | Landing page with Login and Register CTAs |
| `/register` | `Register`    | No        | Registration form                         |
| `/login`    | `LoginPage`   | No        | Login form                                |
| `/user`     | `UserDetails` | Yes       | Displays authenticated user profile       |

### How Protection Works

`ProtectedRoute` wraps the `/user` route in `App.jsx`. It reads the token from `localStorage` — if none is found, React Router redirects to `/login` before anything renders. The `UserDetails` component never loads for unauthenticated users.

`NavBar` also reads from `localStorage` to show the right links — unauthenticated users see Login and Register, authenticated users see User and Logout. Logout clears the token and redirects to `/login`.

### Auth Flow on the Frontend

```
Register form submitted
  ──▶ POST /api/auth/register via auth.js
  ──▶ Success notification shown
  ──▶ Redirects to /login after 2 seconds

Login form submitted
  ──▶ POST /api/auth/login via auth.js
  ──▶ accessToken saved to localStorage
  ──▶ Redirects to /user after 1 second

/user route loaded
  ──▶ ProtectedRoute checks localStorage for token
  ──▶ GET /api/auth/me with Authorization: Bearer <token>
  ──▶ firstName, lastName, email displayed

Logout clicked
  ──▶ Token removed from localStorage
  ──▶ Redirected to /login
```

### Key Design Decisions

- **Axios in a dedicated `api/auth.js` module** — all API calls live in one place. Pages import named functions, not raw Axios calls. This keeps components clean and makes it trivial to change the base URL or add interceptors later.
- **Loading states and Snackbar notifications** — both Login and Register show a spinner while the request is in flight and display success or error feedback via MUI `Snackbar` and `Alert`, giving the user clear feedback at every step.
- **Multi-stage Dockerfile** — Stage 1 uses Node to run `npm ci` and `npm run build`, producing a static `dist/` bundle. Stage 2 copies only that bundle into a lightweight Nginx alpine image. The final container has no Node.js in it, keeping the image small and production-ready.

---

## How Auth Works End-to-End

```
Register ──▶ Identity hashes password ──▶ saved to PostgreSQL

Login    ──▶ password verified
         ──▶ JWT issued (3hr expiry, signed HMAC-SHA256)
              contains: userId, email, firstName, lastName

Authenticated request:
  Header: Authorization: Bearer <token>
  ──▶ JwtBearer middleware validates signature, issuer, audience
  ──▶ Claims extracted, user identity established
  ──▶ [Authorize] endpoints proceed
  ──▶ No valid token → 401 Unauthorized
```

---

## API Reference

| Method | Endpoint             | Auth | Body                                   | Response                         |
|--------|----------------------|------|----------------------------------------|----------------------------------|
| POST   | `/api/auth/register` | No   | `firstName, lastName, email, password` | `200` or `400` + error message   |
| POST   | `/api/auth/login`    | No   | `email, password`                      | `{ accessToken }` or `401`       |
| GET    | `/api/auth/me`       | Yes  | —                                      | `id, email, firstName, lastName` |
| GET    | `/health`            | No   | —                                      | `{ status: "healthy" }`          |

**Validation enforced via DTOs:**
- Email must be valid format on both register and login
- Password minimum 6 characters on register
- First name and last name required on register

---

## Tests

```bash
dotnet test AuthBackend.Tests
```

### What Is Tested

**`AuthControllerTests`** — unit tests for the HTTP layer using a mocked `IAuthenticationService`:
- `Register` returns `200 OK` on success
- `Register` returns `400 Bad Request` when user already exists
- `Login` returns `200 OK` with token on valid credentials
- `Login` returns `401 Unauthorized` on invalid credentials

**`AuthenticationServiceTests`** — unit tests for business logic using a mocked `UserManager`:
- `RegisterAsync` throws `InvalidOperationException` if user already exists
- `RegisterAsync` calls `CreateAsync` with correct `FirstName` and `LastName`
- `RegisterAsync` throws `InvalidOperationException` if Identity `CreateAsync` fails
- `LoginAsync` throws `UnauthorizedAccessException` if user is not found
- `LoginAsync` throws `UnauthorizedAccessException` if password is wrong
- `LoginAsync` returns a non-empty JWT string on valid credentials

**`UnitTest1`** — baseline sanity check confirming the test runner is wired correctly.

---

## Quick Start

> Use **Git Bash** for all commands.

**Recommended — automated:**
```bash
chmod +x build.sh && ./build.sh
```
Runs: clean → restore → test → build → Docker → health checks.

**Manual:**
```bash
docker-compose up -d --build
cd AuthFrontend && npm install && npm run dev
```

Open `http://localhost:5173`.

---

## Database Access

```bash
docker-compose exec db psql -U postgres -d authdb
```

```sql
-- View all registered users
SELECT "Email", "FirstName", "LastName", "CreatedAt" FROM "AspNetUsers";

-- Count total users
SELECT COUNT(*) FROM "AspNetUsers";

-- Exit
\q
```

---

## What I Would Improve Next

- Global exception handler middleware for consistent API error response shapes
- Store JWT in an `HttpOnly` cookie instead of `localStorage` to reduce XSS exposure
- Password strength validation beyond minimum length
- Replace the `alert()` in `UserDetails` with a proper error notification matching the rest of the UI

---

## Assessment Checklist

| Requirement                          | Status |
|--------------------------------------|--------|
| Register / Login / User Detail pages | ✅     |
| C# API in Docker                     | ✅     |
| PostgreSQL via Docker                | ✅     |
| JWT-protected endpoint               | ✅     |
| Unit tests                           | ✅     |
| Integration tests _(bonus)_          | ✅     |
| Build script _(bonus)_               | ✅     |
