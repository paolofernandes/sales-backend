# Running the Sales API Locally

This guide walks you through getting the API running on your machine from scratch and testing the Sales endpoints.

---

## Prerequisites

| Tool | Version | Notes |
|------|---------|-------|
| [.NET SDK](https://dotnet.microsoft.com/download) | 8.0 | `dotnet --version` to verify |
| [Docker Desktop](https://www.docker.com/products/docker-desktop/) | any recent | Must be running before step 1 |

No PostgreSQL installation needed — Docker handles it.

---

## Step 1 — Start the database

From the repo root:

```bash
docker-compose -f template/backend/docker-compose.yml up -d ambev.developerevaluation.database
```

This starts a PostgreSQL 13 container on port **5432** with:

| Setting  | Value               |
|----------|---------------------|
| Database | `developer_evaluation` |
| User     | `developer`         |
| Password | `ev@luAt10n`        |

Wait a few seconds for Postgres to initialize before proceeding.

---

## Step 2 — Apply the database migrations

```bash
dotnet ef database update \
  --project template/backend/src/Ambev.DeveloperEvaluation.ORM \
  --startup-project template/backend/src/Ambev.DeveloperEvaluation.WebApi
```

This runs all migrations, including the Sales schema (`Sales` and `SaleItems` tables).

> **Windows (PowerShell):**
> ```powershell
> dotnet ef database update `
>   --project template/backend/src/Ambev.DeveloperEvaluation.ORM `
>   --startup-project template/backend/src/Ambev.DeveloperEvaluation.WebApi
> ```

---

## Step 3 — Run the API

```bash
dotnet run --project template/backend/src/Ambev.DeveloperEvaluation.WebApi
```

The API starts on **http://localhost:5119** (or the port shown in the console).  
Swagger UI is available at: **http://localhost:5119/swagger**

---

## Step 4 — Authenticate and get a JWT token

All Sales endpoints require a Bearer token. First, create a user:

**POST** `/api/users`
```json
{
  "username": "testuser",
  "password": "Test@123456",
  "email": "test@example.com",
  "phone": "+5511999999999",
  "role": "Admin",
  "status": "Active"
}
```

Then log in:

**POST** `/api/auth`
```json
{
  "email": "test@example.com",
  "password": "Test@123456"
}
```

The response includes a `token` field. Copy it — you'll need it as a `Bearer` token for all Sales requests.

**In Swagger:** click the **Authorize** button (top right), paste the token, and click **Authorize**.  
**In curl/Postman:** add header `Authorization: Bearer <your-token>`.

---

## Step 5 — Test the Sales endpoints

### Create a sale

**POST** `/api/sales`
```json
{
  "saleNumber": "SALE-001",
  "saleDate": "2026-05-04T10:00:00Z",
  "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "customerName": "Acme Corp",
  "branchId": "3fa85f64-5717-4562-b3fc-2c963f66afb7",
  "branchName": "Downtown Branch",
  "items": [
    {
      "productId": "3fa85f64-5717-4562-b3fc-2c963f66afc8",
      "productName": "Beer 600ml",
      "quantity": 4,
      "unitPrice": 8.50
    },
    {
      "productId": "3fa85f64-5717-4562-b3fc-2c963f66afd9",
      "productName": "Beer 350ml",
      "quantity": 12,
      "unitPrice": 5.00
    }
  ]
}
```

The response includes the sale `id` and shows discounts automatically applied:
- 4 units → **10% discount**
- 12 units → **20% discount**

Save the sale `id` from the response for the next requests.

---

### Get a sale

**GET** `/api/sales/{id}`

To include cancelled items in the response:  
**GET** `/api/sales/{id}?includeCancelledItems=true`

---

### List sales (paginated)

**GET** `/api/sales?page=1&pageSize=10`

---

### Update a sale

**PUT** `/api/sales/{id}`
```json
{
  "saleDate": "2026-05-04T12:00:00Z",
  "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "customerName": "Acme Corp Updated",
  "branchId": "3fa85f64-5717-4562-b3fc-2c963f66afb7",
  "branchName": "Downtown Branch",
  "items": [
    {
      "productId": "3fa85f64-5717-4562-b3fc-2c963f66afc8",
      "productName": "Beer 600ml",
      "quantity": 10,
      "unitPrice": 8.50
    }
  ]
}
```

---

### Cancel an item within a sale

**POST** `/api/sales/{saleId}/items/{itemId}/cancel`

Use the `id` of an item from the `GET` response. Returns the updated `TotalAmount` of the sale.

---

### Cancel an entire sale

**POST** `/api/sales/{id}/cancel`

---

### Delete a sale

**DELETE** `/api/sales/{id}`

---

## Discount business rules (quick reference)

| Quantity per product | Discount applied |
|----------------------|-----------------|
| 1–3 items            | 0%              |
| 4–9 items            | 10%             |
| 10–20 items          | 20%             |
| > 20 items           | Request rejected (`400 Bad Request`) |

---

## Run the unit tests

```bash
dotnet test template/backend/Ambev.DeveloperEvaluation.sln
```

To run only the Sales-related tests:

```bash
dotnet test template/backend/tests/Ambev.DeveloperEvaluation.Unit \
  --filter "FullyQualifiedName~Sale"
```

---

## Troubleshooting

| Problem | Fix |
|---------|-----|
| `connection refused` on port 5432 | Docker container isn't running — check `docker ps` and restart with the command in Step 1 |
| `relation "Sales" does not exist` | Migrations haven't run — repeat Step 2 |
| `401 Unauthorized` on Sales endpoints | JWT token missing or expired — repeat Step 4 |
| Port conflict on 5432 | Stop any local PostgreSQL service, or change the host port in `docker-compose.yml` |
