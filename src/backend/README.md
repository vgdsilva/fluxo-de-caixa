# src/backend

API em C# / .NET 10 (ASP.NET Core + EF Core + PostgreSQL). Decisões no [ADR 0010](../../docs/adr/0010-backend-dotnet.md). Os projetos de teste ficam em [`tests/`](../../tests/README.md) quando existirem.

```text
src/backend/
  WebAPI.slnx                 solução
  global.json                 versão do SDK
  Directory.Build.props       propriedades comuns (nullable, TreatWarningsAsErrors)
  Directory.Packages.props    versões de pacote centralizadas
  nuget.config                só o nuget.org (ignora fontes da máquina, evita o NU1507)
  WebAPI/
    Program.cs
    Config/                   startup (serviços, pipeline, Kestrel, log) e Options/
    Endpoints/                um arquivo por endpoint: Auth/Login.cs, Auth/Refresh.cs, Me/GetMe.cs...
    Dtos/                     formatos compartilhados entre endpoints (UserDto)
    Entities/                 Entity (base), User, Session, Invite, Setting, AuditLog
    Enums/                    UserRole
    Exceptions/               AppException e as exceções de negócio (já com status HTTP)
    Security/                 JWT, sessão, auth handler, UserInfo
    Middlewares/              tratamento de erro, espera do startup
    Utils/                    extensões, mapeamento, infraestrutura de endpoint, startup
    Data/                     WebAPIContext, [Audit], AuditInterceptor, Migrations, Seeders
```

## Endpoint novo

Um arquivo em `Endpoints/<Área>/` (ADR 0012). Ele é achado sozinho e registrado sob `/api`:

```csharp
public static class CreateThing
{
    public sealed record Request(string Name);
    public sealed record Response(Guid Id);

    public sealed class Validation : AbstractValidator<Request>
    {
        public Validation() => RuleFor(r => r.Name).NotEmpty().MaximumLength(120);
    }

    public sealed class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder api) =>
            api.MapPost("/things", Handle).WithTags("Things").WithValidation<Request>();

        private static async Task<Created<Response>> Handle(Request request, WebAPIContext db, CancellationToken ct) { ... }
    }
}
```

Tudo em `/api` exige login; use `.AllowAnonymous()` no que for público. Erro de negócio: lance uma exceção que herda de `AppException` (ex.: `InvalidCredentialsException`), que vira ProblemDetails com o status dela.

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

A API aplica as migrations ao subir e cria o admin (login `admin`, e-mail `admin@local.test` em Development) se ainda não houver usuário. A documentação fica em <http://localhost:5039/docs>.

## Autenticação

Erros sempre em ProblemDetails (`{ type, title, status, traceId }`; validação traz `errors`). JSON em camelCase.

| Endpoint                 | Corpo                           | Resposta                                                |
| ------------------------ | ------------------------------- | ------------------------------------------------------- |
| `POST /api/auth/login`   | `{ "login", "password" }`       | `AuthResponse`; 401 se errar (o e-mail também serve de login) |
| `POST /api/auth/refresh` | `{ "refreshToken" }`            | `AuthResponse` com refresh token novo; o antigo deixa de valer; 401 se a sessão expirou |
| `POST /api/auth/logout`  | `{ "refreshToken" }`            | 204, sessão revogada                                    |
| `GET /api/me`            | `Authorization: Bearer <token>` | `{ id, name, login, email, role }`                      |

`AuthResponse` = `{ accessToken, accessTokenExpiresAt, refreshToken, refreshTokenExpiresAt, user: { id, name, login, email, role } }`.

O JWT vale 15 minutos (`Jwt:AccessTokenMinutes`); a sessão expira após 7 dias sem uso ou 30 dias no total (seção `Session`). O banco guarda só o hash SHA-256 do refresh token, na tabela `session`.

## Comandos

```bash
dotnet build src/backend/WebAPI.slnx
dotnet run --project src/backend/WebAPI
dotnet ef migrations add <Nome> --project src/backend/WebAPI --output-dir Data/Migrations
```
