TMA — Task Management APIs

Project summary

Small microservices-style demo for a task-management system split into three independently built .NET 9 services:

- AuthApiSolution (AuthApi)
  - ASP.NET Core Web API that manages users and issues JWTs. Uses EF Core + SQL Server and contains EF migrations.
  - Exposes endpoints for register/login and user info.

- TaskApiSolution (TaskApi)
  - ASP.NET Core Web API that implements task CRUD and business logic. Validates JWTs issued by AuthApi and publishes domain events to RabbitMQ.

- NotificationSolution (NotificationService)
  - .NET Worker service that consumes RabbitMQ messages and sends email notifications (FluentEmail.Smtp). Uses Polly for resilience.

Key integrations and infrastructure

- Authentication: AuthApi issues JWTs. TaskApi validates tokens and includes an AuthApi HTTP client for cross-service calls.
- Messaging: TaskApi publishes events to RabbitMQ. NotificationService subscribes to RabbitMQ to send emails.
- Logging/observability: Services send structured logs to Seq
- Databases: Each API uses its own SQL Server database
- Reverse proxy: Traefik is used in the compose file to expose hostnames (authapi.localhost, taskapi.localhost).

 Docker Compose

The compose file wires the full stack for local development and includes:
- rabbitmq (rabbitmq:3-management)
- two SQL Server instances (sql_authapi, sql_taskapi)
- init scripts for creating DBs and users for each database
- authapi (built from AuthApi Dockerfile) and an authapi-migrator target to run EF Core migrations
- taskapi (built from TaskApi Dockerfile) and a Flyway container to run SQL migrations for TaskApi
- notificationservice (built from NotificationService Dockerfile)
- smtp4dev (local SMTP web UI)
- seq (datalust/seq)
- traefik (reverse proxy for host-based routing)

Ports exposed (defaults in compose)
- RabbitMQ management UI: 15672
- Seq web UI: 8090
- smtp4dev UI: 5080
- SQL Server (taskapi): host 1434 -> container 1433
- SQL Server (authapi): host 1435 -> container 1433
- Traefik: 80 (hosts authapi.localhost and taskapi.localhost via labels)

How to run locally (recommended)
1. Docker Desktop or any docker engine is required. Ensure docker desktop is running with Linux containers.
2. Create a .env with required variables. Minimal variables can be found in template.env file
3. Start the services: docker compose -f tma_compose.yaml up --build -d
4. Check health/status: RabbitMQ UI (http://localhost:15672), Seq (http://localhost:8090), smtp4dev (http://localhost:5080), services via traefik hostnames (authapi.localhost, taskapi.localhost)

Stopping the environment
- docker compose -f tma_compose.yaml down

Tech stack
- Languages & frameworks: C#, .NET 9, ASP.NET Core Web API, .NET Worker
- Persistence & migrations: Microsoft SQL Server, EF Core, Flyway
- Messaging: RabbitMQ
- Email: FluentEmail.Smtp
- Resilience: Polly
- Logging: Serilog + Seq
- Containerization: Docker, Docker Compose, Traefik (local reverse proxy)
- Testing: xUnit, NSubstitute
- Tools: AutoMapper, BCrypt