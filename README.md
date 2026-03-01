# Full-Stack Authentication Application

React 19 + ASP.NET Core 8.0 + PostgreSQL + Docker

## 🚀 Quick Start

### Option 1: Automated Build (Recommended)
```bash
# Complete setup with one command
make all
```
This runs: clean → restore dependencies → run tests → build → start Docker → health checks

### Option 2: Manual Setup
```bash
# 1. Start database and backend API
docker-compose up -d --build

# 2. Install frontend dependencies
cd AuthFrontend && npm install

# 3. Start frontend development server
npm run dev
```

### Option 3: Step-by-Step Manual
```bash
# 1. Restore .NET packages (backend dependencies)
dotnet restore AuthBackend/AuthBackend.csproj
dotnet restore AuthBackend.Tests/AuthBackend.Tests.csproj

# 2. Install Node.js packages (frontend dependencies)
cd AuthFrontend && npm install

# 3. Run tests
dotnet test AuthBackend.Tests

# 4. Build backend
dotnet build AuthBackend/AuthBackend.csproj

# 5. Build frontend
npm run build

# 6. Start services
docker-compose up -d --build
cd AuthFrontend && npm run dev
```

## 📡 API Endpoints

```
POST /api/auth/register    - {firstName, lastName, email, password}
POST /api/auth/login       - {email, password} → {accessToken}
GET  /api/auth/me         - Authorization: Bearer <token>
```

## 🧪 Testing

```bash
# Run all tests
dotnet test AuthBackend.Tests

# With coverage
dotnet test --collect:"XPlat Code Coverage"
```

## 🐳 Docker Services

- **API**: http://localhost:5277
- **Frontend**: http://localhost:5173  
- **Database**: localhost:5432

## 🔧 Development

```bash
# Backend
cd AuthBackend && dotnet run

# Frontend
cd AuthFrontend && npm run dev

# Both in parallel
make dev
```

## 📁 Structure

```
AuthBackend/           # .NET 8.0 API
AuthFrontend/         # React 19 SPA
AuthBackend.Tests/     # xUnit tests
docker-compose.yml     # Container orchestration
```

## 🎯 Assessment Compliance

✅ Frontend: Register/Login/UserDetails pages  
✅ Backend: C# API in Docker container  
✅ Database: PostgreSQL with Identity  
✅ Auth: JWT token protection