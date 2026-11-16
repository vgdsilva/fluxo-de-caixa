# 0001 Monorepo pnpm, um container, mesma origem

**Status:** aceita (16/11/2026)

**Contexto.** Front e back em TypeScript, poucos usuários, um notebook velho como servidor. Queremos compartilhar tipos entre front e back sem publicar pacotes.

**Decisão.** Monorepo com pnpm workspaces: `apps/web`, `apps/api` e `packages/contracts`. Em produção, um único container: o NestJS serve a API em `/api` e o build estático do Vue no resto. Em desenvolvimento, o Vite repassa `/api` para o Nest (proxy), imitando a mesma origem.

**Consequências.** Sem CORS e sem truques de cookie entre domínios. Um `pnpm install` na raiz instala tudo e o lockfile fixa as versões. `contracts` precisa ser compilado antes dos apps (o script `pnpm dev`/`pnpm build` da raiz já faz isso).
