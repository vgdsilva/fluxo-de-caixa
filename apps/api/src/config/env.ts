import { z } from 'zod';

const envSchema = z.object({
  NODE_ENV: z.enum(['development', 'production', 'test']).default('development'),
  PORT: z.coerce.number().int().positive().default(3000),
  /** Endereço do front, usado nos links gerados (primeiro admin; convites na fatia 5). */
  APP_URL: z.url().default('http://localhost:5173'),

  // Banco (Postgres). Em dev, o script apps/api/scripts/setup-dev-db.ps1 cria usuário e banco.
  DB_HOST: z.string().default('localhost'),
  DB_PORT: z.coerce.number().int().positive().default(5432),
  DB_NAME: z.string().min(1),
  DB_USER: z.string().min(1),
  DB_PASSWORD: z.string().min(1),
  /** Aplica as migrations pendentes ao subir a API. */
  DB_MIGRATE_ON_START: z
    .enum(['true', 'false'])
    .default('true')
    .transform((v) => v === 'true'),
  /** Mostra o SQL gerado no log (só para depurar). */
  DB_DEBUG: z
    .enum(['true', 'false'])
    .default('false')
    .transform((v) => v === 'true'),
});

export type Env = z.infer<typeof envSchema>;

/** Usado pelo ConfigModule: recebe process.env (já mesclado com o .env) e devolve a config tipada. */
export function validateEnv(raw: Record<string, unknown>): Env {
  const result = envSchema.safeParse(raw);
  if (!result.success) {
    throw new Error(`Configuração inválida:\n${z.prettifyError(result.error)}`);
  }
  return result.data;
}
