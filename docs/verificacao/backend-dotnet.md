# Verificação: back-end .NET (login com JWT + sessão no banco)

Pré-requisito: a "Primeira execução" de [`src/backend/README.md`](../../src/backend/README.md) feita e a API rodando com `dotnet run --project src/backend/WebAPI`.

1. Abra <http://localhost:5039/docs>. O Scalar mostra "Fluxo de Caixa API" com `auth`, `me` e `health`.
2. `GET /api/health` responde 200.
3. `GET /api/me` sem token responde 401 com corpo ProblemDetails.
4. `POST /api/auth/login` com `{"login":"admin","password":"<Admin:Password>"}` responde 200 com `accessToken`, `accessTokenExpiresAt` (uns 15 min à frente), `refreshToken`, `refreshTokenExpiresAt` e `user` (com `login` e `email`). Com `"login":"admin@local.test"` também funciona.
5. `GET /api/me` com `Authorization: Bearer <accessToken>` responde 200 com o admin.
6. `POST /api/auth/refresh` com `{"refreshToken":"<refreshToken>"}` responde 200 com tokens novos. Repetir com o refresh token antigo responde 401.
7. `POST /api/auth/logout` com o refresh token atual responde 204; um refresh com ele depois disso responde 401.
8. Login sem `login` responde 400 com `errors.Login`. Senha errada 5 vezes seguidas: a 6ª tentativa, mesmo com a senha certa, responde 401 até passar o bloqueio (15 min).
9. No banco: `select action, entity_type from audit_log order by at desc limit 5;` mostra a criação e as atualizações da sessão; `select token_hash from session;` só tem hashes, nunca o refresh token devolvido.
