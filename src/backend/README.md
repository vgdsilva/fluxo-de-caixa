# src/backend

API em C# / .NET 10 (ASP.NET Core + EF Core + PostgreSQL). Decisões no [ADR 0010](../../docs/adr/0010-backend-dotnet.md). Os projetos de teste ficam em [`tests/`](../../tests/README.md) quando existirem.

```text
src/backend/
  WebAPI.slnx                 solução
  global.json                 versão do SDK
  Directory.Build.props       propriedades comuns (nullable, TreatWarningsAsErrors)
  Directory.Packages.props    versões de pacote centralizadas
  WebAPI/
    Configuration/            startup, opções (Database, Jwt, Session, Admin)
    Controllers/              endpoints HTTP e contratos de request/response
    Core/                     serviços, segurança (JWT, sessões), seeders, utilitários
    Data/                     WebAPIContext, entidades, interceptor de auditoria, migrations
```

## Primeira execução

```bash
# na raiz do repositório
cp deploy/.env.example deploy/.env            # troque a senha
docker compose -f deploy/docker-compose.yml up -d

cd src/backend/WebAPI
dotnet user-secrets set Database:Password "<a senha do deploy/.env>"
dotnet user-secrets set Jwt:SigningKey "<32 ou mais caracteres aleatórios>"
dotnet user-secrets set Admin:Password "<senha do primeiro admin>"
dotnet run
```

A API aplica as migrations ao subir e cria o admin (`Admin:Email`, padrão `admin@local.test` em Development) se ainda não houver usuário. A documentação fica em <http://localhost:5039/docs>.

## Autenticação

| Endpoint                 | Entrada                         | Saída                                                                  |
| ------------------------ | ------------------------------- | ---------------------------------------------------------------------- |
| `POST /api/auth/login`   | `{ "email", "password" }`       | `{ accessToken, expiresAt, user }` + cookie `fc_refresh`; 401 se errar |
| `POST /api/auth/refresh` | cookie `fc_refresh`             | mesmo formato, cookie novo; 401 se a sessão expirou                    |
| `POST /api/auth/logout`  | cookie `fc_refresh`             | 204, sessão revogada e cookie apagado                                  |
| `GET /api/me`            | `Authorization: Bearer <token>` | `{ id, name, email, role }`                                            |

O JWT vale 15 minutos (`Jwt:AccessTokenMinutes`); a sessão expira após 7 dias sem uso ou 30 dias no total (seção `Session`).

## Comandos

```bash
dotnet build src/backend/WebAPI.slnx
dotnet run --project src/backend/WebAPI
dotnet ef migrations add <Nome> --project src/backend/WebAPI --output-dir Data/Migrations
```
