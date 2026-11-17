import {
  type CreatedPasswordReset,
  type PasswordResetCheck,
  resetPasswordSchema,
} from '@fluxo/contracts';
import { Body, Controller, Get, HttpCode, Param, ParseUUIDPipe, Post } from '@nestjs/common';
import { Throttle } from '@nestjs/throttler';
import { createZodDto } from 'nestjs-zod';
import { Public } from '../../common/guards/public.decorator';
import { Roles } from '../../common/guards/roles.decorator';
import { PasswordResetService } from './password-reset.service';

class ResetPasswordDto extends createZodDto(resetPasswordSchema) {}

/** Admin gera o link: POST /api/admin/users/:id/password-reset */
@Controller('admin/users')
@Roles('admin')
export class AdminPasswordResetController {
  constructor(private readonly resets: PasswordResetService) {}

  @Post(':id/password-reset')
  create(@Param('id', ParseUUIDPipe) id: string): Promise<CreatedPasswordReset> {
    return this.resets.create(id);
  }
}

/** Quem recebeu o link: GET/POST /api/password-reset (abertos, com limite de tentativas). */
@Controller('password-reset')
@Public()
@Throttle({ default: { limit: 10, ttl: 60_000 } })
export class PasswordResetController {
  constructor(private readonly resets: PasswordResetService) {}

  @Get(':token')
  check(@Param('token') token: string): Promise<PasswordResetCheck> {
    return this.resets.check(token);
  }

  @Post()
  @HttpCode(204)
  async reset(@Body() body: ResetPasswordDto): Promise<void> {
    await this.resets.reset(body);
  }
}
