# Schmeconomics Containerized Setup

This document outlines how to containerize the Schmeconomics application with both API and client running together in Docker containers.

## Overview

The setup includes:
1. **API Service** (`Schmeconomics.Api`) - ASP.NET Core Web API with EF Core migrations
2. **Client Service** (`schmeconomics.client`) - Nuxt.js frontend application
3. **Database** - SQLite (in containerized environment)
4. **Docker Orchestration** - Using docker-compose

## Files Created

### 1. `docker-compose.yml`
Orchestrates all services together:
- API service with database migration handling
- Client service
- Database service

### 2. `Dockerfile.api`
Builds the API with EF Core tools for database migrations:
- Installs dotnet-ef tool
- Runs `dotnet ef database update` before starting the app
- Uses multi-stage build for optimized deployment

### 3. `Dockerfile.client`
Builds and runs the Nuxt.js client:
- Installs dependencies
- Builds the application
- Runs in preview mode

### 4. `README.md`
Documentation on how to run the containerized setup

## Key Features

### Database Migration
The API Dockerfile ensures database migrations are applied before starting the service:
```bash
dotnet ef database update && dotnet Schmeconomics.Api.dll
```

### CORS Configuration
- Flexible CORS policy that allows all origins in production (containerized)
- Development environment maintains localhost:3000 restriction
- Can be configured via `AllowedOrigins` appsetting

### Environment Variables
- API uses `NUXT_PUBLIC_API_BASE_URL` for client-to-API communication
- Both services use standard Docker container port mappings:
  - API: 5153 (mapped to host 5153)
  - Client: 3000 (mapped to host 3000)

## Running the Setup

### Prerequisites
- Docker and Docker Compose installed

### Commands
```bash
# Run both services together
docker-compose up

# Run in detached mode
docker-compose up -d

# Stop services
docker-compose down

# View logs
docker-compose logs -f
```

## Service Access
- **API**: http://localhost:5153
- **Client**: http://localhost:3000

## Development Mode
For development, you can run individual components:
```bash
# Run API only
docker-compose up api

# Run client only  
docker-compose up client
```

The client will connect to the API at `http://localhost:5153` by default.