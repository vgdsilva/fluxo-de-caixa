import axios from "axios";
import { jwtDecode } from "jwt-decode";
import { BACKEND_ENDPOINT } from "@/system/runtimeConfigHelper.js";
import appConfig from "@/system/appConfig.js";

/*
 * Sessão do usuário no front.
 *
 * Contrato com o WebAPI (ver docs/contrato-autenticacao.md):
 *  - POST /api/auth/login   { login, password }  -> AuthResponse
 *  - POST /api/auth/refresh { refreshToken }     -> AuthResponse (refresh token novo a cada chamada)
 *  - POST /api/auth/logout  { refreshToken }     -> 204 (revoga a sessão no banco)
 *
 * AuthResponse = { accessToken, accessTokenExpiresAt, refreshToken, refreshTokenExpiresAt, user }
 *
 * O access token (JWT) dura poucos minutos. Este helper agenda a renovação um pouco antes de ele
 * expirar e também renova sob demanda quando uma chamada recebe 401 (ver apiClientProvider.js).
 */

const STORAGE_KEY = "authSession";
const LOGIN_FLAG = "app.loginSuccessful";

let refreshTimerId = null;
let refreshInFlight = null;
const listeners = new Set();

export function getAuthSession() {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    return raw ? JSON.parse(raw) : null;
  } catch {
    return null;
  }
}

export function getAccessToken() {
  return getAuthSession()?.accessToken ?? null;
}

export function getSessionUser() {
  return getAuthSession()?.user ?? null;
}

export function hasSession() {
  const session = getAuthSession();
  if (!session?.refreshToken) return false;
  return !isExpired(session.refreshTokenExpiresAt);
}

export function saveAuthSession(authResponse) {
  const session = normalize(authResponse);
  localStorage.setItem(STORAGE_KEY, JSON.stringify(session));
  localStorage.setItem(LOGIN_FLAG, "true");
  scheduleRefresh();
  notify(session);
  return session;
}

export function clearAuthSession() {
  stopRefreshSchedule();
  localStorage.removeItem(STORAGE_KEY);
  localStorage.removeItem(LOGIN_FLAG);
  notify(null);
}

/** Avisado sempre que a sessão muda (login, renovação, logout, outra aba). */
export function onAuthSessionChange(callback) {
  listeners.add(callback);
  return () => listeners.delete(callback);
}

/**
 * Renova o access token. Chamadas simultâneas compartilham a mesma requisição,
 * porque o refresh token é rotacionado e só pode ser usado uma vez.
 */
export function refreshAccessToken() {
  if (refreshInFlight) return refreshInFlight;

  const session = getAuthSession();
  if (!session?.refreshToken) return Promise.reject(new Error("Sem sessão para renovar"));

  refreshInFlight = axios
    .post(`${BACKEND_ENDPOINT()}/auth/refresh`, { refreshToken: session.refreshToken })
    .then((response) => saveAuthSession(response.data))
    .catch((error) => {
      // Outra aba pode ter renovado antes: se o token guardado mudou, segue com ele.
      const current = getAuthSession();
      if (current?.refreshToken && current.refreshToken !== session.refreshToken) return current;
      if (error?.response?.status === 401 || error?.response?.status === 403) clearAuthSession();
      throw error;
    })
    .finally(() => {
      refreshInFlight = null;
    });

  return refreshInFlight;
}

/** Garante um access token válido antes de uma chamada. */
export async function ensureValidAccessToken() {
  const session = getAuthSession();
  if (!session) return null;
  if (!isExpiringSoon(session.accessTokenExpiresAt, 5)) return session.accessToken;
  const renewed = await refreshAccessToken();
  return renewed?.accessToken ?? null;
}

export function scheduleRefresh() {
  stopRefreshSchedule();
  const session = getAuthSession();
  if (!session?.accessTokenExpiresAt) return;

  const advance = appConfig.auth.refreshBeforeExpirationSeconds * 1000;
  const lifetime = toTime(session.accessTokenExpiresAt) - Date.now();
  // Tokens muito curtos: renova na metade da vida em vez de "60 s antes".
  const wait = Math.max(lifetime - Math.min(advance, lifetime / 2), 1000);

  refreshTimerId = setTimeout(() => {
    refreshAccessToken().catch((e) => console.warn("Falha ao renovar o token", e));
  }, wait);
}

export function stopRefreshSchedule() {
  if (refreshTimerId) clearTimeout(refreshTimerId);
  refreshTimerId = null;
}

/** Mantém as abas abertas em sincronia (login, renovação e logout feitos em outra aba). */
export function startCrossTabSync() {
  window.addEventListener("storage", (event) => {
    if (event.key !== STORAGE_KEY) return;
    const session = getAuthSession();
    if (session) scheduleRefresh();
    else stopRefreshSchedule();
    notify(session);
  });
}

function normalize(data) {
  const accessTokenExpiresAt =
    data.accessTokenExpiresAt ?? expirationFromJwt(data.accessToken) ?? null;
  return {
    accessToken: data.accessToken,
    accessTokenExpiresAt,
    refreshToken: data.refreshToken,
    refreshTokenExpiresAt: data.refreshTokenExpiresAt ?? null,
    user: data.user ?? userFromJwt(data.accessToken),
  };
}

function expirationFromJwt(token) {
  try {
    const { exp } = jwtDecode(token);
    return exp ? new Date(exp * 1000).toISOString() : null;
  } catch {
    return null;
  }
}

function userFromJwt(token) {
  try {
    const claims = jwtDecode(token);
    return { id: claims.sid ?? claims.sub, email: claims.email, name: claims.name ?? claims.email };
  } catch {
    return null;
  }
}

function toTime(value) {
  return value ? new Date(value).getTime() : 0;
}

function isExpired(value) {
  return value ? toTime(value) <= Date.now() : false;
}

function isExpiringSoon(value, seconds) {
  return toTime(value) - Date.now() <= seconds * 1000;
}

function notify(session) {
  listeners.forEach((callback) => {
    try {
      callback(session);
    } catch (e) {
      console.error(e);
    }
  });
}
