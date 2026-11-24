# Fluxo de Caixa

App pessoal de fluxo de caixa e projeção financeira. Monorepo (ADR 0011): `src/backend` (C# / .NET, ASP.NET Core + EF Core + PostgreSQL), `src/frontend` (Vue 3, PWA), `tests/` (testes .NET (ainda vazio) e, no futuro, e2e), `docs/`, `deploy/` (docker-compose e scripts). Plano: `docs/plano-base.md`. Decisões: `docs/adr/`.

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
- `archive/stack-nest` é a stack Nest/pnpm antiga, só para consulta: não edite, não importe e não use como base.

## Back-end .NET (ADR 0010)

- Um projeto só: `src/backend/WebAPI` (.NET 10, EF Core + Npgsql), pastas por tipo (ADR 0012): `Config`, `Endpoints`, `Dtos`, `Entities`, `Enums`, `Exceptions`, `Services` (+ `Impl`), `Security`, `Middlewares`, `Utils`, `Data`. Sem repositories: o service usa o `WebAPIContext`.
- Minimal API, um arquivo por endpoint em `Endpoints/<Área>/`: classe estática com `Request`, `Response`, `Validation` (FluentValidation) e `Endpoint : IEndpoint` (achado por reflection). Tudo em `/api` exige login; público usa `.AllowAnonymous()`. Erro de negócio = exceção que herda de `AppException` (já com o status).
- Versões de pacote só em `Directory.Packages.props`; `TreatWarningsAsErrors` ligado (warning quebra o build).
- Toda configuração do EF fica no `WebAPIContext` (`OnConfiguring`/`OnModelCreating`), não no `Program`. Connection string montada a partir da seção `Database` (Host, Port, Name, Username, Password).
- Segredos por user-secrets (`dotnet user-secrets set ... --project src/backend/WebAPI`): `Database:Password`, `Jwt:SigningKey` (32+ caracteres) e `Admin:Password` (cria o primeiro admin).
- Auditoria por atributo: `[Audit(AuditOperations.Create | Update | Delete)]` na entidade e `[AuditIgnore]` na propriedade.
- Login: sessão no banco + JWT de 15 min no `Authorization: Bearer`; o refresh token vai no corpo (`/api/auth/refresh` e `/logout` recebem `{ refreshToken }`) e é girado a cada uso. Erros em ProblemDetails. Contrato em `src/backend/README.md`.
- Nova migration: `dotnet ef migrations add <Nome> --project src/backend/WebAPI --output-dir Data/Migrations`.
- Documentação da API em `/docs` (Scalar) quando `Docs:Enabled` (padrão em Development).

## Regras que não se quebram

- Todo dado tem dono (`OwnerId`); os filtros globais de dono e de soft delete só são desligados em contexto de sistema, de forma explícita.
- Nunca apague de verdade: remoção vira soft delete.
- Endpoints e o chat chamam serviços de aplicação; nunca o `DbContext` direto.
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
