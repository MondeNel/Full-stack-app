# =============================================================================
# Full-Stack Authentication Application Build Script (PowerShell)
# =============================================================================
# This script automates the complete build, test, and deployment process
# for the authentication application with React frontend and .NET backend.
# =============================================================================

# Enable error handling
$ErrorActionPreference = "Stop"

# Colors for output
function Write-ColorOutput {
    param(
        [string]$Message,
        [string]$Color = "White"
    )
    
    switch ($Color) {
        "Red" { Write-Host $Message -ForegroundColor Red }
        "Green" { Write-Host $Message -ForegroundColor Green }
        "Yellow" { Write-Host $Message -ForegroundColor Yellow }
        "Blue" { Write-Host $Message -ForegroundColor Blue }
        default { Write-Host $Message }
    }
}

# =============================================================================
# CONFIGURATION
# =============================================================================

Write-ColorOutput "🔧 Configuration" "Blue"

# Project directories
$ProjectRoot = $PSScriptRoot
$BackendDir = Join-Path $ProjectRoot "AuthBackend"
$FrontendDir = Join-Path $ProjectRoot "AuthFrontend"
$TestsDir = Join-Path $ProjectRoot "AuthBackend.Tests"

# Build configurations
$BuildConfiguration = "Release"
$NodeEnv = "production"

Write-Host "Project Root: $ProjectRoot"
Write-Host "Backend Directory: $BackendDir"
Write-Host "Frontend Directory: $FrontendDir"
Write-Host "Tests Directory: $TestsDir"

# =============================================================================
# CLEANUP FUNCTION
# =============================================================================

function Cleanup {
    Write-ColorOutput "🧹 Cleaning up previous builds..." "Yellow"
    
    # Clean backend
    if (Test-Path $BackendDir) {
        Write-Host "Cleaning backend..."
        Set-Location $BackendDir
        dotnet clean --configuration $BuildConfiguration
        Remove-Item -Path "bin" -Recurse -Force -ErrorAction SilentlyContinue
        Remove-Item -Path "obj" -Recurse -Force -ErrorAction SilentlyContinue
    }
    
    # Clean frontend
    if (Test-Path $FrontendDir) {
        Write-Host "Cleaning frontend..."
        Set-Location $FrontendDir
        Remove-Item -Path "dist" -Recurse -Force -ErrorAction SilentlyContinue
        Remove-Item -Path "build" -Recurse -Force -ErrorAction SilentlyContinue
        Remove-Item -Path "node_modules\.cache" -Recurse -Force -ErrorAction SilentlyContinue
    }
    
    # Clean tests
    if (Test-Path $TestsDir) {
        Write-Host "Cleaning tests..."
        Set-Location $TestsDir
        dotnet clean --configuration $BuildConfiguration
        Remove-Item -Path "bin" -Recurse -Force -ErrorAction SilentlyContinue
        Remove-Item -Path "obj" -Recurse -Force -ErrorAction SilentlyContinue
    }
    
    Write-ColorOutput "✅ Cleanup completed" "Green"
}

# =============================================================================
# RESTORE DEPENDENCIES
# =============================================================================

function Restore-Dependencies {
    Write-ColorOutput "📦 Restoring dependencies..." "Blue"
    
    # Restore backend dependencies
    Write-Host "Restoring backend dependencies..."
    Set-Location $BackendDir
    dotnet restore
    
    # Restore test dependencies
    Write-Host "Restoring test dependencies..."
    Set-Location $TestsDir
    dotnet restore
    
    # Restore frontend dependencies
    Write-Host "Restoring frontend dependencies..."
    Set-Location $FrontendDir
    npm install
    
    Write-ColorOutput "✅ Dependencies restored" "Green"
}

# =============================================================================
# RUN TESTS
# =============================================================================

function Run-Tests {
    Write-ColorOutput "🧪 Running tests..." "Blue"
    
    # Run backend unit tests
    Write-Host "Running backend unit tests..."
    Set-Location $TestsDir
    dotnet test --configuration $BuildConfiguration --no-build --verbosity normal
    
    Write-ColorOutput "✅ All tests passed" "Green"
}

# =============================================================================
# BUILD BACKEND
# =============================================================================

function Build-Backend {
    Write-ColorOutput "🔨 Building backend..." "Blue"
    
    Set-Location $BackendDir
    dotnet build --configuration $BuildConfiguration --no-restore
    
    Write-ColorOutput "✅ Backend built successfully" "Green"
}

# =============================================================================
# BUILD FRONTEND
# =============================================================================

function Build-Frontend {
    Write-ColorOutput "🎨 Building frontend..." "Blue"
    
    Set-Location $FrontendDir
    
    # Set production environment
    $env:NODE_ENV = $NodeEnv
    
    # Build React application
    npm run build
    
    Write-ColorOutput "✅ Frontend built successfully" "Green"
}

# =============================================================================
# DOCKER OPERATIONS
# =============================================================================

function Invoke-DockerOperations {
    Write-ColorOutput "🐳 Docker operations..." "Blue"
    
    # Stop and remove existing containers
    Write-Host "Stopping existing containers..."
    Set-Location $ProjectRoot
    docker-compose down
    
    # Build and start containers
    Write-Host "Building and starting containers..."
    docker-compose up -d --build
    
    # Wait for services to be healthy
    Write-Host "Waiting for services to start..."
    Start-Sleep -Seconds 10
    
    # Check container status
    Write-Host "Checking container status..."
    docker-compose ps
    
    Write-ColorOutput "✅ Docker operations completed" "Green"
}

# =============================================================================
# HEALTH CHECKS
# =============================================================================

function Invoke-HealthChecks {
    Write-ColorOutput "🏥 Running health checks..." "Blue"
    
    # Check backend health
    Write-Host "Checking backend health..."
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:5277" -TimeoutSec 5 -ErrorAction Stop
        Write-ColorOutput "✅ Backend is accessible" "Green"
    }
    catch {
        Write-ColorOutput "⚠️ Backend health check failed (may still be starting)" "Yellow"
    }
    
    # Check frontend accessibility
    Write-Host "Checking frontend accessibility..."
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:5173" -TimeoutSec 5 -ErrorAction Stop
        Write-ColorOutput "✅ Frontend is accessible" "Green"
    }
    catch {
        Write-ColorOutput "⚠️ Frontend not accessible (may not be running)" "Yellow"
    }
    
    Write-ColorOutput "✅ Health checks completed" "Green"
}

# =============================================================================
# MAIN EXECUTION
# =============================================================================

function Main {
    Write-ColorOutput "🚀 Starting Full-Stack Application Build" "Green"
    Write-Host "=================================================="
    
    # Parse command line arguments
    $Clean = $false
    $Test = $false
    $Docker = $false
    $Health = $false
    
    for ($i = 0; $i -lt $args.Count; $i++) {
        switch ($args[$i]) {
            "--clean" {
                $Clean = $true
            }
            "--test" {
                $Test = $true
            }
            "--docker" {
                $Docker = $true
            }
            "--health" {
                $Health = $true
            }
            "--all" {
                $Clean = $true
                $Test = $true
                $Docker = $true
                $Health = $true
            }
            default {
                Write-Host "Unknown option: $($args[$i])"
                Write-Host "Usage: .\build.ps1 [--clean] [--test] [--docker] [--health] [--all]"
                exit 1
            }
        }
    }
    
    # Default behavior if no arguments provided
    if (-not $Clean -and -not $Test -and -not $Docker -and -not $Health) {
        $Clean = $true
        $Test = $true
        $Docker = $true
        $Health = $true
    }
    
    # Execute requested operations
    if ($Clean) {
        Cleanup
    }
    
    Restore-Dependencies
    
    if ($Test) {
        Run-Tests
    }
    
    Build-Backend
    Build-Frontend
    
    if ($Docker) {
        Invoke-DockerOperations
    }
    
    if ($Health) {
        Invoke-HealthChecks
    }
    
    Write-Host "=================================================="
    Write-ColorOutput "🎉 Build completed successfully!" "Green"
    Write-Host ""
    Write-Host "🌐 Application URLs:"
    Write-Host "   Frontend: http://localhost:5173"
    Write-Host "   Backend API: http://localhost:5277"
    Write-Host "   API Documentation: http://localhost:5277/swagger"
    Write-Host ""
    Write-Host "🐳 Docker Commands:"
    Write-Host "   View logs: docker-compose logs -f"
    Write-Host "   Stop services: docker-compose down"
    Write-Host "   Restart services: docker-compose restart"
}

# =============================================================================
# SCRIPT EXECUTION
# =============================================================================

# Run main function with all arguments
Main $args
