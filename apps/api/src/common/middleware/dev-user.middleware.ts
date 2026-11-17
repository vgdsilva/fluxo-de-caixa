import { Injectable, type NestMiddleware } from '@nestjs/common';
import { ConfigService } from '@nestjs/config';
import type { NextFunction, Request, Response } from 'express';
import type { Env } from '../../config/env';
import { UsersService } from '../../modules/users/users.service';
import { CurrentUserContext } from '../context/current-user.context';

export const DEV_USER_HEADER = 'x-dev-user';

/**
 * PROVISÓRIO até a fatia 4 (login). Só em NODE_ENV=development: o cabeçalho `X-Dev-User: <username>`
 * diz quem é o usuário da requisição, para testar o isolamento entre usuários sem ter login.
 * Em produção este middleware não faz nada.
 */
@Injectable()
export class DevUserMiddleware implements NestMiddleware {
  constructor(
    private readonly users: UsersService,
    private readonly config: ConfigService<Env, true>,
  ) {}

  async use(req: Request, _res: Response, next: NextFunction): Promise<void> {
    const username = req.header(DEV_USER_HEADER);
    if (this.config.get('NODE_ENV', { infer: true }) !== 'development' || !username) {
      next();
      return;
    }
    const userId = await this.users.findIdByUsername(username);
    if (!userId) {
      next();
      return;
    }
    // Todo o resto da requisição roda "dentro" deste usuário.
    CurrentUserContext.runAsUser(userId, () => next());
  }
}
