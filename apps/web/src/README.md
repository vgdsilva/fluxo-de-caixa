# apps/web/src

```text
main.ts              cria o app Vue, registra Pinia e router
App.vue              raiz: layout + <RouterView>
assets/styles/       CSS global (Tailwind + tema do shadcn)
components/
  ui/                componentes do shadcn-vue (gerados pelo CLI, ver ADR 0009)
  common/            componentes nossos reaproveitáveis (ex.: ApiStatus)
  layout/            casca das telas (AppLayout)
pages/               uma tela por rota (pages/auth/ na fatia 4, pages/admin/ na fatia 6)
composables/         lógica reativa reaproveitável, o "hook" do Vue (useApiHealth)
services/api/        chamadas HTTP; http.ts valida respostas com @fluxo/contracts
stores/              stores do Pinia (estado global)
router/              rotas e guardas de navegação
utils/               funções puras (cn para classes do Tailwind)
```

Regras: página monta componentes; componente não chama `fetch`, usa um composable; composable chama `services/api`; tipos de API só de `@fluxo/contracts`.
