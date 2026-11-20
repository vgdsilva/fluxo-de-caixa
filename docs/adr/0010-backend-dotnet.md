# 0010 Back-end em C# / .NET 10 com EF Core

**Status:** aceita (20/11/2026). Substitui a parte de back-end do ADR 0003 (NestJS) e o ORM do ADR 0002 (MikroORM). O PostgreSQL continua.

**Contexto.** O dono é desenvolvedor .NET. Manter o back em Nest exige aprender outro ecossistema para a parte que mais concentra regra de negócio (projeção, parcelas, auditoria), e o ganho de compartilhar tipos com o front não compensa esse custo.

**Decisão.** Back-end em C# / .NET 10 (ASP.NET Core com controllers, EF Core 10 + Npgsql), em `apps/backend`, organizado em camadas:

- `WebAPI.Common` contratos, exceções, opções (sem dependência de projeto)
- `WebAPI.Data` `WebAPIContext`, entidades, mapeamentos EF, migrations
- `WebAPI.Framework` background services, fila, segurança, middlewares, extensões de DI
- `WebAPI.Core` regras de negócio, handlers de mensagens, webhooks, validadores (FluentValidation)
- `WebAPI` host HTTP; `WebAPI.Worker` host opcional só de background; `WebAPI.Test` xUnit

Mensageria pelo padrão Transactional Outbox (outbox/inbox no banco, fila em memória por enquanto). Versões centralizadas em `Directory.Packages.props`.

**Consequências.**

- `apps/api` (Nest) continua no repositório até o back em .NET cobrir o que ele faz hoje (health); a remoção é um PR à parte.
- Os contratos zod (ADR 0007) deixam de ser a fonte única do formato da API. O caminho natural é gerar o cliente do front a partir do OpenAPI da API .NET; isso pede um ADR próprio.
- As garantias do ADR 0002 (dono, soft delete, auditoria) passam para EF Core: query filters globais e interceptor de `SaveChanges`.
