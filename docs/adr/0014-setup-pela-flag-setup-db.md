# 0014 Primeiro admin pela configuração SETUP_DB

**Status:** aceita (17/11/2026). Substitui a parte "primeiro admin por link no log" do ADR 0013.

**Contexto.** O dono do projeto quer que, ao abrir o app pela primeira vez, o sistema consulte no banco uma configuração `SETUP_DB` (0 ou 1) e, se for 0, abra a tela de setup para criar o admin. O usuário admin não deve ser criado por migration.

**Decisão.**

- **Tabela `system_settings`** (chave/valor, entidade `SystemSetting` no módulo `settings`). A migration que cria a tabela só grava a flag: `SETUP_DB = 1` se já existirem usuários, `0` se o banco for novo. Nenhuma migration cria usuário.
- **`GET /api/setup/status`** (público) responde `{ required: true }` enquanto `SETUP_DB` não for `1`.
- **`POST /api/setup`** (público, só enquanto `required`) cria o admin e marca `SETUP_DB = 1` na mesma transação, com a linha da flag travada (`SELECT ... FOR UPDATE`), então dois envios simultâneos não criam dois admins. Depois disso as duas rotas respondem `required: false` / 404. Auditoria: `create` do `User` (automática) e `setup_completed`.
- **Front:** a guarda de rotas consulta o status uma vez; com setup pendente, qualquer tela leva a `/setup`; sem setup pendente, `/setup` leva ao login. Depois de criar o admin, o login segue pela tela de sessão (feed).
- O seed de desenvolvimento não cria usuários (desde 17/11/2026); o admin vem sempre da tela de setup.
- Sai o link de uso único no log (`FirstAdminService`).

**Consequências.** Sem o token do log, quem abrir o app primeiro num banco novo pode criar o admin. Por isso o setup deve ser feito antes de publicar o app no Funnel (passo do checklist da fatia 8). A tabela `system_settings` serve para outras configurações do sistema no futuro (ex.: teto de custo da IA).
