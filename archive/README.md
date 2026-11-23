# archive

Código de stacks abandonadas, guardado só para consulta. Nada aqui é compilado, testado pela CI ou mantido.

## stack-nest/

A primeira versão do sistema (fatia 1 da fase 0), em TypeScript, como estava no `develop` em 23/11/2026:

```text
stack-nest/
  apps/api            NestJS 11 + MikroORM (API em /api, health)
  apps/web            Vue 3 + Vite + Pinia + Tailwind + shadcn-vue
  packages/contracts  schemas zod compartilhados
  package.json, pnpm-*, eslint, prettier, tsconfig.base.json, .nvmrc  configuração do workspace pnpm
```

As fatias 2 a 6 dessa stack (persistência, login, convites, perfil, admin) nunca entraram no `develop` e estão na branch `feature/new-rebrand`.

Para rodar de novo, copie a pasta para fora do repositório e siga o README antigo (`pnpm install` e `pnpm dev` dentro dela). Os ADRs e docs dessa época continuam em `docs/` (ADRs 0001, 0003, 0007, 0008 e 0009, `docs/fase-0.md`).
