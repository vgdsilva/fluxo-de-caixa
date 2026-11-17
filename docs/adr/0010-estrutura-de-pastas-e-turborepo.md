# 0010 Estrutura de pastas por camada e Turborepo

**Status:** aceita (17/11/2026), revisada no mesmo dia para usar no web os nomes exatos do modelo. Ajusta a árvore da seção "Estrutura da solução" de `docs/fase-0.md`.

**Contexto.** O dono do projeto trouxe modelos de arquitetura Node que usa como referência: monorepo com pnpm workspace e Turborepo; back com `config/`, `controllers/`, `services/`, `models/`, `middleware/`, `validators/`, `utils/`, `constants/` e app separado do server; front com `assets/`, `components/` (common, ui, layout), `pages/`, `hooks/`, `services/api`, `store/`, `routes/` e `utils/`. Pediu também que funcionalidades como auth fiquem dentro de uma pasta `modules`.

**Decisão.**

- **Raiz:** Turborepo orquestra `dev`, `build`, `lint`, `typecheck` e `test` (`turbo.json`), respeitando a ordem de dependência (o `contracts` compila antes dos apps).
- **API (`apps/api/src`):** `main.ts` (server) e `app.module.ts` (app); `config/`; `common/` com `constants/`, `guards/`, `interceptors/`, `middleware/`, `validators/` e `utils/`; `modules/<feature>/` com `*.module.ts`, `*.controller.ts`, `*.service.ts` e, a partir da fatia 2, `*.entity.ts` (os "models"); `database/` para o MikroORM na fatia 2. Auth vira `modules/auth/` (fatia 4).
- **Web (`apps/web`):** `public/` com `images/`, `icons/` e `favicon.ico`; `.env` (proxy do Vite); em `src/`: `assets/`, `components/{common,ui,layout}`, `pages/`, `hooks/`, `context/`, `services/` (`api.ts` é o cliente HTTP), `utils/`, `routes/`, `store/`, `App.vue` e `main.ts`. Os nomes seguem o modelo mesmo onde o Vue usa outro termo: `hooks/` guarda composables, `context/` guarda chaves de `provide`/`inject`, `routes/` é o Vue Router e `store/` é o Pinia.

Adaptações em relação aos modelos, para não brigar com Nest e Vue:

- No back, controllers e services ficam dentro do módulo da feature, não em pastas globais: é o que o Nest espera (cada módulo registra os seus no DI) e o que mantém a regra "um módulo só usa o que o outro exporta". Quando um módulo crescer, ganha subpastas `controllers/`, `services/` e `entities/` dentro dele.
- Não criamos `apps/mobile` (o app é PWA) nem `packages/ui`, `packages/config` e `packages/utils` por enquanto: os componentes do shadcn-vue vivem no `apps/web` e não há um segundo consumidor. `packages/contracts` cumpre o papel do `packages/types`.

**Consequências.** `pnpm dev`, `pnpm build`, `pnpm lint` e `pnpm typecheck` passam pelo Turborepo, com cache local em `.turbo/`. `apps/api/src/README.md` e `apps/web/README.md` descrevem as pastas.
