import { type CanActivate, Injectable, UnauthorizedException } from '@nestjs/common';
import { AppContext } from '../context/app-context';

/**
 * Barra a requisição se não houver usuário identificado. Provisório: na fatia 4 vira o
 * SessionGuard global, e as rotas abertas passam a usar @Public().
 */
@Injectable()
export class RequireUserGuard implements CanActivate {
  canActivate(): boolean {
    if (!AppContext.userId()) throw new UnauthorizedException();
    return true;
  }
}
