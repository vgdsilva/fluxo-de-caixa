# Fluxo de Caixa

Controle financeiro pessoal e do casal, com foco em responder "quanto vai sobrar nos próximos meses, e cabe esta compra?". Hospedado em casa, acessado pelo celular.

- Plano do sistema: [docs/plano-base.md](docs/plano-base.md)
- Fase atual (fundação): [docs/fase-0.md](docs/fase-0.md)
- Decisões de arquitetura: [docs/adr/](docs/adr/README.md)
- Nest e Vue para quem vem do .NET: [docs/guia-nest-vue-para-dotnet.md](docs/guia-nest-vue-para-dotnet.md)

## Estrutura

```text
apps/api            NestJS 11 (API em /api), sendo substituída pelo back em .NET (ADR 0010)
apps/backend        C# / .NET 10: solução WebAPI.sln (ASP.NET Core + EF Core + PostgreSQL)
apps/web            Vue 3 + Vite + Pinia + Tailwind + shadcn-vue (SPA/PWA)
packages/contracts  schemas zod compartilhados (@fluxo/contracts)
docs/               plano, spec da fase 0, ADRs, roteiros de verificação
```

## Rodando local

Requisitos: Node >= 22.12 (24 recomendado, ver `.nvmrc`), pnpm 11, PostgreSQL local (a partir da fatia 2).

```bash
pnpm install
cp apps/api/.env.example apps/api/.env
pnpm dev
```

Abra http://localhost:5173. A API fica em http://localhost:3000/api e o Vite repassa `/api` para ela.

Checagens (as mesmas da CI): `pnpm format:check && pnpm lint && pnpm typecheck && pnpm build`.

### Back-end .NET

Requisitos: .NET SDK 10 e PostgreSQL (ou `docker compose up -d` em `apps/backend`). Detalhes em [apps/backend/README.md](apps/backend/README.md).

```bash
cd apps/backend
dotnet build
dotnet run --project WebAPI
```
