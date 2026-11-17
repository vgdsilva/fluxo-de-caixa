import { ForbiddenException, Injectable, type NestMiddleware } from '@nestjs/common';
import type { NextFunction, Request, Response } from 'express';

export const CSRF_HEADER = 'x-requested-with';
export const CSRF_VALUE = 'fluxo';
const SAFE_METHODS = new Set(['GET', 'HEAD', 'OPTIONS']);

/**
 * Anti-CSRF: toda requisição que altera dados (POST, PUT, PATCH, DELETE) precisa do cabeçalho
 * `X-Requested-With: fluxo`, que o front sempre envia. Um site de terceiros não consegue mandar
 * cabeçalho customizado para cá sem o navegador fazer um preflight de CORS, que a API não
 * autoriza. Soma-se ao cookie SameSite=Lax.
 */
@Injectable()
export class CsrfMiddleware implements NestMiddleware {
  use(req: Request, _res: Response, next: NextFunction): void {
    if (!SAFE_METHODS.has(req.method) && req.header(CSRF_HEADER) !== CSRF_VALUE) {
      throw new ForbiddenException('Requisição sem o cabeçalho anti-CSRF.');
    }
    next();
  }
}
