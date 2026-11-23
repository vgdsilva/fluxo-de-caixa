# Fluxo de Caixa

Controle financeiro pessoal e do casal, com foco em responder "quanto vai sobrar nos próximos meses, e cabe esta compra?". Hospedado em casa, acessado pelo celular.

- Plano do sistema: [docs/plano-base.md](docs/plano-base.md)
- Decisões de arquitetura: [docs/adr/](docs/adr/README.md)
- Índice da documentação: [docs/README.md](docs/README.md)

## Estrutura

Monorepo com back-end em .NET e front-end em Vue (ADR [0011](docs/adr/0011-estrutura-monorepo.md)).

```text
src/
  backend/     API em C# / .NET (ASP.NET Core + EF Core + PostgreSQL), solução WebAPI.slnx
  frontend/    SPA em Vue 3 (PWA), projeto Node autocontido
tests/         testes do back-end (xUnit) e, no futuro, ponta a ponta
docs/          plano, ADRs, guias e roteiros de verificação
deploy/        docker-compose, .env de exemplo e scripts de operação
.github/       CI
```

Cada pasta tem um README curto com o que mora nela e os comandos:
[src/backend](src/backend/README.md), [src/frontend](src/frontend/README.md), [tests](tests/README.md), [deploy](deploy/README.md).

Regras de convivência:

- `src/frontend` e `src/backend` só se falam por HTTP; nenhum importa código do outro.
- Teste .NET mora em `tests/`, não dentro de `src/backend`.
- Nada de segredo no git: `.env` e user-secrets ficam locais; só os `.env.example` são versionados.
- `.editorconfig`, `.gitignore` e `.gitattributes` valem para o repositório inteiro e ficam só na raiz.

## Rodando local

Requisitos: .NET SDK (versão no `src/backend/global.json`), Node 22.12 ou mais novo e Docker (para o PostgreSQL).

```bash
# banco
cp deploy/.env.example deploy/.env
docker compose -f deploy/docker-compose.yml up -d

# API
dotnet run --project src/backend/WebAPI

# front (outro terminal)
cd src/frontend
npm install
npm run dev
```

Testes do back-end: `dotnet test src/backend/WebAPI.slnx`.
