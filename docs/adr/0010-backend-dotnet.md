# 0010 Back-end em C# / .NET 10, EF Core, JWT + sessão no banco

**Status:** aceita (23/11/2026). Substitui a parte de back-end do ADR 0003 (NestJS), o ORM do ADR 0002 (MikroORM) e o mecanismo de sessão do ADR 0005. O PostgreSQL continua; convite e segundo fator do ADR 0005 continuam valendo.

**Contexto.** O dono é desenvolvedor .NET. Manter o back em Nest exige aprender outro ecossistema justamente na parte com mais regra de negócio (projeção, parcelas, auditoria). O dono já tinha uma base própria de API em .NET, de um projeto só, que vira o ponto de partida.

**Decisão.**

- Um projeto ASP.NET Core com controllers em `src/backend/WebAPI` (estrutura do ADR 0011), .NET 10, EF Core 10 + Npgsql. Pastas por papel: `Configuration` (startup e opções), `Controllers`, `Core` (serviços, segurança, utilitários), `Data` (contexto, entidades, migrations). Sem separar em vários projetos enquanto o tamanho não pedir.
- Toda configuração do EF fica no `WebAPIContext`. A connection string é montada a partir da seção `Database` (um campo por item); a senha vem de user-secrets ou variável de ambiente.
- Instantes em `timestamp with time zone` (UTC), sem o modo legado de timestamp do Npgsql.
- Remoção vira soft delete num interceptor de `SaveChanges`, com filtro global e índices únicos filtrados por `system_is_deleted = false`. Auditoria por atributo: `[Audit(...)]` na entidade escolhe as operações (Create, Update, Delete) e `[AuditIgnore]` exclui propriedades.
- Login com sessão no banco e JWT curto: `POST /api/auth/login` (`{ login, password }`) cria uma `Session` e devolve no corpo um JWT (HS256, 15 min) e um refresh token opaco, com as duas validades. O banco guarda só o hash SHA-256 do refresh token. `POST /api/auth/refresh` (`{ refreshToken }`) gira o refresh token e emite um JWT novo; `POST /api/auth/logout` (`{ refreshToken }`) revoga a sessão. Erros em ProblemDetails. Senha com `PasswordHasher` do ASP.NET Core Identity, bloqueio depois de 5 tentativas e rate limit no login.
- `ServiceLocator` fica disponível para código que não recebe injeção (ex.: utilitários estáticos), mas serviços novos usam injeção pelo construtor.
- Documentação OpenAPI com Scalar em `/docs`, ligada por `Docs:Enabled` (padrão só em Development).

**Consequências.**

- Revogar uma sessão corta a renovação na hora, mas o JWT já emitido vale até expirar (no máximo 15 min). Se isso incomodar, o handler pode checar a sessão a cada requisição.
- O refresh token vai no corpo, e não num cookie `HttpOnly`, porque foi o contrato que o front adotou e funciona igual fora do navegador. O custo é que o front precisa guardar o refresh token, e um XSS consegue lê-lo de onde ele estiver. A rotação limita o estrago: um token roubado e usado invalida o do dono, que cai para o login. Se isso pesar, dá para mover o refresh token para cookie sem mudar o resto.
- Sem contratos compartilhados por código: o formato da API vem do OpenAPI (`/openapi/v1.json`). Gerar o cliente do front a partir dele pede um ADR próprio.
- 2FA e convite (ADR 0005) entram em fatias próprias sobre esse modelo.
