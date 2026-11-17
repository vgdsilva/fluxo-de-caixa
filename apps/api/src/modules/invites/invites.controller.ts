import {
  acceptInviteSchema,
  type CreatedInvite,
  createInviteSchema,
  type Invite,
  type InviteCheck,
} from '@fluxo/contracts';
import {
  Body,
  Controller,
  Delete,
  Get,
  HttpCode,
  Param,
  ParseUUIDPipe,
  Post,
} from '@nestjs/common';
import { Throttle } from '@nestjs/throttler';
import { createZodDto } from 'nestjs-zod';
import { Public } from '../../common/guards/public.decorator';
import { Roles } from '../../common/guards/roles.decorator';
import { InvitesService } from './invites.service';

class CreateInviteDto extends createZodDto(createInviteSchema) {}
class AcceptInviteDto extends createZodDto(acceptInviteSchema) {}

/** Admin: /api/admin/invites (criar, listar, revogar). A tela vem na fatia 6. */
@Controller('admin/invites')
@Roles('admin')
export class AdminInvitesController {
  constructor(private readonly invites: InvitesService) {}

  @Post()
  create(@Body() body: CreateInviteDto): Promise<CreatedInvite> {
    return this.invites.create(body);
  }

  @Get()
  list(): Promise<Invite[]> {
    return this.invites.list();
  }

  @Delete(':id')
  @HttpCode(204)
  async revoke(@Param('id', ParseUUIDPipe) id: string): Promise<void> {
    await this.invites.revoke(id);
  }
}

/** Convidado: /api/invites (abertos, com limite de tentativas por IP). */
@Controller('invites')
@Public()
@Throttle({ default: { limit: 10, ttl: 60_000 } })
export class InvitesController {
  constructor(private readonly invites: InvitesService) {}

  @Get(':token')
  check(@Param('token') token: string): Promise<InviteCheck> {
    return this.invites.check(token);
  }

  @Post('accept')
  @HttpCode(204)
  async accept(@Body() body: AcceptInviteDto): Promise<void> {
    await this.invites.accept(body);
  }
}
