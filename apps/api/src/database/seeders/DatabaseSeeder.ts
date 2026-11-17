import type { EntityManager } from '@mikro-orm/postgresql';
import { Seeder } from '@mikro-orm/seeder';
import { User } from '../../modules/users/user.entity';
import { runAsSystem } from '../system-context';

/**
 * Dados de desenvolvimento: dois usuários para testar o isolamento (X-Dev-User: ana / bruno).
 * Idempotente: rodar de novo não duplica nada. Na fase 1 ganha contas e lançamentos de exemplo.
 * Rodar: `pnpm --filter @fluxo/api db:seed`
 */
export class DatabaseSeeder extends Seeder {
  async run(rootEm: EntityManager): Promise<void> {
    await runAsSystem(rootEm, async (em) => {
      await upsertUser(em, 'ana', 'Ana (seed)', 'admin');
      await upsertUser(em, 'bruno', 'Bruno (seed)', 'user');
      await em.flush();
    });
  }
}

async function upsertUser(
  em: EntityManager,
  username: string,
  name: string,
  role: User['role'],
): Promise<User> {
  const existing = await em.findOne(User, { username });
  if (existing) return existing;
  return em.create(User, { username, name, role });
}
