import { z } from 'zod';

export const roleSchema = z.enum(['admin', 'user']);

/** GET /api/me: o usuário logado. */
export const meSchema = z.object({
  id: z.uuid(),
  username: z.string(),
  name: z.string(),
  email: z.string().nullable(),
  role: roleSchema,
  version: z.number().int(),
});
export type Me = z.infer<typeof meSchema>;

/** PATCH /api/me: alterações de perfil (o nome; a senha tem rota própria). */
export const updateMeSchema = z.object({
  name: z.string().trim().min(1, 'Informe o nome').max(120),
  version: z.number().int(),
});
export type UpdateMe = z.infer<typeof updateMeSchema>;

/** PATCH /api/me/password */
export const changePasswordSchema = z.object({
  currentPassword: z.string().min(1, 'Informe a senha atual').max(200),
  newPassword: z.string().min(12, 'Mínimo de 12 caracteres').max(200),
});
export type ChangePassword = z.infer<typeof changePasswordSchema>;

/** GET /api/me/sessions: aparelhos com sessão aberta. */
export const sessionInfoSchema = z.object({
  id: z.uuid(),
  createdAt: z.iso.datetime(),
  lastSeenAt: z.iso.datetime(),
  ip: z.string().nullable(),
  userAgent: z.string().nullable(),
  /** É a sessão deste navegador? */
  current: z.boolean(),
});
export type SessionInfo = z.infer<typeof sessionInfoSchema>;

/** GET /api/admin/users */
export const adminUserSchema = z.object({
  id: z.uuid(),
  username: z.string(),
  name: z.string(),
  email: z.string().nullable(),
  role: roleSchema,
  active: z.boolean(),
  createdAt: z.iso.datetime(),
  lastLoginAt: z.iso.datetime().nullable(),
});
export type AdminUser = z.infer<typeof adminUserSchema>;
