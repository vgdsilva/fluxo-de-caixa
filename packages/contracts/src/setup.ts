import { z } from 'zod';
import { passwordSchema, usernameSchema } from './auth';

/**
 * GET /api/setup/status — o sistema ainda precisa do primeiro admin?
 * Vem da configuração SETUP_DB no banco: 0 (ou ausente) = setup pendente, 1 = concluído.
 */
export const setupStatusSchema = z.object({ required: z.boolean() });
export type SetupStatus = z.infer<typeof setupStatusSchema>;

/** POST /api/setup — cria o primeiro admin e marca SETUP_DB = 1. */
export const setupSchema = z.object({
  username: usernameSchema,
  name: z.string().trim().min(1, 'Informe o nome').max(120),
  password: passwordSchema,
});
export type SetupInput = z.infer<typeof setupSchema>;
