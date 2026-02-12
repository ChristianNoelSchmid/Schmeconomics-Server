# Schmeconomics.Entities
## Running database management
1. Migrations
  - In root solution folder: `dotnet ef migrations add --project Schmeconomics.Entities --startup-project Schmeconomics.Api [migration-name]`
2. Database Update
  - In root solution folder: `dotnet ef database update --project Schmeconomics.Entities --startup-project Schmeconomics.Api`