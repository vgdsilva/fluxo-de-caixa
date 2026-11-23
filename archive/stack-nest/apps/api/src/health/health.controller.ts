import { Controller, Get } from '@nestjs/common';
import type { HealthResponse } from '@fluxo/contracts';
import { APP_VERSION } from '../app-version';

// GET /api/health. Usado pelo front na fatia 1 e, na fatia 8, pelo healthcheck do Docker Compose.
// A partir da fatia 4 esta rota será marcada com @Public().
@Controller('health')
export class HealthController {
  @Get()
  get(): HealthResponse {
    return { status: 'ok', time: new Date().toISOString(), version: APP_VERSION };
  }
}
