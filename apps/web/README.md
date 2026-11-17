# apps/web

```text
public/                  arquivos servidos como estão (sem build)
  images/  icons/  favicon.ico
src/
  assets/styles/         CSS global (Tailwind + tema do shadcn)
  components/
    common/              componentes nossos reaproveitáveis (ex.: ApiStatus)
    ui/                  componentes do shadcn-vue (gerados pelo CLI, ver ADR 0009)
    layout/              casca das telas (AppLayout)
  pages/                 uma tela por rota (pages/auth/ na fatia 4, pages/admin/ na fatia 6)
  hooks/                 lógica reativa reaproveitável (useApiHealth); no Vue se chamam composables
  context/               estado de uma parte da árvore via provide/inject (o Context do React)
  services/              chamadas HTTP: api.ts (cliente que valida com @fluxo/contracts) + um *.service.ts por área
  utils/                 funções puras (cn para classes do Tailwind)
  routes/                rotas do Vue Router e guardas de navegação
  store/                 stores do Pinia (estado global)
  App.vue                raiz: layout + <RouterView>
  main.ts                cria o app, registra Pinia e rotas
.env / .env.example      configuração local do Vite (proxy de /api)
package.json  vite.config.ts
```

Regras: página monta componentes; componente não chama `fetch`, usa um hook; hook chama `services/`; tipos de API só de `@fluxo/contracts`.
