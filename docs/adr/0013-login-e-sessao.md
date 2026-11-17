# 0013 Login, sessão e primeiro admin

**Status:** aceita (17/11/2026). Implementa a fatia 4 da spec; segundo fator, convites e lockout ficam na fatia 5.

**Decisão.**

- **Senha:** argon2id com `@node-rs/argon2` (binário pré-compilado, sem build nativo). Hash em `users.password_hash`, propriedade `hidden` e `lazy` (não carrega por padrão e sai como `"***"` na auditoria). Mínimo de 12 caracteres (`passwordSchema` em `@fluxo/contracts`); a lista de senhas comuns entra na fatia 5. Usuário inexistente gasta o mesmo tempo de uma verificação real.
- **Sessão no Postgres** (tabela `sessions`, entidade sem auditoria automática): token aleatório de 32 bytes no cookie `fluxo_sid` (`HttpOnly`, `SameSite=Lax`, `Secure` em produção, `Path=/`); no banco só o SHA-256 do token. Expiração deslizante de 7 dias, renovada no máximo a cada 5 minutos. Cada login cria uma sessão nova (id trocado no login). Logout revoga.
- **Guards globais** (`APP_GUARD` no `AuthModule`): `SessionGuard` (toda rota exige sessão, exceto `@Public()`) e `RolesGuard` (`@Roles('admin')`). O usuário vai para o `AppContext`. O `X-Dev-User` e os guards provisórios da fatia 2 foram removidos.
- **Auditoria:** `login`, `login_failed` (com `reason`, `username`, ip e aparelho em `metadata`), `logout` e `first_admin_created` pelo `AuditWriter`. Mensagem de erro sempre genérica: "Usuário ou senha inválidos."
- **Primeiro admin:** com zero usuários (inclusive removidos), a API escreve no log um link `APP_URL/setup?token=...` de uso único. O token vive só na memória (reiniciar gera outro). Depois que o admin existe, `GET/POST /api/auth/setup` respondem 404.
- **Front (pedido do dono):** login em tela dividida ao meio (marca à esquerda, formulário à direita; no celular só o formulário). Ao clicar em "Acessar", o formulário dá lugar à tela de sessão (`/login/sessao`), que mostra um feed de mensagens passo a passo (validar credenciais, abrir sessão, carregar perfil). Sucesso: feed verde e "Bem-vindo(a)", depois abre o app. Erro: tela de erro (`/login/erro`) com feed vermelho e "Tentar novamente", que volta ao login com o usuário preenchido. A senha passa do formulário para a tela de sessão só pela store do Pinia, em memória, e é descartada ao ser usada.
- **Guarda de rotas no Vue Router:** rota sem `meta.public` exige login (consulta `/api/me` uma vez). Ela só esconde telas; quem protege os dados é a API. Um 401 no meio do uso volta para o login.
- `trust proxy` = loopback (proxy do Vite em dev, Funnel em produção), para `req.ip` ser o IP real.

**Consequências.** Para testar a API pelo terminal, faça login e guarde o cookie (`curl -c cookies.txt ...`, depois `-b cookies.txt`). O seed define a senha de `ana` e `bruno` a partir de `SEED_PASSWORD` em `apps/api/.env`. Sem anti-CSRF ainda: `SameSite=Lax` já barra o envio do cookie em POST vindo de outro site; o cabeçalho anti-CSRF entra na fatia 5.
