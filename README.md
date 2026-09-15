# CashFlow Challenge

API em C# para registrar créditos e débitos e consultar o saldo diário consolidado.

## Arquitetura

- `CashFlow.Domain`: regras e entidades de negócio.
- `CashFlow.Application`: casos de uso e contratos.
- `CashFlow.Infrastructure`: EF Core, PostgreSQL, Redis outbox, Kafka e DLQ.
- `CashFlow.API`: endpoints HTTP minimalistas.
- `CashFlow.Tests`: testes unitários.

Fluxo resumido:
1. `POST /transactions` grava o lançamento no PostgreSQL.
2. O evento entra no Redis como outbox.
3. Um worker lê a outbox e publica no Kafka.
4. O consumer consolida o saldo diário.
5. Se o processamento falhar, a mensagem vai para a DLQ.
6. O `Id` da transação evita duplicidade quando a mesma requisição é reenviada.

## Pré-requisitos

- .NET 8 SDK
- Docker + Docker Compose

## Como rodar

### Opção 1: tudo com Docker

```bash
docker compose up --build
```

API:

- `http://localhost:8080`

Swagger:

- `http://localhost:8080/swagger`

### Opção 2: rodar a API local e deixar infra no Docker

```bash
docker compose up -d postgres kafka redis
dotnet run --project CashFlow.API
```

## Testes

```bash
dotnet test
```

## Endpoints

### Criar lançamento

`POST /transactions`

Exemplo de `curl`:

```bash
curl -X POST "http://localhost:8080/transactions" \
  -H "Content-Type: application/json" \
  -d '{
    "id": "c0b1d5d4-1f8b-4d8f-9db4-0c2b7dba6d8b",
    "amount": 150.00,
    "type": "Credit",
    "description": "Venda"
  }'
```

Body:

```json
{
  "id": "c0b1d5d4-1f8b-4d8f-9db4-0c2b7dba6d8b",
  "amount": 150.00,
  "type": "Credit",
  "description": "Venda"
}
```

### Consultar saldo diário

`GET /balances/daily/{date}`

Exemplo:

```bash
curl "http://localhost:8080/balances/daily/2026-09-14"
```

## Kafka

- Tópico principal: `cashflow.transactions.created`
- DLQ: `cashflow.transactions.created.dlq`

## Redis

- Fila pendente: `cashflow:transactions:outbox:pending`
- Fila em processamento: `cashflow:transactions:outbox:processing`
- DLQ do outbox: `cashflow:transactions:outbox:dlq`

