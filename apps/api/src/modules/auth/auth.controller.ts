import {
  type LoginInput,
  loginSchema,
  type Me,
  type SetupStatus,
  setupSchema,
} from '@fluxo/contracts';
import { Body, Controller, Get, HttpCode, Param, Post, Req, Res } from '@nestjs/common';
import { ConfigService } from '@nestjs/config';
import type { CookieOptions, Request, Response } from 'express';
import { createZodDto } from 'nestjs-zod';
import { Public } from '../../common/guards/public.decorator';
import type { Env } from '../../config/env';
import { AuthService } from './auth.service';
import { SESSION_COOKIE, SESSION_TTL_MS } from './sessions.service';

class LoginDto extends createZodDto(loginSchema) {}
class SetupDto extends createZodDto(setupSchema) {}

/**
 * /api/auth: login, logout e primeiro admin. Tudo aqui é @Public(), porque quem chama ainda
 * não tem sessão. `passthrough: true` deixa o Nest serializar o retorno mesmo usando `res`
 * para escrever o cookie.
 */
@Controller('auth')
@Public()
export class AuthController {
  constructor(
    private readonly auth: AuthService,
    private readonly config: ConfigService<Env, true>,
  ) {}

  @Post('login')
  @HttpCode(200)
  async login(
    @Body() body: LoginDto,
    @Req() req: Request,
    @Res({ passthrough: true }) res: Response,
  ): Promise<Me> {
    const { token, me } = await this.auth.login(body as LoginInput, {
      ip: req.ip,
      userAgent: req.header('user-agent'),
    });
    res.cookie(SESSION_COOKIE, token, this.cookieOptions());
    return me;
  }

  @Post('logout')
  @HttpCode(204)
  async logout(@Req() req: Request, @Res({ passthrough: true }) res: Response): Promise<void> {
    const token = (req.cookies as Record<string, string> | undefined)?.[SESSION_COOKIE];
    await this.auth.logout(token);
    res.clearCookie(SESSION_COOKIE, { ...this.cookieOptions(), maxAge: undefined });
  }

  @Get('setup/:token')
  setupStatus(@Param('token') token: string): SetupStatus {
    return { valid: this.auth.setupStatus(token) };
  }

  @Post('setup')
  @HttpCode(204)
  async setup(@Body() body: SetupDto): Promise<void> {
    await this.auth.setup(body);
  }

  /** Cookie da sessão: HttpOnly (JS não lê), SameSite=Lax, Secure em produção (HTTPS do Funnel). */
  private cookieOptions(): CookieOptions {
    return {
      httpOnly: true,
      sameSite: 'lax',
      secure: this.config.get('NODE_ENV', { infer: true }) === 'production',
      path: '/',
      maxAge: SESSION_TTL_MS,
    };
  }
}
