# Build System Documentation

## Overview

This project includes a comprehensive build system that automates the complete build, test, and deployment process for the full-stack authentication application with React frontend and .NET backend.

## Available Build Scripts

### 1. PowerShell Script (Windows)
**File**: `build.ps1`

```powershell
# Run complete build pipeline
.\build.ps1

# Run specific operations
.\build.ps1 --clean --test
.\build.ps1 --docker --health
.\build.ps1 --all
```

### 2. Bash Script (Linux/macOS)
**File**: `build.sh`

```bash
# Make executable
chmod +x build.sh

# Run complete build pipeline
./build.sh

# Run specific operations
./build.sh --clean --test
./build.sh --docker --health
./build.sh --all
```

### 3. Makefile (Cross-platform)
**File**: `Makefile`

```bash
# Show available commands
make help

# Run complete build pipeline
make all

# Run specific operations
make clean build
make test docker
make health
```

## Build Pipeline Explained

### 🔧 Configuration
- **Project Root**: Automatically detected
- **Backend Directory**: `AuthBackend/`
- **Frontend Directory**: `AuthFrontend/`
- **Tests Directory**: `AuthBackend.Tests/`
- **Build Configuration**: Release
- **Node Environment**: Production

### 🧹 Cleanup Phase
**Purpose**: Remove all build artifacts and temporary files

**Backend Cleanup**:
- `dotnet clean` - Removes compiled binaries
- Deletes `bin/` and `obj/` folders
- Ensures clean build state

**Frontend Cleanup**:
- Removes `dist/` and `build/` folders
- Clears npm cache (`node_modules/.cache`)
- Ensures clean build state

**Tests Cleanup**:
- `dotnet clean` on test project
- Removes test artifacts

### 📦 Dependency Restoration
**Purpose**: Restore all project dependencies

**Backend Dependencies**:
```bash
cd AuthBackend
dotnet restore
```
- Restores NuGet packages
- Downloads required .NET packages
- Ensures all dependencies are available

**Test Dependencies**:
```bash
cd AuthBackend.Tests
dotnet restore
```
- Restores test-specific packages
- Includes testing frameworks (xUnit, Moq)

**Frontend Dependencies**:
```bash
cd AuthFrontend
npm install
```
- Downloads Node.js packages
- Installs React, Material-UI, and other dependencies

### 🧪 Testing Phase
**Purpose**: Run all automated tests

**Backend Unit Tests**:
```bash
cd AuthBackend.Tests
dotnet test --configuration Release --no-build --verbosity normal
```

**Test Categories**:
- **Unit Tests**: Test individual components in isolation
  - `AuthControllerTests.cs` - API endpoint tests
  - `AuthenticationServiceTests.cs` - Business logic tests
- **Integration Tests**: Test full API functionality
  - `AuthIntegrationTests.cs` - End-to-end API tests

**Test Frameworks**:
- **xUnit**: Testing framework
- **Moq**: Mocking framework for unit tests
- **ASP.NET Core Testing**: Integration test utilities

### 🔨 Build Phase
**Purpose**: Compile and optimize the application

**Backend Build**:
```bash
cd AuthBackend
dotnet build --configuration Release --no-restore
```
- Compiles .NET code to Release configuration
- Optimizes for production
- Creates deployment-ready binaries

**Frontend Build**:
```bash
cd AuthFrontend
NODE_ENV=production npm run build
```
- Creates optimized React bundle
- Minifies JavaScript and CSS
- Generates static assets for production

### 🐳 Docker Operations
**Purpose**: Build and start containerized services

**Docker Compose Operations**:
```bash
# Stop existing containers
docker-compose down

# Build and start new containers
docker-compose up -d --build

# Wait for services to start
sleep 10

# Check container status
docker-compose ps
```

**Services**:
- **api**: .NET backend application
- **db**: PostgreSQL database
- **Automatic Database Migrations**: Applied on startup

### 🏥 Health Checks
**Purpose**: Verify application health and accessibility

**Backend Health Check**:
```bash
curl -f http://localhost:5277
```
- Verifies backend API is accessible
- Checks if application started successfully

**Frontend Health Check**:
```bash
curl -f http://localhost:5173
```
- Verifies frontend is accessible
- Checks if React application is running

## Command Line Options

### PowerShell Script Options
- `--clean`: Run cleanup phase
- `--test`: Run testing phase
- `--docker`: Run Docker operations
- `--health`: Run health checks
- `--all`: Run all phases (default)

### Bash Script Options
Same as PowerShell script options

### Makefile Targets
- `help`: Show available commands
- `clean`: Clean build artifacts
- `restore`: Restore dependencies
- `test`: Run tests
- `build-backend`: Build .NET backend
- `build-frontend`: Build React frontend
- `build`: Build both frontend and backend
- `docker`: Build and start Docker containers
- `health`: Run health checks
- `all`: Run complete pipeline

## Development Commands

### Makefile Development Targets
- `dev-backend`: Start backend in development mode
- `dev-frontend`: Start frontend in development mode
- `dev`: Start both in parallel
- `logs`: Show Docker logs
- `stop`: Stop Docker containers
- `restart`: Restart Docker containers

## Application URLs

After successful build and Docker startup:

- **Frontend**: http://localhost:5173
- **Backend API**: http://localhost:5277
- **API Documentation**: http://localhost:5277/swagger
- **Database**: localhost:5432 (PostgreSQL)

## Docker Management

### Useful Docker Commands
```bash
# View logs
docker-compose logs -f

# View specific service logs
docker-compose logs -f api
docker-compose logs -f db

# Stop services
docker-compose down

# Restart services
docker-compose restart

# Rebuild and restart
docker-compose up -d --build

# Access database
docker-compose exec db psql -U postgres -d authdb
```

## Troubleshooting

### Common Issues

1. **Port Conflicts**:
   - Ensure ports 5173, 5277, and 5432 are available
   - Stop conflicting services if needed

2. **Docker Issues**:
   - Ensure Docker Desktop is running
   - Check Docker logs for errors

3. **Node.js Issues**:
   - Ensure Node.js 20+ is installed
   - Clear npm cache: `npm cache clean --force`

4. **.NET Issues**:
   - Ensure .NET 8.0 SDK is installed
   - Check `dotnet --version`

### Debug Mode

For debugging, you can run individual phases:

```bash
# Run just the cleanup
.\build.ps1 --clean

# Run just the tests
.\build.ps1 --test

# Run just Docker operations
.\build.ps1 --docker
```

## CI/CD Integration

These build scripts are designed to be easily integrated into CI/CD pipelines:

### GitHub Actions Example
```yaml
- name: Run Build Script
  run: ./build.ps1 --all
```

### Azure DevOps Example
```yaml
- task: PowerShell@2
  inputs:
    filePath: 'build.ps1'
    arguments: '--all'
```

## Environment Variables

The build scripts respect these environment variables:

- `BUILD_CONFIGURATION`: Release (default) or Debug
- `NODE_ENV`: production (default) or development
- `DOTNET_ENVIRONMENT`: Production or Development

## Performance Considerations

- **Parallel Builds**: Frontend and backend build in parallel where possible
- **Caching**: Dependencies are cached between builds
- **Incremental Builds**: Use `--no-build` flags where appropriate
- **Docker Layer Caching**: Optimized Dockerfile for faster rebuilds
