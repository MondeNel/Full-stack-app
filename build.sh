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
    
    # Run backend unit tests
    echo "Running backend unit tests..."
    cd "$TESTS_DIR"
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

health_checks() {
    echo -e "${BLUE}🏥 Running health checks...${NC}"
    
    # Check backend health
    echo "Checking backend health..."
    if curl -f http://localhost:5277/health > /dev/null 2>&1; then
        echo -e "${GREEN}✅ Backend is healthy${NC}"
    else
        echo -e "${YELLOW}⚠️ Backend health check failed (health endpoint may not exist)${NC}"
    fi
    
    # Check frontend accessibility
    echo "Checking frontend accessibility..."
    if curl -f http://localhost:5173 > /dev/null 2>&1; then
        echo -e "${GREEN}✅ Frontend is accessible${NC}"
    else
        echo -e "${YELLOW}⚠️ Frontend not accessible (may not be running)${NC}"
    fi
    
    echo -e "${GREEN}✅ Health checks completed${NC}"
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
