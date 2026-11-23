# Fluxo de Caixa

App pessoal de fluxo de caixa e projeção financeira. Monorepo (ADR 0011): `src/backend` (C# / .NET, ASP.NET Core + EF Core + PostgreSQL), `src/frontend` (Vue 3, PWA), `tests/` (testes .NET e, no futuro, e2e), `docs/`, `deploy/` (docker-compose e scripts). Plano: `docs/plano-base.md`. Decisões: `docs/adr/`.

O dono é desenvolvedor .NET e novo em Vue: ao introduzir algo não óbvio do Vue, explique em uma frase (comentário ou PR) e, se for recorrente, acrescente em `docs/guia-nest-vue-para-dotnet.md`.

## Comandos

- Banco local: `cp deploy/.env.example deploy/.env` e `docker compose -f deploy/docker-compose.yml up -d`
- Back-end: `dotnet build src/backend/WebAPI.slnx`, `dotnet test src/backend/WebAPI.slnx`, `dotnet run --project src/backend/WebAPI`
- Testes com banco leem a connection string de `WEBAPI_TEST_POSTGRES`.
- Front-end: `cd src/frontend && npm install && npm run dev` (detalhes em `src/frontend/README.md`)
- A CI (`.github/workflows/ci.yml`) roda um job por stack; cada um só age quando a pasta tem código.

## Onde cada coisa mora

- Código de produção do back em `src/backend`; projeto de teste .NET em `tests/<Projeto>.Test`, incluído na solução.
- Front autocontido em `src/frontend` (package.json e lockfile próprios); teste de unidade do front fica junto do componente.
- Docker, `.env.example` e scripts de operação em `deploy/`.
- `.editorconfig`, `.gitignore` e `.gitattributes` só na raiz; não crie cópias dentro das pastas.

## Regras que não se quebram

- Todo dado tem dono (`OwnerId`); os filtros globais de dono e de soft delete só são desligados em contexto de sistema, de forma explícita.
- Nunca apague de verdade: remoção vira soft delete.
- Controllers e o chat chamam serviços de aplicação; nunca o `DbContext` direto.
- O front só fala com o back por HTTP; nunca importa código de `src/backend`.
- Dinheiro em centavos inteiros no código, `numeric(14,2)` no banco; datas de negócio `'YYYY-MM-DD'`; instantes `timestamptz` UTC; exibição em `America/Sao_Paulo`.
- Nomes de código em inglês; textos da interface em português.
- Nada de segredo no git: `.env` e user-secrets ficam locais.
- Toda fatia termina com um roteiro curto de verificação manual em `docs/verificacao/`.

## Glossário (código = interface)

- OwnedEntity = registro com dono
- Invite = convite
- AuditEntry = registro de auditoria
- (fase 1) Statement = fatura, Installment = parcela, Planned/Actual = previsto/realizado, SharedFlow = fluxo compartilhado

## Como trabalhar

- Branches: `feature/<descricao-curta>` para funcionalidade nova e `hotfix/<descricao-curta>` para correção. Nada de commit direto no `main` ou no `develop`.
- Uma fatia vertical por sessão (API + tela); PR pequeno, com o roteiro de verificação manual.
- Decisão de arquitetura nova = ADR em `docs/adr` antes do código.
- Ao fechar uma fatia, atualize este arquivo se surgiu convenção nova.
