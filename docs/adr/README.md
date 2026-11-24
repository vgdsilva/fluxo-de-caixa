# ADRs

Registros curtos de decisão (contexto, decisão, consequências). Uma decisão nova de arquitetura ganha um ADR antes do código; para mudar uma decisão, crie um ADR novo que substitui o antigo.

| #                                     | Decisão                                                | Situação                         |
| ------------------------------------- | ------------------------------------------------------ | -------------------------------- |
| [0001](0001-monorepo-pnpm.md)         | Monorepo pnpm, um container em produção, mesma origem  | substituída pela 0011            |
| [0002](0002-postgres-mikroorm.md)     | PostgreSQL com MikroORM                                | Postgres vale; ORM vira EF Core  |
| [0003](0003-vue-spa-pwa-nestjs.md)    | Vue 3 SPA + PWA no front, NestJS no back               | front vale; back vira .NET       |
| [0004](0004-dados-por-usuario.md)     | Dados por usuário, sem tenant                          | vale                             |
| [0005](0005-autenticacao-propria.md)  | Convite, 2FA obrigatório, sessão em cookie, sem e-mail | sessão vira JWT + sessão no banco (0010) |
| [0006](0006-tailscale-funnel.md)      | Acesso pelo Tailscale Funnel                           | vale                             |
| [0007](0007-contratos-zod.md)         | Contratos zod compartilhados                           | histórico (stack Nest)           |
| [0008](0008-versoes-fixadas.md)       | Versões fixadas: Nest 11, TypeScript 5.9, PrimeVue 4.5 | histórico (stack Nest)           |
| [0009](0009-shadcn-vue.md)            | shadcn-vue + Tailwind no lugar do PrimeVue             | histórico (front antigo)         |
| [0010](0010-backend-dotnet.md)        | Back-end em C# / .NET 10, EF Core, JWT + sessão no banco | vale                           |
| [0011](0011-estrutura-monorepo.md)    | Estrutura do monorepo: src, tests, docs, deploy        | vale                             |
| [0012](0012-estrutura-backend.md)      | Back-end: pastas por tipo, um arquivo por endpoint (minimal API) | proposta              |
