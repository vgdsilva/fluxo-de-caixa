# 0011 Estrutura do monorepo: src, tests, docs, deploy

**Status:** aceita (23/11/2026). Substitui o ADR 0001 (monorepo pnpm). Os ADRs 0003, 0007, 0008 e 0009 ficam como histórico da stack Nest/pnpm.

**Contexto.** O back-end passou para .NET (ADR 0010) e o front-end vai ser refeito em Vue sobre outra base. O layout antigo (`apps/api`, `apps/web`, `packages/contracts`, workspace pnpm na raiz) existia para compartilhar TypeScript entre front e back, o que deixou de fazer sentido. Um repositório com duas stacks precisa de pastas que digam o papel de cada coisa, não a ferramenta.

**Decisão.** Um repositório só, organizado assim:

- `src/backend` solução .NET (projetos de produção, `global.json`, `Directory.*.props`).
- `src/frontend` projeto Vue autocontido, com seu próprio `package.json` e lockfile. Sem workspace na raiz.
- `tests/` projetos de teste .NET (ex.: `tests/WebAPI.Test`, incluído na solução de `src/backend`) e, no futuro, `tests/e2e`. Testes de unidade do front ficam junto do código em `src/frontend`.
- `docs/` plano, ADRs, guias e roteiros de verificação.
- `deploy/` docker-compose, `.env.example` e scripts de operação.
- `archive/` código de stacks abandonadas, só para consulta, fora da CI.
- Na raiz só o que vale para tudo: `README.md`, `CLAUDE.md`, `.gitignore`, `.gitattributes`, `.editorconfig` e `.github/`.

A stack antiga (Nest, MikroORM, contratos zod, `apps/web` e a configuração do workspace pnpm) vai para `archive/stack-nest`. As fatias 2 a 6 dela continuam na branch `feature/new-rebrand`.

**Consequências.**

- A CI tem um job por stack (`backend` e `frontend`), cada um só roda quando a pasta tem código.
- Sem contratos compartilhados por código: o formato da API passa a vir do OpenAPI do back-end. Gerar o cliente do front a partir dele pede um ADR próprio.
- Um `.editorconfig` e um `.gitignore` únicos na raiz, com seções para .NET e Node; as pastas não repetem esses arquivos.
