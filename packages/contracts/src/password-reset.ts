import { z } from 'zod';
import { passwordSchema } from './auth';

/** POST /api/admin/users/:id/password-reset — o link aparece só aqui, uma vez. */
export const createdPasswordResetSchema = z.object({
  link: z.url(),
  expiresAt: z.iso.datetime(),
});
export type CreatedPasswordReset = z.infer<typeof createdPasswordResetSchema>;

/** GET /api/password-reset/:token */
export const passwordResetCheckSchema = z.object({
  valid: z.boolean(),
  username: z.string().nullable(),
});
export type PasswordResetCheck = z.infer<typeof passwordResetCheckSchema>;

/** POST /api/password-reset */
export const resetPasswordSchema = z.object({
  token: z.string().min(20).max(200),
  password: passwordSchema,
});
export type ResetPassword = z.infer<typeof resetPasswordSchema>;
