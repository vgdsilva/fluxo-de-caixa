# apps/api/src

```text
main.ts            sobe o servidor HTTP (o "server" do modelo); prefixo /api
app.module.ts      compõe a aplicação (o "app" do modelo): config + módulos
config/            variáveis de ambiente validadas com zod no start
common/            peças transversais, sem regra de negócio
  constants/  guards/  interceptors/  middleware/  validators/  utils/
modules/           uma pasta por funcionalidade (módulo Nest)
  health/            health.module.ts, health.controller.ts, health.service.ts
  auth/              (fatia 4) sessão, login, convites, 2FA
  audit/             (fatia 3) auditoria
database/          (fatia 2) MikroORM: config, entidade base, filtros, subscribers, migrations
```

Dentro de cada módulo, os arquivos seguem o sufixo do papel: `*.controller.ts` (rotas), `*.service.ts` (casos de uso), `*.entity.ts` (modelos do banco, fatia 2), `*.module.ts` (registro no DI). Quando um módulo crescer, os arquivos vão para subpastas `controllers/`, `services/` e `entities/` dentro dele.

Regras: controller só chama service; um módulo só usa de outro o que está em `exports`; formatos de request/response vêm de `@fluxo/contracts`.
