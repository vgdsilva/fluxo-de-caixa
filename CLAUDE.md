# Fluxo de Caixa

App pessoal de fluxo de caixa e projeção financeira. Monorepo pnpm: `apps/web` (Vue 3 + Vite + Pinia + Vue Router + Tailwind + shadcn-vue, PWA), `apps/api` (NestJS 11 + MikroORM + PostgreSQL), `packages/contracts` (schemas zod compartilhados). Plano: `docs/plano-base.md`. Fase atual: `docs/fase-0.md` (fatias 1 a 8). Decisões: `docs/adr/`. Estrutura de pastas: ADR 0010, `apps/api/src/README.md` e `apps/web/README.md`.

O dono é desenvolvedor .NET e novo em Nest/Vue: ao introduzir algo não óbvio do Nest ou do Vue, explique em uma frase (comentário ou PR) e, se for recorrente, acrescente em `docs/guia-nest-vue-para-dotnet.md`.

## Comandos

- Instalar: `pnpm install`
- Rodar local: `pnpm dev` (Turborepo: compila contracts, depois api em :3000 e web em :5173 com proxy de `/api`)
- Checagens (as mesmas da CI): `pnpm format:check && pnpm lint && pnpm typecheck && pnpm build`
- Formatar: `pnpm format`
- Configuração da API: copie `apps/api/.env.example` para `apps/api/.env`
- Banco de dev (uma vez): `pnpm --filter @fluxo/api db:setup` (pede a senha do `postgres`; cria `fluxo_app` e `fluxo_dev`)
- Nova migration: `pnpm --filter @fluxo/api db:migration:create` (as migrations rodam sozinhas no start da API)
- Dados de exemplo: `pnpm --filter @fluxo/api db:seed` (vazio por enquanto; o seed não cria usuários)
- Testar a API pelo terminal: `POST /api/auth/login` com `curl -c cookies.txt`, depois `-b cookies.txt` (cookie `fluxo_sid`)
- Ver a auditoria: `GET /api/audit?limit=20` logado como admin
- Primeiro admin: com `SETUP_DB` = 0 na tabela `system_settings`, o front abre a tela `/setup` (ADR 0014). Nenhuma migration cria usuário.
- Migration com SQL manual (dados): se o `pnpm dev` estiver rodando, o `nest --watch` aplica a migration assim que o arquivo é gerado. Pare o dev antes, ou escreva o arquivo completo de uma vez.
- Testes automatizados: ainda não existem (entram depois da fase 0). Os runners já estão configurados (Jest na api, Vitest no web).

## Regras que não se quebram

- Todo dado tem dono (`ownerId`); nunca desligue os filtros `owner`/`notDeleted` fora do `SystemContext`.
- Nunca apague de verdade: remoção vira soft delete pelo subscriber.
- Controllers e o chat chamam serviços de aplicação; nunca o `EntityManager` direto.
- Um módulo Nest só usa o que outro módulo exporta em `exports`.
- Formato de request/response só em `packages/contracts`; o front nunca escreve tipo de API à mão.
- `web` só fala com `api` por HTTP (o lint bloqueia import de `apps/api`).
- Dinheiro em centavos inteiros no código, `numeric(14,2)` no banco; datas de negócio `'YYYY-MM-DD'`; instantes `timestamptz` UTC; exibição em `America/Sao_Paulo`.
- Nomes de código em inglês; textos da interface em português.
- Toda fatia termina com um roteiro curto de verificação manual em `docs/verificacao/`.

## Versões fixadas e UI (ver ADRs 0008 e 0009)

- NestJS 11 (não 12): `nestjs-zod` e `@nestjs/config` 4 ainda exigem Nest 11.
- TypeScript 5.9: `typescript-eslint` e `ts-jest` ainda não aceitam TS 6/7.
- Componentes: shadcn-vue + Tailwind 4 (ADR 0009), não PrimeVue. Componente novo: `pnpm dlx shadcn-vue@latest add <nome>` em `apps/web`; o código vai para `src/components/ui/`. Depois de rodar o CLI, confira o `src/assets/styles/main.css`: ele recoloca um `@import` do Google Fonts que deve ser removido (a fonte é local).
- Node: CI usa a versão do `.nvmrc` (24); localmente funciona com Node >= 22.12.

## Onde fica cada coisa

- API: `modules/<feature>/` (module, controller, service, entity, dto), `common/` (context, guards, interceptors, middleware, validators, utils, constants), `config/`, `database/` (config do MikroORM, entidades base, filtros, subscriber, migrations, seeders). Auth = `modules/auth/`.
- Entidade nova: estende `OwnedEntity` (dado de usuário) ou `BaseEntity`, entra em `database/entities.ts`, gera migration. Tipo de coluna sempre explícito (`@Property({ type: ... })`); campos com default usam `Opt<T>`. Serviço nunca filtra por `ownerId`/`deletedAt` à mão nem grava auditoria de alteração à mão (o `AuditSubscriber` grava). Eventos sem entidade (login etc.) usam o `AuditWriter`. Campo sensível na entidade = `hidden: true` (sai mascarado na auditoria). Índice único em entidade com soft delete deve ser parcial (`where deleted_at is null`), senão um registro apagado bloqueia o valor. DTO = `createZodDto(schema de @fluxo/contracts)`. Molde completo de módulo com entidade dona + CRUD: commit `46664bf` (módulo notes, removido depois).
- Web (nomes fixos, pedidos pelo dono): `public/{images,icons,favicon.ico}`; em `src/`: `assets/`, `components/{common,ui,layout}`, `pages/`, `hooks/` (composables do Vue, `useX.ts`), `context/` (provide/inject), `services/` (`api.ts` + `*.service.ts`), `utils/`, `routes/` (Vue Router), `store/` (Pinia). Não renomeie para `composables/`, `router/` ou `stores/`.
- Componente não chama `fetch`: usa um hook, que usa `services/`.
- Toda rota da API exige sessão (SessionGuard global); rota aberta usa `@Public()`, rota de admin usa `@Roles('admin')`. No front, rota aberta tem `meta.public` e telas de login usam `meta.layout: 'auth'` (AuthLayout, tela dividida).
- Usuário da requisição: `AppContext.userId()` / `AppContext.role()`; nunca leia do body.

## Glossário (código = interface)

- OwnedEntity = registro com dono
- Invite = convite
- AuditEntry = registro de auditoria (`audit.entries`)
- AppContext = contexto da requisição (usuário, papel, canal, CorrelationId)
- (fase 1) Statement = fatura, Installment = parcela, Planned/Actual = previsto/realizado, SharedFlow = fluxo compartilhado

## Como trabalhar

- Branches: `feature/<descricao-curta>` para funcionalidade nova (ex.: `feature/fatia-2-persistencia`) e `hotfix/<descricao-curta>` para correção. Nada de commit direto no `main` nem no `develop`. Até o fim da fase 1, tudo vai no branch `feature/new-rebrand`.
- Uma fatia vertical por sessão (contrato + API + tela); PR pequeno, com o roteiro de verificação manual.
- Decisão de arquitetura nova = ADR em `docs/adr` antes do código.
- Ao fechar uma fatia, atualize este arquivo se surgiu convenção nova.
