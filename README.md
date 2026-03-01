# Full-Stack Authentication Application

React 19 + ASP.NET Core 8.0 + PostgreSQL + Docker

## 🚀 Quick Start

**Use Git Bash to run all commands**

### Option 1: Automated Build (Recommended)
```bash
# Navigate to project directory
cd "c:\Users\monde\Full-stack-app"

# Make script executable
chmod +x build.sh

# Complete setup with one command
./build.sh
```
This runs: clean → restore dependencies → run tests → build → start Docker → health checks

### Option 2: Manual Setup
```bash
# Navigate to project directory
cd "c:\Users\monde\Full-stack-app"

# 1. Start database and backend API
docker-compose up -d --build

# 2. Install frontend dependencies
cd AuthFrontend && npm install

# 3. Start frontend development server
npm run dev
```

### Option 3: Step-by-Step Manual
```bash
# Navigate to project directory
cd "c:\Users\monde\Full-stack-app"

# 1. Clean all projects
dotnet clean

# 2. Restore .NET packages (backend dependencies)
dotnet restore AuthBackend/AuthBackend.csproj
dotnet restore AuthBackend.Tests/AuthBackend.Tests.csproj

# 3. Install Node.js packages (frontend dependencies)
cd AuthFrontend && npm install

# 4. Run tests
dotnet test AuthBackend.Tests --configuration Release

# 5. Build backend
dotnet build AuthBackend/AuthBackend.csproj --configuration Release

# 6. Build frontend
npm run build

# 7. Start services
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
```

## 🐳 Docker Services

- **API**: http://localhost:5277
- **Frontend**: http://localhost:5173  
- **Database**: localhost:5432

### Database Access Commands

```bash
# Connect to PostgreSQL database
docker-compose exec db psql -U postgres -d authdb

# View all registered users
SELECT email, "firstName", "lastName", "createdAt" FROM "AspNetUsers";

# View users table structure
\d "AspNetUsers";

# View all tables
\dt;

# Count total users
SELECT COUNT(*) FROM "AspNetUsers";

# View recent users (last 10)
SELECT email, "firstName", "lastName", "createdAt" 
FROM "AspNetUsers" 
ORDER BY "createdAt" DESC 
LIMIT 10;

# Exit database
\q
```

## 🔧 Development

**Use Git Bash for all commands**

```bash
# Backend development server
cd AuthBackend && dotnet run

# Frontend development server
cd AuthFrontend && npm run dev

# Both in parallel (requires make)
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