#!/bin/bash

# =============================================================================
# Full-Stack Authentication Application Build Script
# =============================================================================
# This script automates the complete build, test, and deployment process
# for the authentication application with React frontend and .NET backend.
# =============================================================================

set -e  # Exit on any error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# =============================================================================
# CONFIGURATION
# =============================================================================

echo -e "${BLUE}🔧 Configuration${NC}"

# Project root directory
PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
BACKEND_DIR="$PROJECT_ROOT/AuthBackend"
FRONTEND_DIR="$PROJECT_ROOT/AuthFrontend"
TESTS_DIR="$PROJECT_ROOT/AuthBackend.Tests"

# Build configurations
BUILD_CONFIGURATION="Release"
NODE_ENV="production"

echo "Project Root: $PROJECT_ROOT"
echo "Backend Directory: $BACKEND_DIR"
echo "Frontend Directory: $FRONTEND_DIR"
echo "Tests Directory: $TESTS_DIR"

# =============================================================================
# CLEANUP FUNCTION
# =============================================================================

cleanup() {
    echo -e "${YELLOW}🧹 Cleaning up previous builds...${NC}"
    
    # Clean backend
    if [ -d "$BACKEND_DIR" ]; then
        echo "Cleaning backend..."
        cd "$BACKEND_DIR"
        dotnet clean --configuration $BUILD_CONFIGURATION
        rm -rf bin obj
    fi
    
    # Clean frontend
    if [ -d "$FRONTEND_DIR" ]; then
        echo "Cleaning frontend..."
        cd "$FRONTEND_DIR"
        rm -rf dist build node_modules/.cache
    fi
    
    # Clean tests
    if [ -d "$TESTS_DIR" ]; then
        echo "Cleaning tests..."
        cd "$TESTS_DIR"
        dotnet clean --configuration $BUILD_CONFIGURATION
        rm -rf bin obj
    fi
    
    echo -e "${GREEN}✅ Cleanup completed${NC}"
}

# =============================================================================
# RESTORE DEPENDENCIES
# =============================================================================

restore_dependencies() {
    echo -e "${BLUE}📦 Restoring dependencies...${NC}"
    
    # Restore backend dependencies
    echo "Restoring backend dependencies..."
    cd "$BACKEND_DIR"
    dotnet restore
    
    # Restore test dependencies
    echo "Restoring test dependencies..."
    cd "$TESTS_DIR"
    dotnet restore
    
    # Restore frontend dependencies
    echo "Restoring frontend dependencies..."
    cd "$FRONTEND_DIR"
    npm install
    
    echo -e "${GREEN}✅ Dependencies restored${NC}"
}

# =============================================================================
# RUN TESTS
# =============================================================================

run_tests() {
    echo -e "${BLUE}🧪 Running tests...${NC}"
    
    # Build tests first
    echo "Building tests..."
    cd "$TESTS_DIR"
    dotnet build --configuration $BUILD_CONFIGURATION
    
    # Run backend unit tests
    echo "Running backend unit tests..."
    dotnet test --configuration $BUILD_CONFIGURATION --no-build --verbosity normal
    
    echo -e "${GREEN}✅ All tests passed${NC}"
}

# =============================================================================
# BUILD BACKEND
# =============================================================================

build_backend() {
    echo -e "${BLUE}🔨 Building backend...${NC}"
    
    cd "$BACKEND_DIR"
    dotnet build --configuration $BUILD_CONFIGURATION --no-restore
    
    echo -e "${GREEN}✅ Backend built successfully${NC}"
}

# =============================================================================
# BUILD FRONTEND
# =============================================================================

build_frontend() {
    echo -e "${BLUE}🎨 Building frontend...${NC}"
    
    cd "$FRONTEND_DIR"
    
    # Set production environment
    export NODE_ENV=$NODE_ENV
    
    # Build React application
    npm run build
    
    echo -e "${GREEN}✅ Frontend built successfully${NC}"
}

# =============================================================================
# DOCKER OPERATIONS
# =============================================================================

docker_operations() {
    echo -e "${BLUE}🐳 Docker operations...${NC}"
    
    # Stop and remove existing containers
    echo "Stopping existing containers..."
    cd "$PROJECT_ROOT"
    docker-compose down
    
    # Build and start containers
    echo "Building and starting containers..."
    docker-compose up -d --build
    
    # Wait for services to be healthy
    echo "Waiting for services to start..."
    sleep 10
    
    # Check container status
    echo "Checking container status..."
    docker-compose ps
    
    echo -e "${GREEN}✅ Docker operations completed${NC}"
}

# =============================================================================
# HEALTH CHECKS
# =============================================================================

wait_for_service() {
    local name=$1
    local url=$2
    local max_attempts=${3:-12}
    local delay=${4:-5}

    echo "Checking $name health..."
    for ((i=1; i<=max_attempts; i++)); do
        if curl -sf "$url" > /dev/null 2>&1; then
            echo -e "${GREEN}✅ $name is accessible${NC}"
            return 0
        fi
        if [ $i -eq $max_attempts ]; then
            echo -e "${RED}❌ $name failed to respond after $((max_attempts * delay))s.${NC}"
            echo -e "${RED}   Run 'docker-compose logs' to investigate.${NC}"
            return 1
        fi
        echo -e "${YELLOW}⏳ $name not ready yet (attempt $i/$max_attempts) — retrying in ${delay}s...${NC}"
        sleep $delay
    done
}

health_checks() {
    echo -e "${BLUE}🏥 Running health checks...${NC}"

    backend_ok=0
    frontend_ok=0

    wait_for_service "Backend"  "http://localhost:5277/health" || backend_ok=1
    wait_for_service "Frontend" "http://localhost:5173" || frontend_ok=1

    if [ $backend_ok -ne 0 ] || [ $frontend_ok -ne 0 ]; then
        echo -e "${YELLOW}⚠️ One or more services failed health checks.${NC}"
        echo -e "${YELLOW}   Run 'docker-compose logs -f' to investigate.${NC}"
    else
        echo -e "${GREEN}✅ All health checks passed${NC}"
    fi
}

# =============================================================================
# MAIN EXECUTION
# =============================================================================

main() {
    echo -e "${GREEN}🚀 Starting Full-Stack Application Build${NC}"
    echo "=================================================="
    
    # Parse command line arguments
    CLEAN=false
    TEST=false
    DOCKER=false
    HEALTH=false
    
    while [[ $# -gt 0 ]]; do
        case $1 in
            --clean)
                CLEAN=true
                shift
                ;;
            --test)
                TEST=true
                shift
                ;;
            --docker)
                DOCKER=true
                shift
                ;;
            --health)
                HEALTH=true
                shift
                ;;
            --all)
                CLEAN=true
                TEST=true
                DOCKER=true
                HEALTH=true
                shift
                ;;
            *)
                echo "Unknown option: $1"
                echo "Usage: $0 [--clean] [--test] [--docker] [--health] [--all]"
                exit 1
                ;;
        esac
    done
    
    # Default behavior if no arguments provided
    if [[ "$CLEAN" == false && "$TEST" == false && "$DOCKER" == false && "$HEALTH" == false ]]; then
        CLEAN=true
        TEST=true
        DOCKER=true
        HEALTH=true
    fi
    
    # Execute requested operations
    if [ "$CLEAN" = true ]; then
        cleanup
    fi
    
    restore_dependencies
    
    if [ "$TEST" = true ]; then
        run_tests
    fi
    
    build_backend
    build_frontend
    
    if [ "$DOCKER" = true ]; then
        docker_operations
    fi
    
    if [ "$HEALTH" = true ]; then
        health_checks
    fi
    
    echo "=================================================="
    echo -e "${GREEN}🎉 Build completed successfully!${NC}"
    echo ""
    echo "🌐 Application URLs:"
    echo "   Frontend: http://localhost:5173"
    echo "   Backend API: http://localhost:5277"
    echo "   API Documentation: http://localhost:5277/swagger"
    echo ""
    echo "🐳 Docker Commands:"
    echo "   View logs: docker-compose logs -f"
    echo "   Stop services: docker-compose down"
    echo "   Restart services: docker-compose restart"
}

# =============================================================================
# SCRIPT EXECUTION
# =============================================================================

# Run main function with all arguments
main "$@"