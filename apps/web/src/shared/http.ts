import type { z } from 'zod';

export class ApiError extends Error {
  constructor(
    readonly status: number,
    message: string,
  ) {
    super(message);
    this.name = 'ApiError';
  }
}

/**
 * GET na API validando a resposta com o schema de @fluxo/contracts.
 * Se a API devolver algo fora do contrato, falha aqui em vez de quebrar a tela adiante.
 */
export async function apiGet<S extends z.ZodType>(path: string, schema: S): Promise<z.infer<S>> {
  const response = await fetch(`/api${path}`, {
    credentials: 'same-origin',
    headers: { Accept: 'application/json' },
  });
  if (!response.ok) {
    throw new ApiError(response.status, `GET ${path} respondeu ${response.status}`);
  }
  return schema.parse(await response.json());
}
