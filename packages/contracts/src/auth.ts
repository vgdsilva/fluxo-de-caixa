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

/** Corpo de erro padrão da API (o que o Nest devolve em exceções HTTP). */
export const apiErrorSchema = z.object({
  statusCode: z.number(),
  message: z.union([z.string(), z.array(z.string())]),
  error: z.string().optional(),
});
export type ApiErrorBody = z.infer<typeof apiErrorSchema>;
