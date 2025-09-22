# Trade Executor API

This is a .NET-based API application for executing trades, storing them in a SQL Server database, and publishing trade events to a Kafka message queue. The entire stack runs via Docker Compose.

## Features

- REST API for trade management
- SQL Server database integration
- Kafka messaging for trade events
- Docker Compose for local development

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/get-started)

## Getting Started

1. **Download project files**  
   ```bash
   cd trade.executor\TradeExecutor.api
2. **Build and start all services with Docker Compose:**
    ```bash
    docker-compose up --build
    
    This will start:
   - SQL Server
   - ASP.NET Core API
   - Zookeeper & Kafka
   - Kafdrop (Kafka UI)

3. **API Endpoints:**
   - `POST /trades/add` — Add a new trade
   - `POST /trades/update/{id}` — Update a trade
   - `GET /trades/{id}` — Get a trade by Id
   - `GET /trades` — List all trades

   They can be accessed and tested using Swagger. Once the app is running, open your browser and navigate to:
   `http://localhost:5243/swagger`

4. **Kafka:**
   - Trades are published to the Kafka topic specified in `docker-compose.yml` (`trades`).
   - Kafdrop UI available at [http://localhost:9000], to visually inspect Kafka messages, partitions and offsets

5. **Configuration:**
    - Database connection and Kafka settings are set in `docker-compose.yml` and can be overridden via environment variables.

6. **Future Improvements:**
    - Add Authentication and Authorization, including claims and roles, for users using the API. Retrieve `customerId` from user session.
    - If another topic is needed, use generics in `MessagingService.cs` so that a `T` message model can be published. 
    - Expand the functionality to add further trade processing and update `TradeStatus` accordingly
    - Depending on consumers' needs, optimize message model to include only necessary properties