import { z } from 'zod';
import { passwordSchema, usernameSchema } from './auth';

export const emailSchema = z.email('E-mail inválido').max(254);

/** POST /api/admin/invites — admin cria um convite (opcionalmente preso a um e-mail). */
export const createInviteSchema = z.object({
  email: emailSchema.optional(),
});
export type CreateInvite = z.infer<typeof createInviteSchema>;

/** Resposta da criação: o link aparece só aqui, uma vez. */
export const createdInviteSchema = z.object({
  id: z.uuid(),
  link: z.url(),
  expiresAt: z.iso.datetime(),
});
export type CreatedInvite = z.infer<typeof createdInviteSchema>;

export const inviteStatusSchema = z.enum(['pending', 'used', 'revoked', 'expired']);

/** GET /api/admin/invites */
export const inviteSchema = z.object({
  id: z.uuid(),
  email: z.string().nullable(),
  status: inviteStatusSchema,
  createdAt: z.iso.datetime(),
  expiresAt: z.iso.datetime(),
  usedAt: z.iso.datetime().nullable(),
});
export type Invite = z.infer<typeof inviteSchema>;

/** GET /api/invites/:token — o convite ainda vale? */
export const inviteCheckSchema = z.object({
  valid: z.boolean(),
  email: z.string().nullable(),
});
export type InviteCheck = z.infer<typeof inviteCheckSchema>;

/** POST /api/invites/accept — o convidado cria a conta. */
export const acceptInviteSchema = z.object({
  token: z.string().min(20).max(200),
  name: z.string().trim().min(1, 'Informe o nome').max(120),
  username: usernameSchema,
  email: emailSchema,
  password: passwordSchema,
});
export type AcceptInvite = z.infer<typeof acceptInviteSchema>;
