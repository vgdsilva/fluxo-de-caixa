# 0015 Convites, proteções do login e login sem 2FA

**Status:** aceita (17/11/2026). Implementa a fatia 5 da spec, com uma mudança de escopo pedida pelo dono do projeto: o segundo fator fica fora por enquanto.

**Decisão.**

- **Só login com usuário e senha.** A spec previa 2FA obrigatório (TOTP e passkeys). O dono pediu "só o login normal". A implementação de TOTP + códigos de recuperação + `TwoFactorGuard` + telas chegou a ser feita e testada pela API, e ficou guardada no branch local `feature/2fa-totp` (commit `b0bfb72`, não enviado ao GitHub) para quando quiser retomar. Passkeys não foram feitas.
- **Convites** (`invites`): cadastro só por convite. O admin cria em `POST /api/admin/invites` (opcionalmente preso a um e-mail), lista e revoga; o link `APP_URL/convite?token=...` aparece uma vez. Uso único, 72 h, só o hash do token no banco. O convidado informa nome, usuário, e-mail e senha; conta criada com papel `user`, na mesma transação que marca o convite como usado.
- **Política de senha** (`common/validators/password-policy.ts`): mínimo de 12 caracteres (schema), lista de senhas comuns, sequências, caractere repetido e senha que contenha usuário, nome ou e-mail. Vale para setup, convite e reset.
- **Bloqueio por conta:** 5 senhas erradas seguidas bloqueiam por 15 minutos (`users.failed_login_count`, `users.locked_until`); auditoria `account_locked` e `login_failed` com `reason`.
- **Limite por IP** (`@nestjs/throttler`): 300 req/min no geral; login, convites e reset de senha 10/min. Mensagem em português.
- **Anti-CSRF:** toda escrita (POST, PUT, PATCH, DELETE) exige o cabeçalho `X-Requested-With: fluxo` (`CsrfMiddleware`); o front sempre envia. Soma-se ao cookie `SameSite=Lax`.
- **Reset de senha pelo admin** (`password_resets`): `POST /api/admin/users/:id/password-reset` gera `APP_URL/redefinir-senha?token=...`, uso único, 1 h; um link novo invalida os anteriores. Redefinir encerra todas as sessões do usuário e tira o bloqueio.
- **Auditoria** ignora `failedLoginCount` e `lockedUntil` nas alterações automáticas (viram eventos próprios).

**Consequências.** Para testar a API pelo terminal, escritas precisam de `-H "X-Requested-With: fluxo"`. As telas de admin para criar convites e gerar links de reset entram na fatia 6; até lá, pela API. Sem 2FA, o risco de senha vazada fica mitigado só por senha forte, bloqueio e limite por IP; vale reavaliar antes de expor o app no Funnel (fatia 8).
