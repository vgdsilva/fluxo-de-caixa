import { Controller, Get } from '@nestjs/common';
import type { HealthResponse } from '@fluxo/contracts';
import { HealthService } from './health.service';

// GET /api/health. Usado pelo front e, na fatia 8, pelo healthcheck do Docker Compose.
// A partir da fatia 4 esta rota será marcada com @Public().
@Controller('health')
export class HealthController {
  // O Nest injeta o serviço pelo tipo do parâmetro do construtor.
  constructor(private readonly healthService: HealthService) {}

  @Get()
  get(): HealthResponse {
    return this.healthService.check();
  }
}
