# Verificação manual: fatia 3 (auditoria)

Critério da spec: uma alteração gera linha com antes e depois no banco.

## Preparação

1. `pnpm --filter @fluxo/api dev` (ou `pnpm dev`). No log: `Migrations aplicadas: Migration20261117181550` na primeira vez.
2. `pnpm --filter @fluxo/api db:seed` se ainda não houver os usuários `ana` (admin) e `bruno`.

## Passos

| #   | Ação                                                                                                | Esperado                                                                                                                                                                                           |
| --- | --------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1   | `GET /api/me` com `X-Dev-User: ana`                                                                 | Perfil da Ana com `version`                                                                                                                                                                        |
| 2   | `PATCH /api/me` com `{"name":"Ana Silva","version":<a do passo 1>}` e `X-Correlation-Id: <um uuid>` | 200; a resposta traz o mesmo `x-correlation-id`                                                                                                                                                    |
| 3   | `GET /api/audit?limit=5` com `X-Dev-User: ana`                                                      | Primeira linha: `action: update`, `entityType: User`, `channel: ui`, `changes: {"name":{"from":"Ana (seed)","to":"Ana Silva"}}`, `correlationId` igual ao do passo 2                               |
| 4   | Repetir o `PATCH` com a versão antiga                                                               | 409, e nenhuma linha nova na auditoria                                                                                                                                                             |
| 5   | `GET /api/audit` com `X-Dev-User: bruno`                                                            | 403 (só admin)                                                                                                                                                                                     |
| 6   | `GET /api/audit` sem cabeçalho                                                                      | 401                                                                                                                                                                                                |
| 7   | Criar, editar, remover e restaurar um registro (pelo ORM, como em um job)                           | Quatro linhas: `create` (campos preenchidos, `from: null`), `update`, `delete` (`deletedAt` de null para a data) e `restore` (`deletedAt` de volta para null), com `channel: job` e `userId: null` |
| 8   | No banco: `select action, entity_type, changes from audit.entries order by at desc limit 5`         | As mesmas linhas, com `changes` em jsonb                                                                                                                                                           |

Exemplo:

```bash
curl -H "X-Dev-User: ana" http://localhost:3000/api/me
curl -X PATCH -H "X-Dev-User: ana" -H "Content-Type: application/json" -d '{"name":"Ana Silva","version":1}' http://localhost:3000/api/me
curl -H "X-Dev-User: ana" "http://localhost:3000/api/audit?limit=5"
```

## Resultado em 17/11/2026

Postgres 17.6, MikroORM 7.2.10: passos 1 a 8 OK. O passo 7 foi feito com um script (tsx) que criou os usuários `teste-auditoria` e `teste-auditoria-2`, alterou, removeu e restaurou; os dois ficaram removidos (soft delete) no banco de dev. O mascaramento de campos `hidden` só terá o que testar na fatia 4 (hash de senha).
