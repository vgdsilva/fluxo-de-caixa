import { Injectable, type NestMiddleware } from '@nestjs/common';
import type { NextFunction, Request, Response } from 'express';
import { AppContext } from '../context/app-context';

export const CORRELATION_HEADER = 'x-correlation-id';
const UUID = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

/**
 * Primeiro middleware de toda requisição: abre o AppContext com um CorrelationId (reaproveita o
 * cabeçalho X-Correlation-Id se vier um UUID válido) e devolve o mesmo id na resposta.
 * É middleware, e não interceptor como dizia a spec, porque o contexto precisa envolver a
 * requisição inteira, inclusive os guards, que rodam antes dos interceptors.
 */
@Injectable()
export class CorrelationIdMiddleware implements NestMiddleware {
  use(req: Request, res: Response, next: NextFunction): void {
    const incoming = req.header(CORRELATION_HEADER);
    const correlationId = incoming && UUID.test(incoming) ? incoming : undefined;
    AppContext.run({ correlationId, channel: 'ui' }, () => {
      res.setHeader(CORRELATION_HEADER, AppContext.correlationId() ?? '');
      next();
    });
  }
}
