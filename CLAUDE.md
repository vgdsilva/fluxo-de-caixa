# Fluxo de Caixa

App pessoal de fluxo de caixa e projeção financeira. Monorepo pnpm: `apps/web` (Vue 3 + Vite + Pinia + Vue Router + Tailwind + shadcn-vue, PWA), `apps/api` (NestJS 11 + MikroORM + PostgreSQL), `packages/contracts` (schemas zod compartilhados). Plano: `docs/plano-base.md`. Fase atual: `docs/fase-0.md` (fatias 1 a 8). Decisões: `docs/adr/`.

O dono é desenvolvedor .NET e novo em Nest/Vue: ao introduzir algo não óbvio do Nest ou do Vue, explique em uma frase (comentário ou PR) e, se for recorrente, acrescente em `docs/guia-nest-vue-para-dotnet.md`.

## Comandos

- Instalar: `pnpm install`
- Rodar local: `pnpm dev` (contracts em watch, api em :3000, web em :5173 com proxy de `/api`)
- Checagens (as mesmas da CI): `pnpm format:check && pnpm lint && pnpm typecheck && pnpm build`
- Formatar: `pnpm format`
- Configuração da API: copie `apps/api/.env.example` para `apps/api/.env`
- Nova migration (a partir da fatia 2): `pnpm --filter @fluxo/api mikro-orm migration:create`
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
- Componentes: shadcn-vue + Tailwind 4 (ADR 0009), não PrimeVue. Componente novo: `pnpm dlx shadcn-vue@latest add <nome>` em `apps/web`; o código vai para `src/components/ui/`.
- Node: CI usa a versão do `.nvmrc` (24); localmente funciona com Node >= 22.12.

## Glossário (código = interface)

- OwnedEntity = registro com dono
- Invite = convite
- AuditEntry = registro de auditoria
- (fase 1) Statement = fatura, Installment = parcela, Planned/Actual = previsto/realizado, SharedFlow = fluxo compartilhado

## Como trabalhar

- Uma fatia vertical por sessão (contrato + API + tela); PR pequeno, com o roteiro de verificação manual.
- Decisão de arquitetura nova = ADR em `docs/adr` antes do código.
- Ao fechar uma fatia, atualize este arquivo se surgiu convenção nova.
