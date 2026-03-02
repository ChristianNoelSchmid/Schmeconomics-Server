# Schmeconomics - Containerized Setup

This project contains both the backend API and frontend client applications, containerized for easy deployment.

## Project Structure

- `Schmeconomics.Api/` - ASP.NET Core Web API backend
- `schmeconomics.client/` - Nuxt.js frontend application  
- `Dockerfile.api` - Docker configuration for the API
- `Dockerfile.client` - Docker configuration for the client
- `docker-compose.yml` - Orchestration file for both services

## Running with Docker Compose

To run both applications together in containers:

```bash
docker-compose up
```

This will:
1. Build and start the database container
2. Build and start the API container, which will:
   - Run database migrations using EF Core
   - Start the ASP.NET Core application on port 5153
3. Build and start the client container, which will:
   - Build the Nuxt.js application
   - Serve it on port 3000

## Environment Variables

### API Configuration
The API supports these environment variables:
- `ASPNETCORE_ENVIRONMENT` - Set to "Production" or "Development"
- `AllowedOrigins` - CORS policy origins (default: "*")

### Client Configuration  
The client supports this environment variable:
- `NUXT_PUBLIC_API_BASE_URL` - Base URL for API calls (default: "http://localhost:5153")

## Accessing the Applications

- **API**: http://localhost:5153
- **Client**: http://localhost:3000

## Development Mode

For development, you can run each component separately:

### Running API Only
```bash
docker-compose up api
```

### Running Client Only  
```bash
docker-compose up client
```

The client will connect to the API at `http://localhost:5153` by default.

## Database Migration

Database migrations are automatically handled when starting the API container. The Dockerfile.api includes a step to run `dotnet ef database update` before starting the application.