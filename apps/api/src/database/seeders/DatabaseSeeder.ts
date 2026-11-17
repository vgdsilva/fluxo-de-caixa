import { hash } from '@node-rs/argon2';
import type { EntityManager } from '@mikro-orm/postgresql';
import { Seeder } from '@mikro-orm/seeder';
import { SystemSetting } from '../../modules/settings/system-setting.entity';
import { User } from '../../modules/users/user.entity';
import { runAsSystem } from '../system-context';

/**
 * Dados de desenvolvimento: dois usuários (ana = admin, bruno = user) para testar login,
 * isolamento e auditoria. A senha dos dois vem de SEED_PASSWORD em apps/api/.env.
 * Idempotente: rodar de novo não duplica; só define a senha de quem ainda não tem.
 * Rodar: `pnpm --filter @fluxo/api db:seed`
 */
export class DatabaseSeeder extends Seeder {
  async run(rootEm: EntityManager): Promise<void> {
    const password = process.env.SEED_PASSWORD;
    if (!password || password.length < 12) {
      throw new Error('Defina SEED_PASSWORD (mínimo 12 caracteres) em apps/api/.env para o seed.');
    }
    const passwordHash = await hash(password);

    await runAsSystem(rootEm, async (em) => {
      await upsertUser(em, 'ana', 'Ana (seed)', 'admin', passwordHash);
      await upsertUser(em, 'bruno', 'Bruno (seed)', 'user', passwordHash);
      // Com o admin do seed criado, o setup do primeiro acesso não é mais necessário.
      await em.upsert(SystemSetting, { key: 'SETUP_DB', value: '1' });
      await em.flush();
    });
  }
}

async function upsertUser(
  em: EntityManager,
  username: string,
  name: string,
  role: User['role'],
  passwordHash: string,
): Promise<void> {
  const existing = await em.findOne(User, { username }, { populate: ['passwordHash'] });
  if (!existing) {
    em.create(User, { username, name, role, passwordHash });
  } else if (!existing.passwordHash) {
    existing.passwordHash = passwordHash;
  }
}
