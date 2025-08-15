# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Architecture Overview

This is a .NET 8 Kafka demonstration project that has evolved from simple console applications to a microservices event-driven architecture with Web APIs and Docker infrastructure:

- **Kafka.Producer**: Producer console application using Confluent.Kafka client for topic creation and message publishing with null-key message support
- **Kafka.Consumer**: Consumer console application with KafkaService implementing message consumption functionality
- **Order.API**: ASP.NET Core Web API for order management with event publishing capabilities
- **Stock.API**: ASP.NET Core Web API for stock management (currently basic template structure)
- **Shared.Event**: Class library containing shared event schemas and custom Kafka serializers/deserializers
- **Docker Compose Setup**: Single-broker Kafka cluster with KRaft mode and Kafka UI management interface

### Key Components

**Producer KafkaService** (`Kafka.Producer/KafkaService.cs:6`): Core producer service class handling Kafka operations including topic creation with 3 partitions and replication factor 1. Implements `SendSimpleMessageWithNullKey` method for sending messages without keys. Uses hardcoded broker address `localhost:9094`.

**Consumer KafkaService** (`Kafka.Consumer/KafkaService.cs:5`): Consumer service class implementing `ConsumeSimpleMessageWithNullKey` method with AutoOffsetReset.Earliest configuration and consumer group "use-case-1-group-1". Continuously polls for messages with 5-second timeout.

**Order.API Components**:
- **OrdersController** (`Order.API/Controllers/OrdersController.cs`): REST API controller with POST endpoint at `/api/orders` for order creation
- **OrderService** (`Order.API/Services/OrderService.cs`): Business logic service that creates orders and publishes `OrderCreatedEvent` to Kafka
- **IBus/Bus** (`Order.API/Services/Bus.cs`): Generic message bus abstraction with Kafka implementation using custom serializers
- **OrderCreateRequestDto** (`Order.API/DTOs/OrderCreateRequestDto.cs`): Data transfer object for order creation requests

**Stock.API Components**:
- **WeatherForecastController**: Default template controller (placeholder for future stock management functionality)

**Shared.Event Components** (`Shared.Event/`):
- **OrderCreatedEvent**: Event record containing OrderCode, UserId, and TotalPrice properties
- **Custom Serializers**: JSON-based key/value serializers and deserializers for Kafka messages
- **BusConst**: Shared constants for topic names (`order.created.event`)

**Docker Infrastructure**: 
- Kafka broker runs on ports 9092 (internal) and 9094 (external client connections)
- Kafka UI available at `http://localhost:8080` for cluster management
- Uses KRaft mode (no Zookeeper) with controller and broker roles combined

## Development Commands

### Building and Running
```bash
# Build entire solution
dotnet build Kafka.sln

# Run producer (creates topic and performs operations)
dotnet run --project Kafka.Producer

# Run consumer (consumes messages from specified topic)
dotnet run --project Kafka.Consumer

# Run Order API (Web API for order management)
dotnet run --project Order.API

# Run Stock API (Web API for stock management)
dotnet run --project Stock.API

# Build specific project
dotnet build Kafka.Producer/Kafka.Producer.csproj
dotnet build Kafka.Consumer/Kafka.Consumer.csproj
dotnet build Order.API/Order.API.csproj
dotnet build Stock.API/Stock.API.csproj
dotnet build Shared.Event/Shared.Event.csproj
```

### Docker Operations
```bash
# Start Kafka cluster and UI
docker-compose up -d

# View running containers
docker ps

# Stop services
docker-compose down

# View Kafka logs
docker-compose logs kafka

# View all service logs
docker-compose logs
```

### Project Management
```bash
# Restore NuGet packages
dotnet restore

# Clean build artifacts
dotnet clean

# Add NuGet package to specific project
dotnet add Kafka.Producer package [PackageName]
```

## Configuration Notes

- **Broker Address**: `localhost:9094` used across all Kafka services (Producer, Consumer, and APIs)
- **Topic Configuration**: 
  - Default topic "mytopic" with 3 partitions, replication factor 1 (console apps)
  - Event topic "order.created.event" for microservices communication
- **Consumer Configuration**: 
  - Consumer Group: "use-case-1-group-1" 
  - AutoOffsetReset: Earliest (reads from beginning of topic)
  - Poll Timeout: 5000ms with 500ms processing delay
- **Order.API Configuration**:
  - Kafka bootstrap servers via `BusSettings:Kafka:BootstrapServers` in appsettings
  - Producer configured with `Acks.All` for guaranteed delivery
  - Message timeout: 6000ms with auto-create topics enabled
- **Target Framework**: .NET 8.0 with nullable reference types enabled
- **Docker Target**: Linux containers for cross-platform compatibility

## Current Implementation Features

### Producer Features
- Dynamic topic creation with configurable partitions and replication factor
- Null-key message publishing with sequential numbering (10 messages per run)
- Detailed delivery result logging including all message properties
- Support for custom topic names via parameter passing

### Consumer Features  
- Message consumption with consumer group management ("use-case-1-group-1")
- Automatic offset management starting from earliest available messages
- Continuous polling loop with 5-second timeout and 500ms processing delay
- Real-time message display with value extraction
- Configurable topic consumption via parameter

### Order.API Features
- RESTful order creation endpoint (`POST /api/orders`)
- Event-driven architecture with `OrderCreatedEvent` publishing
- Generic message bus abstraction for decoupled Kafka integration
- Custom JSON serialization for Kafka messages
- Dependency injection with proper service registration
- Swagger/OpenAPI documentation support

### Stock.API Features
- Basic ASP.NET Core Web API template structure
- Swagger/OpenAPI documentation support
- Ready for stock management functionality implementation

### Shared.Event Features
- Centralized event schema definitions (`OrderCreatedEvent`)
- Reusable custom Kafka serializers and deserializers
- Shared constants for topic naming consistency
- JSON-based message serialization with proper error handling

## Example Usage Flow

### Console Applications (Original Demo)
1. Start Kafka cluster: `docker-compose up -d`
2. Run Producer to create topic and send messages: `dotnet run --project Kafka.Producer`
3. Run Consumer to consume messages: `dotnet run --project Kafka.Consumer`
4. Monitor via Kafka UI at `http://localhost:8080`

### Microservices Event-Driven Flow
1. Start Kafka cluster: `docker-compose up -d`
2. Run Order API: `dotnet run --project Order.API`
3. Run Stock API: `dotnet run --project Stock.API` 
4. Create an order via POST request to `http://localhost:[port]/api/orders`
5. Order.API publishes `OrderCreatedEvent` to `order.created.event` topic
6. Monitor events via Kafka UI at `http://localhost:8080`
7. Future: Stock.API will consume order events and manage inventory

## Dependencies

### Core Kafka Dependencies
- **Confluent.Kafka 2.11.0**: Official .NET Kafka client (used across all projects)

### Web API Dependencies  
- **Swashbuckle.AspNetCore 6.6.2**: OpenAPI/Swagger documentation (Order.API, Stock.API)

### Development Tools
- **Microsoft.VisualStudio.Azure.Containers.Tools.Targets 1.22.1**: Docker tooling integration

### Project References
- **Order.API** → **Shared.Event**: For event schemas and serialization
- **Stock.API** → **Shared.Event**: For event schemas and serialization (future consumption)