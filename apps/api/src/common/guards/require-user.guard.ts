import { type CanActivate, Injectable, UnauthorizedException } from '@nestjs/common';
import { CurrentUserContext } from '../context/current-user.context';

/**
 * Barra a requisição se não houver usuário identificado. Provisório: na fatia 4 vira o
 * SessionGuard global, e as rotas abertas passam a usar @Public().
 */
@Injectable()
export class RequireUserGuard implements CanActivate {
  canActivate(): boolean {
    if (!CurrentUserContext.userId()) throw new UnauthorizedException();
    return true;
  }
}
