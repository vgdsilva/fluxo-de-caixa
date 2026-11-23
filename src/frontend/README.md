# src/frontend

SPA em Vue 3, projeto autocontido: tem seu próprio `package.json` e lockfile. Fala com o back-end só por
HTTP; nunca importa nada de `src/backend`. A arquitetura segue o `frontVue` do CloverCRED (só como
referência; nada é importado de lá).

## Comandos

```bash
cd src/frontend
cp .env.example .env   # uma vez
npm install
npm run dev            # porta 8080; /api vai para http://localhost:5039 (a API de src/backend)
npm run dev:mock       # sem back-end: API falsa em mock/devMockApi.js (usuários de teste no topo do arquivo)
npm run build
```

Em desenvolvimento, o Vite repassa `/api` para a API local, imitando a mesma origem de produção.

## Arquitetura (igual à referência)

- **Options API + mixins.** Mixin global em `src/viewsMixins/global/globalMixin.js`.
- **Vuex com módulos namespaced** em `src/store/` (`main`, `ItensMenuModule`, `ViewCacheModule`,
  `ViewParametersModule`). Ações de sistema na raiz: `bootstrap`, `loginSuccessful`, `loadSystem`, `logout`.
- **Rotas** em `src/router/`: layout `Main` em `/index/...` (exige login) e `Auth` em `/auth/...`.
  Cada módulo tem seu arquivo em `src/router/modules/` e entra em `modules/index.js`.
- **HTTP só pelos helpers** de `src/services/requestHelper.js` (`doFindRangeRequest`, `doGetByIdRequest`,
  `doCreateRequest`, `doEditRequest`, `doRemoveRequest`, `doGetRequest`, `doPostRequest`, `doPutRequest`),
  que usam o `apiClient()` de `apiClientProvider.js`. Não chame axios direto.
- **Services** em `src/services/<modulo>/XxxService.js` com `relativePath`; **entidades** em
  `src/entities/<modulo>/Xxx.js` com `XxxFactory.create()`.
- **Permissões** com CASL (`<Can>`, `$can`) em `src/services/ability.js`.
- **Componentes** próprios em `src/components/` (no lugar do `components-builder`), exportados em
  `src/index.js` e registrados globalmente em `main.js`: `Button`, `Input`, `InputPassword`, `Select`,
  `Checkbox`, `Card`, `Table`, `ModalCenter`, `InlineNotification`, `Empty`, `InlineSpinner`, `Icon`, `Logo`.
- **Utilitários** em `src/serviceUtils.js` (formatar dinheiro/data, toasts, bloqueio de tela).
- **Visual:** fonte Quicksand (local, sem Google Fonts) e cores só pelos tokens de
  `src/assets/styles/main.css`.

## Login e sessão

Sessão no banco + JWT curto renovado automaticamente. Contrato em
[docs/contrato-autenticacao.md](../../docs/contrato-autenticacao.md); lógica em
`src/system/authSessionHelper.js` (renovação agendada, renovação no 401, sincronia entre abas).

A tela de login segue o redesign: entrar com e-mail → senha → "Validando sua sessão…" → Início, ou erro com
"Tentar novamente".

## O que ficou de fora da referência

Vistra, NPS, Movidesk, Clarity, mapas, multiempresa/contexto vigente, telemetria, websocket, OIDC/BFF e o
código gerado pelo ArquiteturaGerador.
