# src/backend

API em C# / .NET (ASP.NET Core + EF Core + PostgreSQL). A solução (`WebAPI.slnx`) e os projetos de produção ficam aqui; os projetos de teste ficam em [`tests/`](../../tests/README.md).

O código chega no PR do back-end. Layout esperado:

```text
src/backend/
  WebAPI.slnx                 solução (inclui ../../tests/WebAPI.Test)
  global.json                 versão do SDK
  Directory.Build.props       propriedades comuns (nullable, TreatWarningsAsErrors)
  Directory.Packages.props    versões de pacote centralizadas
  WebAPI/                     projeto da API
```

## Comandos

```bash
# na raiz do repositório
docker compose -f deploy/docker-compose.yml up -d   # banco local
dotnet build src/backend/WebAPI.slnx
dotnet test src/backend/WebAPI.slnx
dotnet run --project src/backend/WebAPI
```
