# Guia rápido: Nest e Vue para quem vem do .NET

Só o que não é óbvio, com o equivalente em ASP.NET Core quando existe. Cresce a cada fatia.

## NestJS (apps/api)

| Nest                                            | Equivalente .NET                                                                                                                   | Onde aparece                               |
| ----------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------ |
| `main.ts` com `NestFactory.create(AppModule)`   | `Program.cs` com `WebApplication.CreateBuilder`                                                                                    | `apps/api/src/main.ts`                     |
| Módulo (`@Module`)                              | Um `IServiceCollection` por feature (`AddIdentity()`), mas com fronteira: só o que está em `exports` é visível para outros módulos | `app.module.ts`, `health/health.module.ts` |
| `providers` de um módulo                        | Registro no container de DI (`services.AddScoped<...>`)                                                                            | a partir da fatia 2                        |
| Controller (`@Controller('health')` + `@Get()`) | Controller com `[Route]`/`[HttpGet]`; o retorno é serializado como JSON                                                            | `health/health.controller.ts`              |
| `app.setGlobalPrefix('api')`                    | `app.MapGroup("/api")` / `PathBase`                                                                                                | `main.ts`                                  |
| `ConfigModule` + `validate`                     | `IOptions<T>` com `ValidateOnStart()`                                                                                              | `config/env.ts`                            |
| Guard (`CanActivate`)                           | Authorization filter / policy                                                                                                      | fatia 4                                    |
| Interceptor                                     | Middleware ou action filter em volta da action                                                                                     | fatia 3 (CorrelationId)                    |
| Pipe (validação)                                | Model binding + validação (`[ApiController]`)                                                                                      | fatia 4 (`nestjs-zod`)                     |

Detalhes que pegam:

- **Decorators precisam de metadata de tipo.** Por isso o `tsconfig` da API liga `experimentalDecorators` e `emitDecoratorMetadata`. É assim que o Nest descobre, pelo tipo do parâmetro do construtor, o que injetar (como o container do .NET faz por reflexão).
- **A API roda em CommonJS**, não ESM. O pacote `@fluxo/contracts` é gerado nos dois formatos para servir ao Nest e ao Vite.
- **`nest start --watch`** compila para `dist/` e reinicia o processo a cada mudança (parecido com `dotnet watch`).
- **Escopo padrão dos providers é singleton.** Dados da requisição (usuário atual) não vão em campos de serviço; vão no `RequestContext` do MikroORM ou em `AsyncLocalStorage` (fatia 2).

## Vue 3 (apps/web)

- **`<script setup>`**: o código do componente roda uma vez por instância; tudo que é declarado ali fica disponível no template. É o equivalente ao code-behind de um componente Blazor.
- **`ref()`**: valor reativo. No script se lê/escreve com `.value`; no template o Vue desembrulha sozinho. Mudou o `.value`, a tela atualiza.
- **Pinia**: store global (estado compartilhado entre telas), como um serviço singleton com estado no Blazor. Instalado, ainda sem stores.
- **Vue Router**: rotas apontam para componentes de página carregados sob demanda (`() => import(...)`), o que gera um arquivo JS por página.
- **PrimeVue**: biblioteca de componentes (botão, tabela, formulário). Importe cada componente onde usar (`import Button from 'primevue/button'`). Fixado no 4.x, ver ADR 0008.
- **Alias `@/`** aponta para `apps/web/src`.
- **Proxy do Vite**: em dev, `fetch('/api/...')` no navegador vai para o Vite (:5173), que repassa ao Nest (:3000). Em produção o Nest serve tudo, então o código do front não muda.
