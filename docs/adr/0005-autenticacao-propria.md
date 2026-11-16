# 0005 Convite, 2FA obrigatório, sessão em cookie, sem e-mail

**Status:** aceita (16/11/2026)

**Contexto.** O app fica exposto na internet pelo Funnel. São poucos usuários e não queremos manter servidor de e-mail.

**Decisão.** Autenticação própria no Nest: senha com argon2id, TOTP com `otplib`, passkeys com `@simplewebauthn`, rate limit com `@nestjs/throttler`. Sessão no servidor (tabela no Postgres) com cookie `HttpOnly`/`Secure`/`SameSite=Lax`, não JWT. Cadastro só por convite; segundo fator obrigatório. Convite e reset de senha geram links de uso único que o admin envia à mão (WhatsApp).

**Consequências.** Sessões podem ser encerradas na hora (admin, "sair de outros aparelhos"). Sem SMTP, DNS ou reputação de e-mail. Requisições que alteram dados exigem cabeçalho anti-CSRF. Fatias 4 e 5.
