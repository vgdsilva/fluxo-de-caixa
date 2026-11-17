import { SetMetadata } from '@nestjs/common';
import type { Role } from '../context/app-context';

export const ROLES_KEY = 'roles';

/** Restringe a rota a estes papéis. Equivale a [Authorize(Roles = "admin")] no ASP.NET. */
export const Roles = (...roles: Role[]) => SetMetadata(ROLES_KEY, roles);
