# 0012 Back-end por módulos, com um arquivo por endpoint em minimal API

**Status:** proposta (24/11/2026). Ajusta a organização de pastas do ADR 0010; o resto dele continua valendo.

**Contexto.** O `WebAPI` estava organizado por tipo de arquivo (`Controllers`, `Core/Services`, `Data/Types`...), então mexer numa funcionalidade, como o login, pedia abrir seis pastas. No projeto Nest, cada funcionalidade era uma pasta em `modules/` com tudo dela, e isso funcionava bem.

**Decisão.**

- Pastas por papel: `Config/` (startup), `Common/` (o que é de todos: segurança, middleware, utilitários, infraestrutura de endpoint), `Database/` (contexto, entidade base, auditoria, migrations) e `Modules/<Nome>/` (uma funcionalidade).
- Cada módulo tem um `<Nome>Module.cs` com `Add<Nome>Module(services)` (registra os serviços) e `Map<Nome>Module(api)` (cria o grupo de rotas e lista os endpoints), como o `*.module.ts` e o `@Controller` do Nest. Entidades, services e opções do módulo ficam na pasta dele.
- Endpoints em minimal API, um arquivo por endpoint em `Modules/<Nome>/Endpoints/`. O arquivo é uma classe estática com o nome da ação (`Login`) e as subclasses `Request`, `Response`, `Validation` (FluentValidation) e `Endpoint` (implementa `IEndpoint`). Quem não tem corpo ou validação omite a subclasse.
- Todo endpoint fica sob o grupo `/api`, que exige usuário autenticado; rota pública usa `.AllowAnonymous()`. A validação roda num filtro (`.WithValidation<Request>()`) e responde 400 em ProblemDetails.
- O endpoint chama o service do módulo; nunca o `DbContext` direto.

**Consequências.**

- Sem MVC: nada de controllers, `[ApiController]` nem filtros MVC. Os equivalentes são filtros de endpoint (`IEndpointFilter`).
- No OpenAPI, tipos aninhados ganham o nome do endpoint na frente (`Login.Request` vira `LoginRequest`), para os vários `Request` não colidirem.
- Endpoint novo precisa ser listado no `Map<Nome>Module`, como um controller era listado no módulo Nest.
- O registro de entidades procura toda classe que herda de `Entity` no projeto, em qualquer pasta. A mudança de pastas e namespaces não altera o banco (a migration gerada para conferir saiu vazia).
