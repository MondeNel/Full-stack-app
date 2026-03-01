# =============================================================================
# Full-Stack Authentication Application Makefile
# =============================================================================
# This Makefile provides cross-platform build automation for the
# authentication application with React frontend and .NET backend.
# =============================================================================

.PHONY: help clean restore test build-backend build frontend docker health all

# Default target
.DEFAULT_GOAL := help

# Colors for output
RED := \033[0;31m
GREEN := \033[0;32m
YELLOW := \033[1;33m
BLUE := \033[0;34m
NC := \033[0m # No Color

# =============================================================================
# CONFIGURATION
# =============================================================================

PROJECT_ROOT := $(shell pwd)
BACKEND_DIR := $(PROJECT_ROOT)/AuthBackend
FRONTEND_DIR := $(PROJECT_ROOT)/AuthFrontend
TESTS_DIR := $(PROJECT_ROOT)/AuthBackend.Tests

BUILD_CONFIGURATION := Release
NODE_ENV := production

# =============================================================================
# HELP TARGET
# =============================================================================

help: ## Show this help message
	@echo "$(BLUE)🔧 Full-Stack Authentication Application Build Commands$(NC)"
	@echo "=================================================="
	@echo ""
	@echo "$(GREEN)Available targets:$(NC)"
	@echo "  help          Show this help message"
	@echo "  clean         Clean all build artifacts"
	@echo "  restore       Restore all dependencies"
	@echo "  test          Run all tests"
	@echo "  build-backend Build .NET backend"
	@echo "  build         Build frontend and backend"
	@echo "  docker        Build and start Docker containers"
	@echo "  health        Run health checks"
	@echo "  all           Run complete build pipeline"
	@echo ""
	@echo "$(YELLOW)Examples:$(NC)"
	@echo "  make all                    # Complete build pipeline"
	@echo "  make build docker          # Build and run with Docker"
	@echo "  make clean build           # Clean build without Docker"

# =============================================================================
# CLEAN TARGET
# =============================================================================

clean: ## Clean all build artifacts
	@echo "$(YELLOW)🧹 Cleaning up previous builds...$(NC)"
	@if [ -d "$(BACKEND_DIR)" ]; then \
		echo "Cleaning backend..."; \
		cd $(BACKEND_DIR) && dotnet clean --configuration $(BUILD_CONFIGURATION) && rm -rf bin obj; \
	fi
	@if [ -d "$(FRONTEND_DIR)" ]; then \
		echo "Cleaning frontend..."; \
		cd $(FRONTEND_DIR) && rm -rf dist build node_modules/.cache; \
	fi
	@if [ -d "$(TESTS_DIR)" ]; then \
		echo "Cleaning tests..."; \
		cd $(TESTS_DIR) && dotnet clean --configuration $(BUILD_CONFIGURATION) && rm -rf bin obj; \
	fi
	@echo "$(GREEN)✅ Cleanup completed$(NC)"

# =============================================================================
# RESTORE TARGET
# =============================================================================

restore: ## Restore all dependencies
	@echo "$(BLUE)📦 Restoring dependencies...$(NC)"
	@echo "Restoring backend dependencies..."
	@cd $(BACKEND_DIR) && dotnet restore
	@echo "Restoring test dependencies..."
	@cd $(TESTS_DIR) && dotnet restore
	@echo "Restoring frontend dependencies..."
	@cd $(FRONTEND_DIR) && npm install
	@echo "$(GREEN)✅ Dependencies restored$(NC)"

# =============================================================================
# TEST TARGET
# =============================================================================

test: ## Run all tests
	@echo "$(BLUE)🧪 Running tests...$(NC)"
	@echo "Running backend unit tests..."
	@cd $(TESTS_DIR) && dotnet test --configuration $(BUILD_CONFIGURATION) --no-build --verbosity normal
	@echo "$(GREEN)✅ All tests passed$(NC)"

# =============================================================================
# BUILD TARGETS
# =============================================================================

build-backend: ## Build .NET backend
	@echo "$(BLUE)🔨 Building backend...$(NC)"
	@cd $(BACKEND_DIR) && dotnet build --configuration $(BUILD_CONFIGURATION) --no-restore
	@echo "$(GREEN)✅ Backend built successfully$(NC)"

build-frontend: ## Build React frontend
	@echo "$(BLUE)🎨 Building frontend...$(NC)"
	@cd $(FRONTEND_DIR) && NODE_ENV=$(NODE_ENV) npm run build
	@echo "$(GREEN)✅ Frontend built successfully$(NC)"

build: build-backend build-frontend ## Build frontend and backend

# =============================================================================
# DOCKER TARGET
# =============================================================================

docker: ## Build and start Docker containers
	@echo "$(BLUE)🐳 Docker operations...$(NC)"
	@echo "Stopping existing containers..."
	@cd $(PROJECT_ROOT) && docker-compose down
	@echo "Building and starting containers..."
	@cd $(PROJECT_ROOT) && docker-compose up -d --build
	@echo "Waiting for services to start..."
	@sleep 10
	@echo "Checking container status..."
	@cd $(PROJECT_ROOT) && docker-compose ps
	@echo "$(GREEN)✅ Docker operations completed$(NC)"

# =============================================================================
# HEALTH TARGET
# =============================================================================

health: ## Run health checks
	@echo "$(BLUE)🏥 Running health checks...$(NC)"
	@echo "Checking backend health..."
	@if curl -f http://localhost:5277 > /dev/null 2>&1; then \
		echo "$(GREEN)✅ Backend is accessible$(NC)"; \
	else \
		echo "$(YELLOW)⚠️ Backend health check failed (may still be starting)$(NC)"; \
	fi
	@echo "Checking frontend accessibility..."
	@if curl -f http://localhost:5173 > /dev/null 2>&1; then \
		echo "$(GREEN)✅ Frontend is accessible$(NC)"; \
	else \
		echo "$(YELLOW)⚠️ Frontend not accessible (may not be running)$(NC)"; \
	fi
	@echo "$(GREEN)✅ Health checks completed$(NC)"

# =============================================================================
# ALL TARGET
# =============================================================================

all: clean restore test build docker health ## Run complete build pipeline
	@echo "=================================================="
	@echo "$(GREEN)🎉 Build completed successfully!$(NC)"
	@echo ""
	@echo "🌐 Application URLs:"
	@echo "   Frontend: http://localhost:5173"
	@echo "   Backend API: http://localhost:5277"
	@echo "   API Documentation: http://localhost:5277/swagger"
	@echo ""
	@echo "🐳 Docker Commands:"
	@echo "   View logs: docker-compose logs -f"
	@echo "   Stop services: docker-compose down"
	@echo "   Restart services: docker-compose restart"

# =============================================================================
# DEVELOPMENT TARGETS
# =============================================================================

dev-backend: ## Start backend in development mode
	@echo "$(BLUE)🔧 Starting backend in development mode...$(NC)"
	@cd $(BACKEND_DIR) && dotnet run

dev-frontend: ## Start frontend in development mode
	@echo "$(BLUE)🎨 Starting frontend in development mode...$(NC)"
	@cd $(FRONTEND_DIR) && npm run dev

dev: ## Start both frontend and backend in development mode
	@echo "$(BLUE)🚀 Starting development environment...$(NC)"
	@make -j2 dev-backend dev-frontend

logs: ## Show Docker logs
	@cd $(PROJECT_ROOT) && docker-compose logs -f

stop: ## Stop Docker containers
	@cd $(PROJECT_ROOT) && docker-compose down

restart: ## Restart Docker containers
	@cd $(PROJECT_ROOT) && docker-compose restart
