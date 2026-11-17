import { z } from 'zod';

export const PASSWORD_MIN_LENGTH = 12;

export const usernameSchema = z
  .string()
  .trim()
  .toLowerCase()
  .min(3, 'Mínimo de 3 caracteres')
  .max(40)
  .regex(/^[a-z0-9._-]+$/, 'Use só letras, números, ponto, hífen ou sublinhado');

export const passwordSchema = z
  .string()
  .min(PASSWORD_MIN_LENGTH, `Mínimo de ${PASSWORD_MIN_LENGTH} caracteres`)
  .max(200);

/** POST /api/auth/login */
export const loginSchema = z.object({
  username: z.string().trim().toLowerCase().min(1, 'Informe o usuário').max(40),
  password: z.string().min(1, 'Informe a senha').max(200),
});
export type LoginInput = z.infer<typeof loginSchema>;

/** GET /api/auth/setup/:token — o link de primeiro acesso ainda vale? */
export const setupStatusSchema = z.object({ valid: z.boolean() });
export type SetupStatus = z.infer<typeof setupStatusSchema>;

/** POST /api/auth/setup — cria o primeiro admin a partir do link escrito no log da API. */
export const setupSchema = z.object({
  token: z.string().min(20).max(200),
  username: usernameSchema,
  name: z.string().trim().min(1, 'Informe o nome').max(120),
  password: passwordSchema,
});
export type SetupInput = z.infer<typeof setupSchema>;

/** Corpo de erro padrão da API (o que o Nest devolve em exceções HTTP). */
export const apiErrorSchema = z.object({
  statusCode: z.number(),
  message: z.union([z.string(), z.array(z.string())]),
  error: z.string().optional(),
});
export type ApiErrorBody = z.infer<typeof apiErrorSchema>;
