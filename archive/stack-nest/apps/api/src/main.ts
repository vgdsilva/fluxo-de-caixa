import { Logger } from '@nestjs/common';
import { ConfigService } from '@nestjs/config';
import { NestFactory } from '@nestjs/core';
import { AppModule } from './app.module';
import type { Env } from './config/env';

async function bootstrap() {
  const app = await NestFactory.create(AppModule);

  // Toda a API fica sob /api; em produção o mesmo processo servirá o build do Vue no resto (fatia 8).
  app.setGlobalPrefix('api');
  app.enableShutdownHooks();

  const config = app.get<ConfigService<Env, true>>(ConfigService);
  const port = config.get('PORT', { infer: true });
  await app.listen(port);
  Logger.log(`API ouvindo em http://localhost:${port}/api`, 'Bootstrap');
}

void bootstrap();
