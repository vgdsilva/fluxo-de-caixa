# WebAPI — backend do Fluxo de Caixa

Backend em C# / .NET 10 (ASP.NET Core + EF Core com PostgreSQL).

## Pré-requisitos

- .NET SDK 10 (`dotnet --list-sdks`)
- PostgreSQL 16+ local, ou Docker: `docker compose up -d` sobe um em `localhost:5432` (usuário/senha `postgres`)
- Opcional: `dotnet tool install --global dotnet-ef` para migrations

## Comandos

| Ação              | Windows (PowerShell)                                                           | make                       |
| ----------------- | ------------------------------------------------------------------------------ | -------------------------- |
| Compilar + testar | `./build.ps1`                                                                  | `make build test`          |
| Rodar a API       | `dotnet run --project WebAPI`                                                  | `make run`                 |
| Rodar o Worker    | `dotnet run --project WebAPI.Worker`                                           | `make worker`              |
| Limpar bin/obj    | `./clean.ps1`                                                                  | `make clean`               |
| Nova migration    | `dotnet ef migrations add Nome --project WebAPI.Data --startup-project WebAPI` | `make migration NAME=Nome` |

Com a API rodando em Development:

- `GET /api/status` — saúde da API e do banco
- `GET /openapi/v1.json` — documento OpenAPI

## Configuração

- `ConnectionStrings:Default` — conexão PostgreSQL. Fora do ambiente local use `dotnet user-secrets`
  ou a variável `WEBAPI_ConnectionStrings__Default`.
- `Jwt:SigningKey` — obrigatório (mín. 32 caracteres). Em Development já vem uma chave local em
  `appsettings.Development.json`; fora dele use `dotnet user-secrets` ou variável `WEBAPI_Jwt__SigningKey`.
- `Background:Enabled` — liga os serviços de background na API. Deixe `false` se usar o `WebAPI.Worker`.
- `Webhooks:InboundSecrets` — segredos por origem para webhooks recebidos.

## Estrutura

Veja [Docs/arquitetura.md](Docs/arquitetura.md).

## Testes

Os testes de banco rodam contra um PostgreSQL real e são pulados se a variável
`WEBAPI_TEST_POSTGRES` não estiver definida. Exemplo (PowerShell):

```powershell
$env:WEBAPI_TEST_POSTGRES = "Host=localhost;Username=postgres;Password=postgres"
dotnet test
```

## Namespace

Os namespaces seguem o nome do projeto (`WebAPI.Core`, `WebAPI.Data`...). Para aplicar um prefixo
de empresa/produto depois, altere o `RootNamespace` em `Directory.Build.props`.
