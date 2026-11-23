# Verificação: back-end .NET (login com JWT + sessão no banco)

Pré-requisito: a "Primeira execução" de [`src/backend/README.md`](../../src/backend/README.md) feita e a API rodando com `dotnet run --project src/backend/WebAPI`.

1. Abra <http://localhost:5039/docs>. O Scalar mostra "Fluxo de Caixa API" com `auth`, `me` e `health`.
2. `GET /api/health` responde 200.
3. `GET /api/me` sem token responde 401.
4. `POST /api/auth/login` com `{"email":"admin@local.test","password":"<Admin:Password>"}` responde 200 com `accessToken`, `expiresAt` (uns 15 min à frente) e `user`, e a resposta traz `Set-Cookie: fc_refresh=...; path=/api/auth; secure; samesite=strict; httponly`.
5. `GET /api/me` com `Authorization: Bearer <accessToken>` responde 200 com o admin.
6. `POST /api/auth/refresh` (o cookie vai junto) responde 200 com um token novo e um cookie novo. Repetir com o cookie antigo responde 401.
7. `POST /api/auth/logout` responde 204; um refresh depois disso responde 401.
8. Login com senha errada 5 vezes seguidas: a 6ª tentativa, mesmo com a senha certa, responde 401 até passar o bloqueio (15 min).
9. No banco: `select action, entity_type from audit_log order by at desc limit 5;` mostra a criação e as atualizações da sessão; `select token_hash from session;` só tem hashes, nunca o token do cookie.
