import { z } from 'zod';

/** Resposta de GET /api/health. */
export const healthResponseSchema = z.object({
  status: z.literal('ok'),
  /** Instante do servidor em ISO 8601, UTC. */
  time: z.iso.datetime(),
  /** Versão do pacote da API. */
  version: z.string(),
});

export type HealthResponse = z.infer<typeof healthResponseSchema>;
