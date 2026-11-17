# 0011 Persistência: MikroORM 7, entidades base, filtros e soft delete

**Status:** aceita (17/11/2026). Detalha o ADR 0002 para a fatia 2.

**Contexto.** A fatia 2 precisa garantir, na camada de dados e não em cada serviço, que todo registro tem dono, nunca some e só aparece para o dono. O dono do projeto pediu uma classe base da qual as entidades herdem.

**Decisão.**

- **MikroORM 7** (`@mikro-orm/postgresql`, `@mikro-orm/nestjs`, `@mikro-orm/migrations`, `@mikro-orm/seeder`). O 7 é só ESM e a API roda em CommonJS: funciona porque o Node >= 22.12 carrega ESM via `require`. Entidades com decorators "legacy" (`@mikro-orm/decorators/legacy`), parecidos com os atributos do EF Core, e com o tipo de cada coluna explícito, porque o CLI das migrations roda com `tsx`, que não gera metadata de tipos.
- **Herança:** `BaseEntity` (id UUID v7 gerado na aplicação, `createdAt`, `updatedAt`, `deletedAt`, `version` para concorrência otimista) e `OwnedEntity extends BaseEntity` (acrescenta `ownerId`). Dado de usuário estende `OwnedEntity`; o próprio `User` estende `BaseEntity`. Ficam em `apps/api/src/database/entities/`.
- **Filtros globais** (`database/filters.ts`), ligados em toda consulta: `notDeleted` (`deletedAt is null`) e `owner` (`ownerId = usuário atual`). Sem usuário na requisição, o filtro `owner` falha em vez de devolver tudo.
- **Subscriber** (`database/subscribers/base-entity.subscriber.ts`, equivalente a um `SaveChangesInterceptor`): preenche `ownerId` no insert, proíbe mudar `ownerId` e transforma `em.remove()` em update de `deletedAt`.
- **Usuário da requisição** num `AsyncLocalStorage` (`common/context/current-user.context.ts`, como um `AsyncLocal<T>`). Até a fatia 4, quem preenche é o `DevUserMiddleware`, só em `NODE_ENV=development`, pelo cabeçalho `X-Dev-User: <username>`; na fatia 4 a sessão assume.
- **Sistema:** `runAsSystem(em, fn)` (`database/system-context.ts`) é o único lugar sem filtro de dono (seeds, jobs).
- **Migrations** em `src/database/migrations`, geradas pelo CLI e aplicadas no start da API (`DB_MIGRATE_ON_START`). Registro explícito de entidades em `database/entities.ts`, como os `DbSet` de um `DbContext`.
- **Banco de desenvolvimento:** `pnpm --filter @fluxo/api db:setup` cria o usuário `fluxo_app` (não superusuário) e o banco `fluxo_dev`.
- **Entidade de exemplo** `Note` (`modules/notes`) com CRUD em `/api/notes`, só para provar as garantias. Serve de molde para a fase 1 e pode ser removida depois.

**Consequências.** Uma entidade nova é: classe em `modules/<feature>/<nome>.entity.ts` estendendo `OwnedEntity`, uma linha em `database/entities.ts` e `pnpm --filter @fluxo/api db:migration:create`. Serviços nunca filtram por dono nem por `deletedAt` à mão. Quando os testes entrarem, o Jest pode ter dificuldade com pacotes só ESM; nesse caso, usar Vitest também na API.
