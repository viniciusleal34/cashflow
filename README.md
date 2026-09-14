# CashFlow Challenge

API em C# para registrar créditos e débitos e consultar o saldo diário consolidado.

## Arquitetura

- `CashFlow.Domain`: regras e entidades de negócio.
- `CashFlow.Application`: casos de uso e contratos.
- `CashFlow.Infrastructure`: EF Core, PostgreSQL, Kafka e DLQ.
- `CashFlow.API`: endpoints HTTP minimalistas.
- `CashFlow.Tests`: testes unitários.

Fluxo resumido:
1. `POST /transactions` grava o lançamento no PostgreSQL.
2. O evento de criação é publicado no Kafka.
3. O consumer consolida o saldo diário.
4. Se o processamento falhar, a mensagem vai para a DLQ.
5. Se Kafka ou banco demorarem para subir, a aplicação tenta novamente sem derrubar a API.

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
docker compose up -d postgres kafka
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
    "amount": 150.00,
    "type": "Credit",
    "occurredAtUtc": "2026-09-14T10:00:00Z",
    "description": "Venda"
  }'
```

Body:

```json
{
  "amount": 150.00,
  "type": "Credit",
  "occurredAtUtc": "2026-09-14T10:00:00Z",
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

## Observações

- Não foi usado `MediatR`, para manter a solução simples.
- A persistência usa Entity Framework Core com PostgreSQL.
- Não há arquivos `Class1.cs` no projeto.

## Melhorias futuras

- Outbox para garantir ainda mais confiabilidade na publicação do evento.
- Retentativas no consumer com backoff exponencial.
- Endpoint para consultar saldo por período.

