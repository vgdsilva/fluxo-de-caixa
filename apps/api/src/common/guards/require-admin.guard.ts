import {
  type CanActivate,
  ForbiddenException,
  Injectable,
  UnauthorizedException,
} from '@nestjs/common';
import { AppContext } from '../context/app-context';

/** Só admin. Provisório: na fatia 4 vira @Roles('admin') + RolesGuard. */
@Injectable()
export class RequireAdminGuard implements CanActivate {
  canActivate(): boolean {
    if (!AppContext.userId()) throw new UnauthorizedException();
    if (AppContext.role() !== 'admin') throw new ForbiddenException();
    return true;
  }
}
