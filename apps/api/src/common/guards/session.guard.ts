import {
  type CanActivate,
  type ExecutionContext,
  Injectable,
  UnauthorizedException,
} from '@nestjs/common';
import { Reflector } from '@nestjs/core';
import type { Request } from 'express';
import { SESSION_COOKIE, SessionsService } from '../../modules/auth/sessions.service';
import { UsersService } from '../../modules/users/users.service';
import { AppContext } from '../context/app-context';
import { IS_PUBLIC_KEY } from './public.decorator';

/**
 * Guard global (registrado como APP_GUARD no AuthModule): toda rota exige sessão válida,
 * exceto as marcadas com @Public(). Lê o cookie, valida a sessão no banco (renovando a expiração)
 * e coloca o usuário no AppContext, de onde filtros, subscribers e serviços leem.
 *
 * Na fatia 5 entra o TwoFactorGuard depois deste.
 */
@Injectable()
export class SessionGuard implements CanActivate {
  constructor(
    private readonly reflector: Reflector,
    private readonly sessions: SessionsService,
    private readonly users: UsersService,
  ) {}

  async canActivate(context: ExecutionContext): Promise<boolean> {
    const isPublic = this.reflector.getAllAndOverride<boolean>(IS_PUBLIC_KEY, [
      context.getHandler(),
      context.getClass(),
    ]);
    if (isPublic) return true;

    const req = context.switchToHttp().getRequest<Request>();
    const token = (req.cookies as Record<string, string> | undefined)?.[SESSION_COOKIE];
    if (!token) throw new UnauthorizedException();

    const session = await this.sessions.validate(token);
    if (!session) throw new UnauthorizedException();

    const role = await this.users.findRole(session.userId);
    if (!role) throw new UnauthorizedException(); // usuário removido ou desativado

    AppContext.setUser(session.userId, role, session.sessionId);
    return true;
  }
}
