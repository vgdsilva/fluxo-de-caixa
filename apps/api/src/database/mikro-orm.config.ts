import { existsSync } from 'node:fs';
import { join } from 'node:path';
import { Migrator } from '@mikro-orm/migrations';
import { defineConfig, type Options } from '@mikro-orm/postgresql';
import { SeedManager } from '@mikro-orm/seeder';
import { type Env, validateEnv } from '../config/env';
import { entities } from './entities';
import { globalFilters } from './filters';
import { BaseEntitySubscriber } from './subscribers/base-entity.subscriber';

/**
 * Configuração do MikroORM, usada pela API (via DatabaseModule) e pelo CLI das migrations.
 * Caminhos relativos à pasta apps/api (de onde os dois rodam).
 */
export function createOrmConfig(env: Env): Options {
  return defineConfig({
    host: env.DB_HOST,
    port: env.DB_PORT,
    dbName: env.DB_NAME,
    user: env.DB_USER,
    password: env.DB_PASSWORD,
    entities,
    filters: globalFilters,
    subscribers: [new BaseEntitySubscriber()],
    extensions: [Migrator, SeedManager],
    debug: env.DB_DEBUG,
    // Não buscar entidades por pasta: a lista explícita em entities.ts é a fonte.
    discovery: { warnWhenNoEntities: true },
    migrations: {
      // JS compilado em produção; TS no CLI de desenvolvimento.
      path: 'dist/database/migrations',
      pathTs: 'src/database/migrations',
      transactional: true,
      allOrNothing: true,
    },
    seeder: {
      path: 'dist/database/seeders',
      pathTs: 'src/database/seeders',
      defaultSeeder: 'DatabaseSeeder',
    },
  });
}

/** Usado só pelo CLI (`pnpm --filter @fluxo/api mikro-orm ...`): lê apps/api/.env sozinho. */
function configForCli(): Options {
  const envFile = join(process.cwd(), '.env');
  if (existsSync(envFile)) process.loadEnvFile(envFile);
  return createOrmConfig(validateEnv(process.env));
}

export default configForCli;
