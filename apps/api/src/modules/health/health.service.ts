import { Injectable } from '@nestjs/common';
import type { HealthResponse } from '@fluxo/contracts';
import { APP_VERSION } from '../../common/constants/app-version';

// Serviço de aplicação: o controller só recebe a requisição e delega para cá.
// @Injectable() registra a classe no container de DI do Nest (como services.AddSingleton no .NET).
@Injectable()
export class HealthService {
  check(): HealthResponse {
    return { status: 'ok', time: new Date().toISOString(), version: APP_VERSION };
  }
}
