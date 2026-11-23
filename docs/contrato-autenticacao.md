# Contrato de autenticação (front ↔ WebAPI)

Login com sessão no banco + JWT curto renovado automaticamente. O back-end implementa este contrato em `src/backend/WebAPI/Controllers/AuthController.cs`.
O front segue este contrato em `src/frontend/src/system/authSessionHelper.js`, e a API falsa `src/frontend/mock/devMockApi.js` também.

## Ideia

- **Sessão no banco:** cada login cria uma linha na tabela `session` (já existe no WebAPI). O `refreshToken`
  é um valor aleatório; o banco guarda só o hash (`token_hash`), com `expires_at`, `ip_address`, `user_agent`
  e `revoked_at`.
- **Access token (JWT):** curto (sugestão: 5 a 15 minutos), com os dados do usuário nas claims. Vai em
  `Authorization: Bearer` em toda chamada.
- **Renovação:** o front chama `/auth/refresh` um pouco antes do JWT expirar e também quando uma chamada
  volta 401. Cada refresh **rotaciona** o refresh token (o antigo é revogado e um novo é devolvido).
- **Logout:** revoga a sessão no banco.

## Endpoints

Todas as rotas com prefixo `/api`. Erros no formato ProblemDetails (`{ title, status, detail }`).

### `POST /api/auth/login` (anônimo)

```json
{ "login": "maria", "password": "..." }
```

`200` → `AuthResponse`. `401` usuário/senha inválidos. `423` (ou `429`) conta bloqueada por tentativas,
com o motivo em `detail`.

### `POST /api/auth/refresh` (anônimo)

```json
{ "refreshToken": "..." }
```

`200` → `AuthResponse` com **novo** refresh token. `401` se a sessão não existe, expirou ou foi revogada.

Sugestão: se um refresh token já revogado for reapresentado (possível roubo), revogar todas as sessões do usuário.

### `POST /api/auth/logout` (anônimo, o refresh token identifica a sessão)

```json
{ "refreshToken": "..." }
```

`204`. Deve responder `204` mesmo se a sessão já não existir.

### `GET /api/me` (Bearer)

`200` → `User`. `401` se o JWT for inválido ou expirado (o front então renova e repete a chamada).

## Tipos

```jsonc
// AuthResponse
{
  "accessToken": "eyJ...",
  "accessTokenExpiresAt": "2026-11-23T15:10:00Z",   // ISO 8601 UTC
  "refreshToken": "base64url-aleatório",
  "refreshTokenExpiresAt": "2026-11-30T15:00:00Z",
  "user": { /* User */ }
}

// User
{
  "id": "guid",
  "name": "Maria Teste",
  "login": "maria",
  "email": "maria@example.com",
  "role": "Admin" | "User"
}
```

Claims do JWT usadas como reserva pelo front (se `user` não vier): `sid` (id), `email`, `name`, `role`, `exp`.

## Observações para o backend

- JSON em camelCase (padrão do ASP.NET).
- `role` como texto (`"Admin"`/`"User"`); o front também aceita `1` para Admin.
- Em dev o front roda em `http://localhost:8080` e repassa `/api` para `http://localhost:5039` pelo proxy do
  Vite, então não precisa de CORS em dev.
- Duração sugerida: access token 10 min, sessão 7 dias deslizantes (cada refresh estende `expires_at`).
