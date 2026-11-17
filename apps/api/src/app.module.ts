import { type MiddlewareConsumer, Module, type NestModule } from '@nestjs/common';
import { ConfigModule } from '@nestjs/config';
import { APP_PIPE } from '@nestjs/core';
import { ZodValidationPipe } from 'nestjs-zod';
import { CorrelationIdMiddleware } from './common/middleware/correlation-id.middleware';
import { validateEnv } from './config/env';
import { DatabaseModule } from './database/database.module';
import { AuditModule } from './modules/audit/audit.module';
import { AuthModule } from './modules/auth/auth.module';
import { HealthModule } from './modules/health/health.module';
import { SetupModule } from './modules/setup/setup.module';
import { UsersModule } from './modules/users/users.module';

// O módulo raiz só compõe os módulos de feature. Cada feature é um módulo Nest próprio
// e só expõe a outros o que estiver em `exports` (ver docs/adr/0010).
@Module({
  imports: [
    ConfigModule.forRoot({
      isGlobal: true,
      cache: true,
      // Valida as variáveis no start: a API não sobe com configuração faltando ou inválida.
      validate: validateEnv,
    }),
    DatabaseModule,
    HealthModule,
    UsersModule,
    AuditModule,
    AuthModule,
    SetupModule,
  ],
  providers: [
    // Valida todo @Body()/@Query() tipado com um DTO de createZodDto (schemas de @fluxo/contracts).
    { provide: APP_PIPE, useClass: ZodValidationPipe },
  ],
})
export class AppModule implements NestModule {
  configure(consumer: MiddlewareConsumer): void {
    // Abre o contexto da requisição (CorrelationId). O usuário entra depois, pelo SessionGuard.
    consumer.apply(CorrelationIdMiddleware).forRoutes('*path');
  }
}
