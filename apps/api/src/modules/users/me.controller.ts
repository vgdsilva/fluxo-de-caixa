import { type Me, updateMeSchema } from '@fluxo/contracts';
import { Body, Controller, Get, Patch, UseGuards } from '@nestjs/common';
import { createZodDto } from 'nestjs-zod';
import { AppContext } from '../../common/context/app-context';
import { RequireUserGuard } from '../../common/guards/require-user.guard';
import { UsersService } from './users.service';

class UpdateMeDto extends createZodDto(updateMeSchema) {}

// /api/me: perfil do usuário logado. A tela "Meu perfil" vem na fatia 6.
@Controller('me')
@UseGuards(RequireUserGuard)
export class MeController {
  constructor(private readonly users: UsersService) {}

  @Get()
  get(): Promise<Me> {
    return this.users.getMe(AppContext.userId()!);
  }

  @Patch()
  update(@Body() body: UpdateMeDto): Promise<Me> {
    return this.users.updateMe(AppContext.userId()!, body);
  }
}
