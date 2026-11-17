import {
  type CanActivate,
  type ExecutionContext,
  ForbiddenException,
  Injectable,
} from '@nestjs/common';
import { Reflector } from '@nestjs/core';
import { AppContext, type Role } from '../context/app-context';
import { ROLES_KEY } from './roles.decorator';

/** Guard global que confere @Roles(...). Roda depois do SessionGuard. */
@Injectable()
export class RolesGuard implements CanActivate {
  constructor(private readonly reflector: Reflector) {}

  canActivate(context: ExecutionContext): boolean {
    const roles = this.reflector.getAllAndOverride<Role[] | undefined>(ROLES_KEY, [
      context.getHandler(),
      context.getClass(),
    ]);
    if (!roles || roles.length === 0) return true;
    const role = AppContext.role();
    if (!role || !roles.includes(role)) throw new ForbiddenException();
    return true;
  }
}
