# 0007 Contratos zod compartilhados

**Status:** aceita (16/11/2026)

**Contexto.** Tipos de API escritos à mão no front divergem do back sem ninguém perceber.

**Decisão.** `packages/contracts` (`@fluxo/contracts`) é a única fonte dos formatos de request e response, em schemas zod. A API valida com eles (`nestjs-zod`, a partir da fatia 4); o front importa os mesmos schemas e valida as respostas (`apiGet` em `apps/web/src/services/api/http.ts`) e os formulários.

**Consequências.** O pacote é compilado com tsup em ESM (para o Vite) e CommonJS (para o Nest). Mudar um contrato quebra o build de quem usa errado, que é o objetivo.
