import { type SetupInput, setupStatusSchema } from '@fluxo/contracts';
import { apiGet, apiSend } from './api';

/** Primeiro acesso: a API diz se ainda falta criar o admin (configuração SETUP_DB no banco). */
export const setupService = {
  async isRequired(): Promise<boolean> {
    const status = await apiGet('/setup/status', setupStatusSchema, {
      skipUnauthorizedHandler: true,
    });
    return status.required;
  },

  async run(input: SetupInput): Promise<void> {
    await apiSend('POST', '/setup', input, undefined, { skipUnauthorizedHandler: true });
  },
};
