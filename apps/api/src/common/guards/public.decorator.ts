import { SetMetadata } from '@nestjs/common';

export const IS_PUBLIC_KEY = 'isPublic';

/**
 * Marca uma rota (ou controller inteiro) como aberta, sem login. Todas as outras exigem sessão,
 * porque o SessionGuard é global. Equivale ao [AllowAnonymous] do ASP.NET.
 */
export const Public = () => SetMetadata(IS_PUBLIC_KEY, true);
