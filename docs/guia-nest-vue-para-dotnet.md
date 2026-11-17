# Guia rápido: Nest e Vue para quem vem do .NET

Só o que não é óbvio, com o equivalente em ASP.NET Core quando existe. Cresce a cada fatia.

## NestJS (apps/api)

| Nest                                                       | Equivalente .NET                                                                                                                   | Onde aparece                                       |
| ---------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------- |
| `@Injectable()` num service + `providers: [...]` no módulo | `services.AddSingleton<HealthService>()`                                                                                           | `modules/health/health.service.ts`                 |
| `main.ts` com `NestFactory.create(AppModule)`              | `Program.cs` com `WebApplication.CreateBuilder`                                                                                    | `apps/api/src/main.ts`                             |
| Módulo (`@Module`)                                         | Um `IServiceCollection` por feature (`AddIdentity()`), mas com fronteira: só o que está em `exports` é visível para outros módulos | `app.module.ts`, `modules/health/health.module.ts` |
| `providers` de um módulo                                   | Registro no container de DI (`services.AddScoped<...>`)                                                                            | `modules/notes/notes.module.ts`                    |
| Controller (`@Controller('health')` + `@Get()`)            | Controller com `[Route]`/`[HttpGet]`; o retorno é serializado como JSON                                                            | `modules/health/health.controller.ts`              |
| `app.setGlobalPrefix('api')`                               | `app.MapGroup("/api")` / `PathBase`                                                                                                | `main.ts`                                          |
| `ConfigModule` + `validate`                                | `IOptions<T>` com `ValidateOnStart()`                                                                                              | `config/env.ts`                                    |
| Guard (`CanActivate`)                                      | Authorization filter / policy                                                                                                      | fatia 4                                            |
| Interceptor                                                | Middleware ou action filter em volta da action                                                                                     | fatia 3 (CorrelationId)                            |
| Pipe (validação)                                           | Model binding + validação (`[ApiController]`)                                                                                      | `ZodValidationPipe` global em `app.module.ts`      |
| `MikroOrmModule` + `EntityManager` injetado                | `AddDbContext` + `DbContext` scoped                                                                                                | `database/database.module.ts`                      |
| `em.flush()`                                               | `SaveChanges()`                                                                                                                    | services                                           |
| Filtros globais do MikroORM                                | `HasQueryFilter`                                                                                                                   | `database/filters.ts`                              |
| `EventSubscriber.onFlush`                                  | `SaveChangesInterceptor`                                                                                                           | `database/subscribers/`                            |
| `AsyncLocalStorage`                                        | `AsyncLocal<T>`                                                                                                                    | `common/context/current-user.context.ts`           |
| `createZodDto(schema)` + `ZodValidationPipe`               | DTO com DataAnnotations + validação automática                                                                                     | `modules/notes/notes.dto.ts`                       |

Detalhes que pegam:

- **Decorators precisam de metadata de tipo.** Por isso o `tsconfig` da API liga `experimentalDecorators` e `emitDecoratorMetadata`. É assim que o Nest descobre, pelo tipo do parâmetro do construtor, o que injetar (como o container do .NET faz por reflexão).
- **A API roda em CommonJS**, não ESM. O pacote `@fluxo/contracts` é gerado nos dois formatos para servir ao Nest e ao Vite. O MikroORM 7 é só ESM e entra pelo `require` de ESM do Node 22 (ADR 0011).
- **`Opt<T>` nas entidades**: marca campos com valor padrão (id, datas, versão, dono) como opcionais no `em.create(...)`. Sem isso o TypeScript exige que você passe tudo.
- **`nest start --watch`** compila para `dist/` e reinicia o processo a cada mudança (parecido com `dotnet watch`).
- **Escopo padrão dos providers é singleton.** Dados da requisição (usuário atual) não vão em campos de serviço; vão no `RequestContext` do MikroORM ou em `AsyncLocalStorage` (fatia 2).

## Vue 3 (apps/web)

- **`<script setup>`**: o código do componente roda uma vez por instância; tudo que é declarado ali fica disponível no template. É o equivalente ao code-behind de um componente Blazor.
- **`ref()`**: valor reativo. No script se lê/escreve com `.value`; no template o Vue desembrulha sozinho. Mudou o `.value`, a tela atualiza.
- **Hooks** (`src/hooks/useX.ts`): no Vue o nome oficial é _composable_, o equivalente dos hooks do React; aqui a pasta se chama `hooks/` por convenção do projeto. Uma função que cria estado reativo (`ref`) e lógica, chamada dentro do `<script setup>`. É onde mora a chamada à API, para o componente só desenhar.
- **Pinia** (`src/store/`): store global (estado compartilhado entre telas), como um serviço singleton com estado no Blazor. Instalado, ainda sem stores.
- **Context** (`src/context/`): `provide`/`inject` do Vue, o mesmo papel do Context do React (dado para uma subárvore de componentes, sem ser global).
- **Vue Router** (`src/routes/`): rotas apontam para componentes de página carregados sob demanda (`() => import(...)`), o que gera um arquivo JS por página.
- **shadcn-vue**: não é uma dependência que você atualiza. O CLI copia o código de cada componente para `src/components/ui/` e ele passa a ser seu (como gerar um componente a partir de um template). Uso: `import { Button } from '@/components/ui/button'`. Ver ADR 0009.
- **Tailwind**: estilo por classes utilitárias direto no template (`class="mb-2 text-2xl font-semibold"`), em vez de um arquivo CSS por componente. As cores do tema são variáveis CSS em `src/assets/styles/main.css` (`bg-background`, `text-muted-foreground`).
- **Alias `@/`** aponta para `apps/web/src`.
- **Proxy do Vite**: em dev, `fetch('/api/...')` no navegador vai para o Vite (:5173), que repassa ao Nest (:3000). Em produção o Nest serve tudo, então o código do front não muda.
