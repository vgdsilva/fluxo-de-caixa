import { type AcceptInvite, type InviteCheck, inviteCheckSchema } from '@fluxo/contracts';
import { apiGet, apiSend } from './api';

/** Lado do convidado: conferir o link e criar a conta. (O lado do admin entra na fatia 6.) */
export const invitesService = {
  check(token: string): Promise<InviteCheck> {
    return apiGet(`/invites/${encodeURIComponent(token)}`, inviteCheckSchema, {
      skipUnauthorizedHandler: true,
    });
  },

  async accept(input: AcceptInvite): Promise<void> {
    await apiSend('POST', '/invites/accept', input, undefined, { skipUnauthorizedHandler: true });
  },
};
