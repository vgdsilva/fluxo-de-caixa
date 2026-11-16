# 0002 PostgreSQL com MikroORM

**Status:** aceita (16/11/2026)

**Contexto.** Toda entidade precisa de dono, soft delete e auditoria, e isso não pode depender de cada serviço lembrar. O dono vem do .NET e conhece EF Core.

**Decisão.** PostgreSQL com MikroORM. Ele tem unit of work e change tracking como o EF Core, filtros globais (equivalente aos query filters do EF) e subscribers de flush (equivalente a interceptors do `SaveChanges`). Migrations do próprio MikroORM, aplicadas no start.

**Consequências.** As três garantias (dono, soft delete, auditoria) ficam na camada de persistência (fatias 2 e 3). Desenvolvimento roda contra o Postgres local; produção em container (fatia 8).
