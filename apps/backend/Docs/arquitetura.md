# Arquitetura do backend

## Projetos e dependências

```
WebAPI.Common      (sem dependências)          contratos, exceções, opções, utilitários
WebAPI.Data        → Common                    EF Core, entidades, mapeamentos, repositório genérico
WebAPI.Framework   → Common, Data              infraestrutura: background, fila, segurança, MVC, DI
WebAPI.Core        → Common, Data, Framework   regras de negócio, handlers, webhooks, validadores
WebAPI      → Core, Framework, Data, Common   host HTTP (controllers, middlewares)
WebAPI.Worker      → Core, Framework           host só de background (opcional)
WebAPI.Test        → todos                     xUnit
```

> O registro de DI do Core (`AddWebAPICore`) fica no próprio Core, porque Framework não referencia Core.

## Fluxo de mensagens (Transactional Outbox)

1. O caso de uso altera entidades e chama `IOutbox.Add(evento)`.
2. `SaveChangesAsync` grava entidades **e** a `OutboxMessage` na mesma transação.
3. `OutboxDispatcherService` lê mensagens pendentes e publica na fila (`IMessagePublisher`).
4. `QueueConsumerService` lê a fila, checa a `InboxMessage` (idempotência) e chama os `IMessageHandler<T>`.

Hoje a fila é em memória (`Channel<T>`). Para rodar WebAPI e Worker em processos separados,
troque por RabbitMQ/Service Bus implementando `IMessagePublisher` + `IMessageQueueReader`.

## Webhooks

- **Saída (outbound):** `IWebhookDispatcher.DispatchAsync(evento)` cria um `WebhookDelivery` por assinatura ativa.
  O `WebhookDeliveryService` faz o POST assinado (`X-Webhook-Signature: t=...,v1=HMAC-SHA256`)
  e reprograma falhas com backoff exponencial (`IWebhookRetryPolicy`).
- **Entrada (inbound):** `POST /api/webhooks/inbound/{origem}` valida o HMAC com o segredo em
  `Webhooks:InboundSecrets:{origem}`, grava no outbox e responde `202`.

## Banco de dados

PostgreSQL (provider Npgsql). Para subir um banco local: `docker compose up -d` na raiz.
Em Development o banco é criado automaticamente no start enquanto não houver migrations.
Datas com hora usam `DateTimeOffset` sempre em UTC (coluna `timestamp with time zone`).
