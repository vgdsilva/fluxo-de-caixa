# Verificação manual: fatia 2 (persistência)

Critério da spec: com dois usuários de seed, um não enxerga o registro do outro; remover só preenche `deletedAt`.

O roteiro usou a entidade de exemplo `Note` (`/api/notes`), que foi removida logo depois da verificação, por decisão do dono do projeto. O código dela continua no histórico do git, no commit `46664bf` ("Fatia 2 (parte 2)"), e serve de molde para as entidades da fase 1. Para repetir este roteiro, faça checkout desse commit.

## Preparação

1. `apps/api/.env` com `DB_*` apontando para o Postgres local. Duas opções:
   - usuário próprio da aplicação: `pnpm --filter @fluxo/api db:setup` (pede a senha do `postgres` e cria `fluxo_app` + banco);
   - ou usar direto o `postgres` (como está hoje na máquina do Vinicius) e criar só o banco.
2. `pnpm --filter @fluxo/api dev`. No log: `Migrations aplicadas: Migration...` e `API ouvindo em http://localhost:3000/api`.
3. `pnpm --filter @fluxo/api db:seed`. Cria os usuários `ana` (admin) e `bruno`, com uma anotação cada. Rodar de novo não duplica.

O usuário da requisição vem do cabeçalho `X-Dev-User` (só em desenvolvimento, até a fatia 4).

## Passos

| #   | Ação                                                               | Esperado                                                                                          |
| --- | ------------------------------------------------------------------ | ------------------------------------------------------------------------------------------------- |
| 1   | `GET /api/notes` com `X-Dev-User: ana`                             | Só "Anotação da Ana"                                                                              |
| 2   | `GET /api/notes` com `X-Dev-User: bruno`                           | Só "Anotação do Bruno"                                                                            |
| 3   | `GET /api/notes` sem cabeçalho                                     | 401                                                                                               |
| 4   | Ana faz `GET /api/notes/<id do Bruno>` (trocando o id na URL)      | 404, como se não existisse                                                                        |
| 5   | Ana faz `PATCH` e `DELETE` no id do Bruno                          | 404 nos dois; a anotação do Bruno continua intacta para ele                                       |
| 6   | Ana cria `{"title":"forjada","ownerId":"<id qualquer>"}`           | 201; no banco o dono é a Ana (o `ownerId` do body é ignorado pelo schema e o subscriber preenche) |
| 7   | Ana cria com `{"title":""}`                                        | 400 com "Informe um título" (validação zod de `@fluxo/contracts`)                                 |
| 8   | Ana faz `PATCH` com `version: 1` e depois de novo com `version: 1` | 200 (versão vira 2) e depois 409 (concorrência otimista)                                          |
| 9   | Ana faz `DELETE` na própria anotação, depois `GET` nela            | 204 e depois 404                                                                                  |
| 10  | No banco: `select title, deleted_at from notes`                    | A linha apagada continua lá, com `deleted_at` preenchido                                          |

Exemplo (Git Bash ou PowerShell com `curl.exe`):

```bash
curl -H "X-Dev-User: ana" http://localhost:3000/api/notes
curl -X PATCH -H "X-Dev-User: ana" -H "Content-Type: application/json" -d '{"title":"x","version":1}' http://localhost:3000/api/notes/<id>
```

## Resultado em 17/11/2026

Windows 11, Node 22.18, Postgres 17.6, MikroORM 7.2.10: passos 1 a 10 OK. A tabela `notes` ficou com 4 linhas, uma com `deleted_at` preenchido e versão 3 (o soft delete também incrementa a versão). A migration inicial foi aplicada no start da API.
