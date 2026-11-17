import { Module } from '@nestjs/common';
import { ConfigModule } from '@nestjs/config';
import { validateEnv } from './config/env';
import { HealthModule } from './modules/health/health.module';

// O módulo raiz só compõe os módulos de feature. Cada feature é um módulo Nest próprio
// e só expõe a outros o que estiver em `exports` (ver docs/fase-0.md, "Estrutura da solução").
@Module({
  imports: [
    ConfigModule.forRoot({
      isGlobal: true,
      cache: true,
      // Valida as variáveis no start: a API não sobe com configuração faltando ou inválida.
      validate: validateEnv,
    }),
    HealthModule,
  ],
})
export class AppModule {}
