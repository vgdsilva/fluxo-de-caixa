import { type AdminUser, changePasswordSchema, type SessionInfo } from '@fluxo/contracts';
import {
  Body,
  Controller,
  Delete,
  Get,
  HttpCode,
  Param,
  ParseUUIDPipe,
  Patch,
  Post,
} from '@nestjs/common';
import { Throttle } from '@nestjs/throttler';
import { createZodDto } from 'nestjs-zod';
import { Roles } from '../../common/guards/roles.decorator';
import { AccountService } from './account.service';

class ChangePasswordDto extends createZodDto(changePasswordSchema) {}

/** /api/me/...: segurança da própria conta (senha e sessões). */
@Controller('me')
export class MySecurityController {
  constructor(private readonly account: AccountService) {}

  @Patch('password')
  @Throttle({ default: { limit: 10, ttl: 60_000 } })
  changePassword(@Body() body: ChangePasswordDto): Promise<{ otherSessionsRevoked: number }> {
    return this.account.changePassword(body);
  }

  @Get('sessions')
  sessions(): Promise<SessionInfo[]> {
    return this.account.listMySessions();
  }

  @Delete('sessions/:id')
  @HttpCode(204)
  async revoke(@Param('id', ParseUUIDPipe) id: string): Promise<void> {
    await this.account.revokeMySession(id);
  }

  @Post('sessions/revoke-others')
  revokeOthers(): Promise<{ revoked: number }> {
    return this.account.revokeMyOtherSessions();
  }
}

/** /api/admin/users: lista, desativa e reativa (o admin não vê dados financeiros por aqui). */
@Controller('admin/users')
@Roles('admin')
export class AdminUsersController {
  constructor(private readonly account: AccountService) {}

  @Get()
  list(): Promise<AdminUser[]> {
    return this.account.listUsers();
  }

  @Post(':id/deactivate')
  @HttpCode(204)
  async deactivate(@Param('id', ParseUUIDPipe) id: string): Promise<void> {
    await this.account.setUserActive(id, false);
  }

  @Post(':id/reactivate')
  @HttpCode(204)
  async reactivate(@Param('id', ParseUUIDPipe) id: string): Promise<void> {
    await this.account.setUserActive(id, true);
  }
}
