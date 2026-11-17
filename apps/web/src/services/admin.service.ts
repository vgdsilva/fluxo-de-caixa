import {
  type AdminUser,
  adminUserSchema,
  type AuditEntry,
  auditEntrySchema,
  type AuditQuery,
  type CreatedInvite,
  createdInviteSchema,
  type CreatedPasswordReset,
  createdPasswordResetSchema,
  type CreateInvite,
  type Invite,
  inviteSchema,
} from '@fluxo/contracts';
import { z } from 'zod';
import { apiGet, apiSend } from './api';

/** Telas de administração: usuários, convites e auditoria (só admin; a API confere o papel). */
export const adminService = {
  users(): Promise<AdminUser[]> {
    return apiGet('/admin/users', z.array(adminUserSchema));
  },

  async setUserActive(id: string, active: boolean): Promise<void> {
    await apiSend('POST', `/admin/users/${id}/${active ? 'reactivate' : 'deactivate'}`);
  },

  passwordResetLink(userId: string): Promise<CreatedPasswordReset> {
    return apiSend(
      'POST',
      `/admin/users/${userId}/password-reset`,
      undefined,
      createdPasswordResetSchema,
    );
  },

  invites(): Promise<Invite[]> {
    return apiGet('/admin/invites', z.array(inviteSchema));
  },

  createInvite(input: CreateInvite): Promise<CreatedInvite> {
    return apiSend('POST', '/admin/invites', input, createdInviteSchema);
  },

  async revokeInvite(id: string): Promise<void> {
    await apiSend('DELETE', `/admin/invites/${id}`);
  },

  audit(query: Partial<AuditQuery>): Promise<AuditEntry[]> {
    const params = new URLSearchParams();
    for (const [key, value] of Object.entries(query)) {
      if (value !== undefined && value !== null && value !== '') params.set(key, String(value));
    }
    return apiGet(`/audit?${params.toString()}`, z.array(auditEntrySchema));
  },
};
