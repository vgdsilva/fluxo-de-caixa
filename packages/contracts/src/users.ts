import { z } from 'zod';

export const roleSchema = z.enum(['admin', 'user']);

/** GET /api/me: o usuário logado. */
export const meSchema = z.object({
  id: z.uuid(),
  username: z.string(),
  name: z.string(),
  role: roleSchema,
  version: z.number().int(),
});
export type Me = z.infer<typeof meSchema>;

/** PATCH /api/me: alterações de perfil (por enquanto só o nome; senha e 2FA na fatia 5). */
export const updateMeSchema = z.object({
  name: z.string().trim().min(1, 'Informe o nome').max(120),
  version: z.number().int(),
});
export type UpdateMe = z.infer<typeof updateMeSchema>;
