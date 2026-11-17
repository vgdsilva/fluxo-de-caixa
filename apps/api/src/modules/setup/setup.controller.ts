import { type SetupStatus, setupSchema } from '@fluxo/contracts';
import { Body, Controller, Get, HttpCode, NotFoundException, Post } from '@nestjs/common';
import { createZodDto } from 'nestjs-zod';
import { Public } from '../../common/guards/public.decorator';
import { SetupService } from './setup.service';

class SetupDto extends createZodDto(setupSchema) {}

/** /api/setup: primeiro acesso, aberto (sem login) enquanto SETUP_DB não for 1. */
@Controller('setup')
@Public()
export class SetupController {
  constructor(private readonly setup: SetupService) {}

  @Get('status')
  async status(): Promise<SetupStatus> {
    return { required: await this.setup.isRequired() };
  }

  @Post()
  @HttpCode(204)
  async run(@Body() body: SetupDto): Promise<void> {
    if (!(await this.setup.isRequired())) throw new NotFoundException();
    await this.setup.run(body);
  }
}
