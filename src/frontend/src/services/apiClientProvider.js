import axios from "axios";
import { BACKEND_ENDPOINT } from "@/system/runtimeConfigHelper.js";
import {
  ensureValidAccessToken,
  refreshAccessToken,
  clearAuthSession,
} from "@/system/authSessionHelper.js";
import { getGlobalRouter } from "@/router/globalRouter.js";

/*
 * Cliente HTTP único do app (mesmo papel do apiClientProvider da referência).
 * - Antes de cada chamada, garante um access token válido e envia `Authorization: Bearer`.
 * - Se a resposta for 401, renova o token uma vez e repete a chamada.
 * - Se a renovação falhar, a sessão acabou: limpa e volta para o login.
 */

let instance = null;

function createClient() {
  const axiosApi = axios.create({
    baseURL: BACKEND_ENDPOINT(),
    headers: {
      Accept: "application/json",
      "Content-Type": "application/json",
    },
  });

  axiosApi.interceptors.request.use(async (request) => {
    if (request.skipAuth) return request;
    const token = await ensureValidAccessToken().catch(() => null);
    if (token) request.headers.Authorization = `Bearer ${token}`;
    return request;
  });

  axiosApi.interceptors.response.use(
    (response) => response,
    async (error) => {
      const original = error.config;
      if (error.response?.status === 401 && original && !original.skipAuth && !original._retried) {
        original._retried = true;
        try {
          const session = await refreshAccessToken();
          original.headers.Authorization = `Bearer ${session.accessToken}`;
          return axiosApi(original);
        } catch {
          redirectToLogin();
        }
      }
      return Promise.reject(error);
    }
  );

  return axiosApi;
}

function redirectToLogin() {
  clearAuthSession();
  const router = getGlobalRouter();
  if (router && router.currentRoute.value.path !== "/auth/login") {
    router.replace({ path: "/auth/session" });
  }
}

// Mantém a assinatura da referência: `apiClient().then(x => x.get(...))`.
export default async () => {
  if (!instance) instance = createClient();
  return instance;
};
