import { MikroORM, PostgreSqlDriver } from '@mikro-orm/postgresql';
import { MikroOrmModule } from '@mikro-orm/nestjs';
import { Global, Logger, Module, type OnModuleInit } from '@nestjs/common';
import { ConfigService } from '@nestjs/config';
import type { Env } from '../config/env';
import { createOrmConfig } from './mikro-orm.config';

/**
 * Liga o MikroORM ao Nest. O MikroOrmModule também registra o middleware que abre um
 * RequestContext por requisição: cada request ganha seu próprio EntityManager (identity map
 * isolado), como um DbContext scoped no ASP.NET.
 */
@Global()
@Module({
  imports: [
    MikroOrmModule.forRootAsync({
      // Obrigatório no MikroORM 7 com useFactory: diz ao Nest qual MikroORM/EntityManager injetar.
      driver: PostgreSqlDriver,
      inject: [ConfigService],
      useFactory: (config: ConfigService<Env, true>) => ({
        ...createOrmConfig(readEnv(config)),
        registerRequestContext: true,
      }),
    }),
  ],
})
export class DatabaseModule implements OnModuleInit {
  private readonly logger = new Logger(DatabaseModule.name);

  constructor(
    private readonly orm: MikroORM,
    private readonly config: ConfigService<Env, true>,
  ) {}

  /** Aplica migrations pendentes antes de a API começar a atender (como Database.Migrate() no .NET). */
  async onModuleInit(): Promise<void> {
    if (!this.config.get('DB_MIGRATE_ON_START', { infer: true })) return;
    const applied = await this.orm.migrator.up();
    if (applied.length > 0) {
      this.logger.log(`Migrations aplicadas: ${applied.map((m) => m.name).join(', ')}`);
    }
  }
}

function readEnv(config: ConfigService<Env, true>): Env {
  return {
    NODE_ENV: config.get('NODE_ENV', { infer: true }),
    PORT: config.get('PORT', { infer: true }),
    DB_HOST: config.get('DB_HOST', { infer: true }),
    DB_PORT: config.get('DB_PORT', { infer: true }),
    DB_NAME: config.get('DB_NAME', { infer: true }),
    DB_USER: config.get('DB_USER', { infer: true }),
    DB_PASSWORD: config.get('DB_PASSWORD', { infer: true }),
    DB_MIGRATE_ON_START: config.get('DB_MIGRATE_ON_START', { infer: true }),
    DB_DEBUG: config.get('DB_DEBUG', { infer: true }),
  };
}
