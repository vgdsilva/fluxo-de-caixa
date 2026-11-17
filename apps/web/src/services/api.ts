import { apiErrorSchema } from '@fluxo/contracts';
import type { z } from 'zod';

/** Erro de uma chamada à API, com a mensagem que a API mandou (já em português). */
export class ApiError extends Error {
  constructor(
    readonly status: number,
    message: string,
  ) {
    super(message);
    this.name = 'ApiError';
  }
}

/** Chamado quando uma rota protegida responde 401 (sessão expirou). Configurado em main.ts. */
let onUnauthorized: (() => void) | undefined;
export function setUnauthorizedHandler(handler: () => void): void {
  onUnauthorized = handler;
}

interface RequestOptions {
  method?: 'GET' | 'POST' | 'PATCH' | 'DELETE';
  body?: unknown;
  /** Não disparar o "sessão expirou" (para as próprias chamadas de login). */
  skipUnauthorizedHandler?: boolean;
}

async function request(path: string, options: RequestOptions = {}): Promise<Response> {
  let response: Response;
  try {
    response = await fetch(`/api${path}`, {
      method: options.method ?? 'GET',
      // O cookie de sessão vai junto porque front e API estão na mesma origem.
      credentials: 'same-origin',
      headers: {
        Accept: 'application/json',
        // Anti-CSRF: a API recusa escritas sem este cabeçalho (CsrfMiddleware).
        'X-Requested-With': 'fluxo',
        ...(options.body !== undefined ? { 'Content-Type': 'application/json' } : {}),
      },
      body: options.body !== undefined ? JSON.stringify(options.body) : undefined,
    });
  } catch {
    throw new ApiError(0, 'Não foi possível falar com o servidor. Verifique sua conexão.');
  }

  if (!response.ok) {
    if (response.status === 401 && !options.skipUnauthorizedHandler) onUnauthorized?.();
    throw new ApiError(response.status, await errorMessage(response));
  }
  return response;
}

async function errorMessage(response: Response): Promise<string> {
  const parsed = apiErrorSchema.safeParse(await response.json().catch(() => null));
  if (parsed.success) {
    const { message, errors } = parsed.data;
    if (errors?.length) return errors.map((e) => e.message).join(' ');
    return Array.isArray(message) ? message.join(' ') : message;
  }
  if (response.status >= 500) return 'O servidor encontrou um erro. Tente de novo em instantes.';
  return `Erro ${response.status}`;
}

/**
 * GET validando a resposta com o schema de @fluxo/contracts.
 * Se a API devolver algo fora do contrato, falha aqui em vez de quebrar a tela adiante.
 */
export async function apiGet<S extends z.ZodType>(
  path: string,
  schema: S,
  options: Omit<RequestOptions, 'method' | 'body'> = {},
): Promise<z.infer<S>> {
  const response = await request(path, options);
  return schema.parse(await response.json());
}

/** POST/PATCH/DELETE. Com `schema`, valida e devolve o corpo; sem, ignora o corpo (ex.: 204). */
export async function apiSend<S extends z.ZodType>(
  method: 'POST' | 'PATCH' | 'DELETE',
  path: string,
  body?: unknown,
  schema?: S,
  options: Omit<RequestOptions, 'method' | 'body'> = {},
): Promise<z.infer<S>> {
  const response = await request(path, { ...options, method, body });
  if (!schema) return undefined as z.infer<S>;
  return schema.parse(await response.json());
}
