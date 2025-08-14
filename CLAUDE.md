# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Architecture Overview

This is a .NET 8 Kafka demonstration project with two console applications and Docker infrastructure:

- **Kafka.Producer**: Producer application using Confluent.Kafka client for topic creation and message publishing with null-key message support
- **Kafka.Consumer**: Consumer application with KafkaService implementing message consumption functionality
- **Docker Compose Setup**: Single-broker Kafka cluster with KRaft mode and Kafka UI management interface

### Key Components

**Producer KafkaService** (`Kafka.Producer/KafkaService.cs:6`): Core producer service class handling Kafka operations including topic creation with 3 partitions and replication factor 1. Implements `SendSimpleMessageWithNullKey` method for sending messages without keys. Uses hardcoded broker address `localhost:9094`.

**Consumer KafkaService** (`Kafka.Consumer/KafkaService.cs:5`): Consumer service class implementing `ConsumeSimpleMessageWithNullKey` method with AutoOffsetReset.Earliest configuration and consumer group "use-case-1-group-1". Continuously polls for messages with 5-second timeout.

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

# Build specific project
dotnet build Kafka.Producer/Kafka.Producer.csproj
dotnet build Kafka.Consumer/Kafka.Consumer.csproj
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

- **Broker Address**: Currently hardcoded to `localhost:9094` in both Producer and Consumer KafkaServices
- **Topic Configuration**: Default topic "mytopic" with 3 partitions, replication factor 1
- **Consumer Configuration**: 
  - Consumer Group: "use-case-1-group-1" 
  - AutoOffsetReset: Earliest (reads from beginning of topic)
  - Poll Timeout: 5000ms with 500ms processing delay
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

## Example Usage Flow

1. Start Kafka cluster: `docker-compose up -d`
2. Run Producer to create topic and send messages: `dotnet run --project Kafka.Producer`
3. Run Consumer to consume messages: `dotnet run --project Kafka.Consumer`
4. Monitor via Kafka UI at `http://localhost:8080`

## Dependencies

- **Confluent.Kafka 2.11.0**: Official .NET Kafka client
- **Microsoft.VisualStudio.Azure.Containers.Tools.Targets 1.22.1**: Docker tooling integration