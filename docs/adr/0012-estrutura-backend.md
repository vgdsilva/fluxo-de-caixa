# 0012 Estrutura do back-end: pastas por tipo e um arquivo por endpoint (minimal API)

**Status:** proposta (24/11/2026). Ajusta a organização de pastas do ADR 0010; o resto dele continua valendo.

**Contexto.** O `WebAPI` misturava critérios (`Controllers`, `Core/Utils/...`, `Data/Types`). O dono quer o layout de um projeto Spring de referência (pastas por tipo: config, controllers, dto, entities, enums, exceptions, services, security, utils), mas com os endpoints no padrão REPR (um arquivo com Request, Response, Validation e Endpoint), sem repositories e, por enquanto, sem services: o projeto é pequeno e o dono prefere a lógica junto do endpoint.

**Decisão.**

- Pastas por tipo em `src/backend/WebAPI`: `Config/` (startup e opções), `Endpoints/<Área>/` (no lugar de controllers), `Dtos/` (formatos compartilhados entre endpoints), `Entities/`, `Enums/`, `Exceptions/`, `Security/`, `Middlewares/`, `Utils/` e `Data/` (contexto, auditoria, migrations, seeders).
- Endpoints em minimal API, um arquivo por endpoint: classe estática com o nome da ação (`Login`) e as subclasses `Request`, `Response`, `Validation` (FluentValidation) e `Endpoint` (implementa `IEndpoint`). Quem não tem corpo ou validação omite a subclasse. Os endpoints são achados por reflection e registrados sob `/api`, como o Spring acha os `@RestController`.
- Toda rota em `/api` exige usuário autenticado; rota pública usa `.AllowAnonymous()`. A validação roda num filtro (`.WithValidation<Request>()`) e responde 400 em ProblemDetails.
- Sem repositories nem services: o `Handle` do endpoint usa o `WebAPIContext` direto (no EF o `DbContext` já faz o papel do repository do JPA). O que mais de um endpoint usa vira um helper em `Security/` ou `Utils/` (ex.: `SessionManager`, `TokenManager`). Quando a mesma regra aparecer em vários endpoints, ou o chat precisar dela, ela volta para um service.
- Erro de negócio é uma exceção que herda de `AppException` e já traz o status HTTP (como o `@ResponseStatus` do Spring). O middleware de erro transforma em ProblemDetails; o endpoint não precisa de try/catch.

**Consequências.**

- Sem MVC: nada de controllers, `[ApiController]` nem filtros MVC. Os equivalentes são filtros de endpoint (`IEndpointFilter`).
- No OpenAPI, tipos aninhados ganham o nome do endpoint na frente (`Login.Request` vira `LoginRequest`), para os vários `Request` não colidirem.
- A mudança de pastas e namespaces não altera o banco (a migration gerada para conferir saiu vazia).
