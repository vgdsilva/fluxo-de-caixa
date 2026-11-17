import { Logger } from '@nestjs/common';
import { ConfigService } from '@nestjs/config';
import { NestFactory } from '@nestjs/core';
import type { NestExpressApplication } from '@nestjs/platform-express';
import cookieParser from 'cookie-parser';
import { AppModule } from './app.module';
import type { Env } from './config/env';

async function bootstrap() {
  const app = await NestFactory.create<NestExpressApplication>(AppModule);

  // Toda a API fica sob /api; em produção o mesmo processo servirá o build do Vue no resto (fatia 8).
  app.setGlobalPrefix('api');
  app.enableShutdownHooks();
  // Lê os cookies da requisição (req.cookies), onde vem o token da sessão.
  app.use(cookieParser());
  // Atrás do proxy do Vite (dev) ou do Tailscale Funnel (produção), ambos na própria máquina:
  // confia no X-Forwarded-For só vindo de loopback, para req.ip ser o IP real do cliente.
  app.set('trust proxy', 'loopback');

  const config = app.get<ConfigService<Env, true>>(ConfigService);
  const port = config.get('PORT', { infer: true });
  await app.listen(port);
  Logger.log(`API ouvindo em http://localhost:${port}/api`, 'Bootstrap');
}

void bootstrap();
