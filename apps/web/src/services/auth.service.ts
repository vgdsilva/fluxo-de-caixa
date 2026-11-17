import {
  type LoginInput,
  type Me,
  meSchema,
  type SetupInput,
  setupStatusSchema,
} from '@fluxo/contracts';
import { apiGet, apiSend } from './api';

/** Chamadas de autenticação. O cookie da sessão é HttpOnly: o front nunca vê o token. */
export const authService = {
  login(input: LoginInput): Promise<Me> {
    return apiSend('POST', '/auth/login', input, meSchema, { skipUnauthorizedHandler: true });
  },

  async logout(): Promise<void> {
    await apiSend('POST', '/auth/logout', undefined, undefined, {
      skipUnauthorizedHandler: true,
    });
  },

  /** Usuário da sessão atual; lança ApiError 401 se não houver sessão. */
  me(): Promise<Me> {
    return apiGet('/me', meSchema, { skipUnauthorizedHandler: true });
  },

  async setupIsValid(token: string): Promise<boolean> {
    const status = await apiGet(`/auth/setup/${encodeURIComponent(token)}`, setupStatusSchema, {
      skipUnauthorizedHandler: true,
    });
    return status.valid;
  },

  async setup(input: SetupInput): Promise<void> {
    await apiSend('POST', '/auth/setup', input, undefined, { skipUnauthorizedHandler: true });
  },
};
