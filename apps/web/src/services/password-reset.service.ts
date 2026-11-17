import {
  type PasswordResetCheck,
  passwordResetCheckSchema,
  type ResetPassword,
} from '@fluxo/contracts';
import { apiGet, apiSend } from './api';

/** Redefinição de senha pelo link que o admin gera. */
export const passwordResetService = {
  check(token: string): Promise<PasswordResetCheck> {
    return apiGet(`/password-reset/${encodeURIComponent(token)}`, passwordResetCheckSchema, {
      skipUnauthorizedHandler: true,
    });
  },

  async reset(input: ResetPassword): Promise<void> {
    await apiSend('POST', '/password-reset', input, undefined, { skipUnauthorizedHandler: true });
  },
};
