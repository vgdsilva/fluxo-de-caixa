import { Controller, Get } from '@nestjs/common';
import { Public } from '../../common/guards/public.decorator';
import type { HealthResponse } from '@fluxo/contracts';
import { HealthService } from './health.service';

// GET /api/health. Usado pelo front e, na fatia 8, pelo healthcheck do Docker Compose.
// Aberta (sem login): @Public().
@Controller('health')
@Public()
export class HealthController {
  // O Nest injeta o serviço pelo tipo do parâmetro do construtor.
  constructor(private readonly healthService: HealthService) {}

  @Get()
  get(): HealthResponse {
    return this.healthService.check();
  }
}
