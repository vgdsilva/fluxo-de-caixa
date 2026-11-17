# 0012 Auditoria

**Status:** aceita (17/11/2026). Detalha a seção "Base de dados e auditoria" da spec da fase 0.

**Contexto.** Toda alteração precisa deixar rastro (quem, quando, por qual canal, antes e depois), sem depender de cada serviço lembrar. Eventos de segurança (login, login falho, reset de 2FA) também. A mesma tabela vai servir para o chat da fase 4 desfazer ações.

**Decisão.**

- **Tabela `audit.entries`** (schema próprio), só de inserção. Colunas: `id` (UUID v7), `at`, `user_id` (null = sistema), `record_owner_id` (dono do registro alterado), `channel` (`ui`, `chat`, `import`, `job`, `auth`), `action` (`create`, `update`, `delete`, `restore` ou evento de segurança), `entity_type`, `entity_id`, `changes` (jsonb, só os campos alterados como `{campo: {from, to}}`), `metadata` (jsonb, extras do evento: ip, aparelho, motivo) e `correlation_id`. A entidade `AuditEntry` não herda de `BaseEntity`.
- **Gravação automática** pelo `AuditSubscriber` (`modules/audit/audit.subscriber.ts`) no `onFlush`, no mesmo flush e na mesma transação da alteração. Roda depois do `BaseEntitySubscriber`, então o soft delete aparece como `delete` e a volta de `deletedAt` para null como `restore`. `id`, `createdAt`, `updatedAt` e `version` não entram em `changes`. Campos com `hidden: true` na entidade (hash de senha, segredo de 2FA, a partir da fatia 4) aparecem como `"***"`.
- **Eventos sem entidade** pelo `AuditWriter` (exportado pelo `AuditModule`), que grava num EntityManager próprio e na hora, para o evento ficar registrado mesmo quando a requisição termina em erro (login falho).
- **CorrelationId** por um middleware (`CorrelationIdMiddleware`), e não por um interceptor como dizia a spec: o contexto precisa envolver a requisição inteira, inclusive os guards, que rodam antes dos interceptors. Aceita `X-Correlation-Id` (UUID) da requisição e devolve no mesmo cabeçalho. Fica no `AppContext` (`common/context/app-context.ts`), que substituiu o `CurrentUserContext` e também guarda usuário, papel e canal.
- **Quem vê:** o admin vê toda a auditoria, inclusive o `changes` de dados financeiros dos outros (decisão do dono em 17/11/2026). `GET /api/audit` com filtros (`userId`, `recordOwnerId`, `entityType`, `entityId`, `action`, `channel`, `correlationId`, `from`, `to`, `limit`), só admin. A tela vem na fatia 6.
- **Em aberto (evolução futura, decisão do dono):** proteção contra UPDATE/DELETE na tabela (permissão ou trigger) e política de retenção. Hoje a API usa o usuário `postgres`, que ignora permissões; nada impede uma alteração manual na tabela.

**Consequências.** Nenhum serviço grava auditoria de alteração à mão. Um update que falha (ex.: 409 de versão) não gera linha. O volume cresce com cada alteração; índices por entidade, dono, usuário, ação e correlation id.
