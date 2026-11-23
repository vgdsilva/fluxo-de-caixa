# src/frontend

SPA em Vue 3 (PWA), projeto autocontido: tem seu próprio `package.json`, lockfile e configuração de lint e formatação. Fala com o back-end só por HTTP; nunca importa nada de `src/backend`.

O código chega no PR do front-end. Testes de unidade e de componente ficam junto do código (`*.spec.ts`); testes ponta a ponta ficam em [`tests/e2e`](../../tests/README.md).

## Comandos

```bash
cd src/frontend
npm install        # ou pnpm install, conforme o lockfile do projeto
npm run dev
npm run build
```

Em desenvolvimento, o Vite repassa `/api` para a API local, imitando a mesma origem de produção.
