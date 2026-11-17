import {
  type ChangePassword,
  type Me,
  meSchema,
  type SessionInfo,
  sessionInfoSchema,
  type UpdateMe,
} from '@fluxo/contracts';
import { z } from 'zod';
import { apiGet, apiSend } from './api';

/** "Meu perfil": dados, senha e sessões do próprio usuário. */
export const profileService = {
  me(): Promise<Me> {
    return apiGet('/me', meSchema);
  },

  update(input: UpdateMe): Promise<Me> {
    return apiSend('PATCH', '/me', input, meSchema);
  },

  changePassword(input: ChangePassword): Promise<{ otherSessionsRevoked: number }> {
    return apiSend('PATCH', '/me/password', input, z.object({ otherSessionsRevoked: z.number() }));
  },

  sessions(): Promise<SessionInfo[]> {
    return apiGet('/me/sessions', z.array(sessionInfoSchema));
  },

  async revokeSession(id: string): Promise<void> {
    await apiSend('DELETE', `/me/sessions/${id}`);
  },

  revokeOtherSessions(): Promise<{ revoked: number }> {
    return apiSend(
      'POST',
      '/me/sessions/revoke-others',
      undefined,
      z.object({ revoked: z.number() }),
    );
  },
};
