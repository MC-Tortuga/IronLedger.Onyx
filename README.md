# IronLedger.Onyx
### High-Performance, Chaos-Resilient Distributed Payment Gateway

IronLedger.Onyx is an enterprise-grade payment orchestration system built with .NET 10. It is designed to solve the critical challenges of financial technology: atomic consistency, linearizable financial accounting, and self-healing infrastructure.

This project demonstrates a shift from standard CRUD patterns toward a correctness-first architecture suitable for high-stakes financial environments.

---

## Architectural Thinking

### 1. Immutable Ledger (TigerBeetle)
Traditional databases often struggle with high-volume financial balances due to row-level locking and race conditions.
* **The Design:** We utilize TigerBeetle for double-entry bookkeeping.
* **The Logic:** We avoid the "Update Balance" anti-pattern. Instead, we append immutable transfers between accounts. This ensures a perfect audit trail and prevents balance drift, effectively making financial integrity a physical law of the system.

### 2. Distributed Saga Orchestration
Handling a payment involves multiple distinct steps: Order creation, Fund reservation, Third-party API integration, and final Ledger commitment.
* **The Implementation:** Built using State Machines to manage the distributed workflow.
* **Resiliency:** If an external provider fails, the Saga automatically triggers a compensating transaction to void the pending ledger transfer, ensuring the system remains consistent without manual intervention.

### 3. Transactional Outbox Pattern
To solve the "Dual Write" problem—where a system must save to a database and notify a message broker simultaneously—we implement the Outbox Pattern.
* **Mechanism:** Messages are persisted to a PostgreSQL Outbox table within the same ACID transaction as the business data.
* **Guarantee:** A background worker ensures at-least-once delivery to RabbitMQ, even if the application process terminates unexpectedly mid-transaction.

---

## Resilience and Chaos Engineering

A senior-level system must be anti-fragile. This repository includes:
* **Chaos Monkey Middleware:** Custom middleware injected into the .NET pipeline to randomly introduce latency or internal server errors, forcing the system to prove its recovery logic.
* **Polly Resilience Pipelines:** Advanced Circuit Breakers, Hedging, and Adaptive Retries wrap all external dependencies.
* **Idempotency Guardians:** Logic that uses distributed locking to track request keys, preventing double-charging during client-side retries.

---

## Tech Stack

* **Runtime:** .NET 10 (C#)
* **Ledger:** TigerBeetle (High-performance distributed ledger)
* **Primary Database:** PostgreSQL 17 (with Entity Framework Core)
* **Messaging:** RabbitMQ via MassTransit
* **Observability:** OpenTelemetry and Serilog
* **Testing:** xUnit and Testcontainers

---

## Getting Started

### 1. Prerequisites
* .NET 10 SDK
* Docker and Docker Compose

### 2. Infrastructure Setup
To start the TigerBeetle cluster, RabbitMQ, and PostgreSQL, run:

```bash
docker-compose up -d
```

### 3. Database Migration and Initialization
Apply the relational schema and initialize the ledger accounts:

```bash
dotnet ef database update --project src/IronLedger.Infrastructure
dotnet run --project src/IronLedger.Worker --init-ledger
```

### 4. Running the Application
Start the API gateway:

```bash
dotnet run --project src/IronLedger.Api
```

---

## Testing Production Readiness

To verify that the system survives service failures:
1. Enable the Chaos Monkey in the application configuration.
2. Run the resilience test suite:

```bash
dotnet test --filter Category=Resilience
```

The tests are designed to pass even when dependencies return errors, proving the Saga's self-healing capabilities.

---

## License

Licensed under the GNU GPL v3.

I chose the GPL v3 for this project to ensure that the patterns of financial integrity and resilience demonstrated here remain open-source for the benefit of the developer community.
