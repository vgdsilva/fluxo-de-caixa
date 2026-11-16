# 0009 shadcn-vue + Tailwind no lugar do PrimeVue

**Status:** aceita (16/11/2026). Substitui a escolha de PrimeVue da spec da fase 0 e a parte do PrimeVue no ADR 0008.

**Contexto.** O PrimeVue 5 passou a exigir chave de licença, e o dono do projeto preferiu o shadcn.

**Decisão.** Componentes com [shadcn-vue](https://www.shadcn-vue.com) (porte do shadcn/ui para Vue, MIT), estilizados com Tailwind CSS 4 (`@tailwindcss/vite`). Diferente de uma biblioteca instalada, o shadcn **copia o código-fonte dos componentes para o projeto** (`apps/web/src/components/ui/`), em cima do Reka UI (componentes sem estilo e acessíveis) e do `class-variance-authority` para as variantes. Ícones com `@lucide/vue`. Fonte Geist servida localmente (`@fontsource-variable/geist`), sem Google Fonts, para o PWA abrir sem sinal.

**Consequências.**

- Novo componente: `pnpm dlx shadcn-vue@latest add <nome>` dentro de `apps/web` (config em `apps/web/components.json`).
- Os arquivos em `components/ui/` são nossos e podem ser editados, mas evite mudanças grandes para não perder a facilidade de regenerar. O lint relaxa duas regras de nomes/props só nessa pasta.
- Tema por variáveis CSS em `apps/web/src/shared/styles.css` (cor base neutral, modo escuro pela classe `.dark`).
- Tabelas e formulários mais ricos (fatia 6 em diante) usam os componentes `table`, `form` e `data-table` do shadcn, com TanStack Table quando precisar de ordenação e filtro.
