# Simplified Backend Using Microservices Architecture for ERP-Like System

## Overview
This project is a small microservices-based system using **C# .NET 10.0**, **Apache Kafka**, **PostgreSQL**, and **Docker**.
It simulates event-driven communication between independent services.

---

## How to run
1. Make sure all tech stack is installed and follows the right version
2. Clone the repository: `git clone https://github.com/anaswebby/simple-microservices-system.git`
3. Enter project folder: `cd simple-microservices-system`
4. Make sure folders contain the right file
5. Open `infra` folder and build docker using command `docker compose build`
6. Run `docker compose up -d` to start service using docker
7. Verify all services is running by using `docker ps`
8. Access APIs using: `http://localhost:5000/swagger/index.html`
9. Create a new PO using POST /api/purchase-orders
10. Check all created PO with status filtering using GET /api/purchase-orders
11. Check all created PO with id using GET /api/purchase-orders/{id}

Extra:
1. to stop the system use: `docker-compose stop`
2. to remove the system use: `docker-compose down`

---

### Tech Stack & Versions
- .NET 10.0
- Apache Kafka (Redpanda)
- PostgreSQL 16
- Docker & Docker Compose
- Git

---

## Architecture Overview
Client -> Order Service -> Kafka (po.created) -> Inventory Service -> Kafka (po.confirmed / po.rejected) -> Notification Service)

### Services:
- **Order Service**: Create and manage Purchase Order (PO).
- **Inventory Service**: Validates and manage stock based on PO.
- **Notification Service**: Sends notifications when PO status changed (PENDING -> CONFIRMED or PENDING -> REJECTED).

### Clarification:
- Each service has its own database.
- Each service has its own Docker container.
- Each service communicate only through Kafka.

### Topics:
- **po.created**: Triggered when a new PO is created.
- **po.confirmed**: Triggered when a PO is confirmed.
- **po.rejected**: Triggered when a PO is rejected.

### Database: 
- **PostgrSQL**: Each service has its own database.
- **orderdb**: owns by order service. Contains table **PurchaseOrders** and **PurchaseOrderItems** for keeping record every PO and every Products for each PO.
- **inventorydb**: owns by inventory service. Contains table **InventoryItems** for keeping track of available quantity for each product SKU.
- **notificationdb**: owns by notification service. Contains table **AuditLogs** for keeping track of status and message for each PO.

---

## What is completed:
- Order Service:
    - Using Minimal API for create PO
    - Get Purchase Order by ID
    - List Purchase Orders with status filtering
- Inventory Service:
    - Kafka consumer for topic **po.created**
    - Kafka producer for topic **po.confirmed** and **po.rejected**
    - Stock validation for every item in PO
-Notification Service:
    - Kafka consumer for order status events
- Event-driven communication using Kafka
- PostgreSQL databases for each service
- Docker Compose for system startup
- Swagger documentation for APIs

---

## What is not completed:
- No retry handling for Kafka
- No dead letter queue for Kafka
- No centralized logging
- No CI/CD pipeline
- No distributed tracing

---

## API Documentation

### Swagger URL: http://localhost:5000/swagger/index.html

### API Endpoints:
- POST /api/purchase-orders
    - Create a new PO
    - Request Body:
    {
        "poNumber": "PO-003",
        "items": [
            {
            "productSku": "SKU-2",
            "quantity": 600
            }
        ]
    }
    - Response Body:
    {
        "id": "f26aeea1-f892-4f94-8b56-c6e68c90f5a1",
        "poNumber": "PO-003",
        "status": 0,
        "createdAt": "2025-12-04T02:20:56.7348836Z",
        "items": [
            {
            "id": "02d5f036-8b61-45bf-bfce-3de7497bd15e",
            "poId": "f26aeea1-f892-4f94-8b56-c6e68c90f5a1",
            "productSKU": "SKU-2",
            "quantity": 600
            }
        ]
    }
- GET /api/purchase-orders
    - List all PO based on Status filter
    - Response Body:
    [
        {
            "id": "97965583-6e12-441b-8382-5919b34b91e0",
            "poNumber": "PO-002",
            "status": 1,
            "createdAt": "2025-12-04T02:17:20.209274Z",
            "items": [
            {
                "id": "9f73fc13-84ff-428b-a2d4-ddd7609058f1",
                "poId": "97965583-6e12-441b-8382-5919b34b91e0",
                "productSKU": "SKU-2",
                "quantity": 20
            }
            ]
        }
    ]
- GET /api/purchase-orders/{id}
    - Get PO by ID
    - Response Body:
    {
        "id": "97965583-6e12-441b-8382-5919b34b91e0",
        "poNumber": "PO-002",
        "status": 1,
        "createdAt": "2025-12-04T02:17:20.209274Z",
        "items": [
            {
            "id": "9f73fc13-84ff-428b-a2d4-ddd7609058f1",
            "poId": "97965583-6e12-441b-8382-5919b34b91e0",
            "productSKU": "SKU-2",
            "quantity": 20
            }
        ]
    }
