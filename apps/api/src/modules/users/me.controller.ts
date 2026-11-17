import { type Me, updateMeSchema } from '@fluxo/contracts';
import { Body, Controller, Get, Patch } from '@nestjs/common';
import { createZodDto } from 'nestjs-zod';
import { AppContext } from '../../common/context/app-context';
import { UsersService } from './users.service';

class UpdateMeDto extends createZodDto(updateMeSchema) {}

// /api/me: perfil do usuário logado (exige sessão, pelo SessionGuard global). Tela na fatia 6.
@Controller('me')
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
