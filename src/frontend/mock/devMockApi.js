import crypto from "node:crypto";

/*
 * API falsa para desenvolver o front sem o WebAPI (`npm run dev:mock`).
 * Segue o contrato de docs/contrato-autenticacao.md. Nada aqui vai para o build.
 *
 * Usuários de teste (só existem neste mock):
 *   admin@example.com / admin123  (Admin; o login "admin" também funciona)
 *   maria@example.com / maria123  (User)
 *
 * Access token dura 2 minutos para dar para ver a renovação automática acontecendo.
 */

const ACCESS_TOKEN_SECONDS = 120;
const REFRESH_TOKEN_DAYS = 7;

const users = [
  { id: "0b9c6a1e-0000-4000-8000-000000000001", login: "admin", password: "admin123", name: "Admin Teste", email: "admin@example.com", role: "Admin" },
  { id: "0b9c6a1e-0000-4000-8000-000000000002", login: "maria", password: "maria123", name: "Maria Teste", email: "maria@example.com", role: "User" },
];

// Sessões no "banco": refreshToken -> { userId, expiresAt, revokedAt }
const sessions = new Map();

function b64url(obj) {
  return Buffer.from(JSON.stringify(obj)).toString("base64url");
}

function createAccessToken(user) {
  const now = Math.floor(Date.now() / 1000);
  const payload = { sid: user.id, email: user.email, name: user.name, role: user.role, iat: now, exp: now + ACCESS_TOKEN_SECONDS };
  return `${b64url({ alg: "none", typ: "JWT" })}.${b64url(payload)}.mock`;
}

function readAccessToken(header) {
  const token = (header || "").replace(/^Bearer\s+/i, "");
  try {
    const payload = JSON.parse(Buffer.from(token.split(".")[1], "base64url").toString());
    if (payload.exp * 1000 <= Date.now()) return null;
    return users.find((u) => u.id === payload.sid) ?? null;
  } catch {
    return null;
  }
}

function publicUser(user) {
  const { password, ...rest } = user;
  return rest;
}

function issue(user) {
  const refreshToken = crypto.randomBytes(32).toString("base64url");
  const refreshTokenExpiresAt = new Date(Date.now() + REFRESH_TOKEN_DAYS * 86400 * 1000);
  sessions.set(refreshToken, { userId: user.id, expiresAt: refreshTokenExpiresAt, revokedAt: null });
  return {
    accessToken: createAccessToken(user),
    accessTokenExpiresAt: new Date(Date.now() + ACCESS_TOKEN_SECONDS * 1000).toISOString(),
    refreshToken,
    refreshTokenExpiresAt: refreshTokenExpiresAt.toISOString(),
    user: publicUser(user),
  };
}

function send(res, status, body) {
  res.statusCode = status;
  if (body === undefined) return res.end();
  res.setHeader("Content-Type", "application/json");
  res.end(JSON.stringify(body));
}

function problem(res, status, title, detail) {
  send(res, status, { type: "about:blank", title, status, detail });
}

function readBody(req) {
  return new Promise((resolve) => {
    let data = "";
    req.on("data", (chunk) => (data += chunk));
    req.on("end", () => {
      try {
        resolve(data ? JSON.parse(data) : {});
      } catch {
        resolve({});
      }
    });
  });
}

const routes = {
  "POST /api/auth/login": async (req, res) => {
    const { login, password } = await readBody(req);
    const user = users.find((u) => (u.login === login || u.email === login) && u.password === password);
    if (!user) return problem(res, 401, "Usuário ou senha inválidos");
    send(res, 200, issue(user));
  },
  "POST /api/auth/refresh": async (req, res) => {
    const { refreshToken } = await readBody(req);
    const session = sessions.get(refreshToken);
    if (!session || session.revokedAt || session.expiresAt <= new Date()) {
      return problem(res, 401, "Sessão inválida ou expirada");
    }
    // Rotação: o refresh token usado deixa de valer.
    session.revokedAt = new Date();
    const user = users.find((u) => u.id === session.userId);
    send(res, 200, issue(user));
  },
  "POST /api/auth/logout": async (req, res) => {
    const { refreshToken } = await readBody(req);
    const session = sessions.get(refreshToken);
    if (session) session.revokedAt = new Date();
    send(res, 204);
  },
  "GET /api/me": async (req, res) => {
    const user = readAccessToken(req.headers.authorization);
    if (!user) return problem(res, 401, "Não autenticado");
    send(res, 200, publicUser(user));
  },
};

export function devMockApi() {
  return {
    name: "fluxo-dev-mock-api",
    configureServer(server) {
      server.middlewares.use(async (req, res, next) => {
        if (!req.url.startsWith("/api/")) return next();
        const path = req.url.split("?")[0];
        const handler = routes[`${req.method} ${path}`];
        if (!handler) return problem(res, 404, "Rota não existe no mock", `${req.method} ${path}`);
        // Pequeno atraso para ver os estados de carregamento.
        setTimeout(() => handler(req, res).catch(next), 250);
      });
    },
  };
}
