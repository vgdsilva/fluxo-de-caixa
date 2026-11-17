import type { EntityManager } from '@mikro-orm/postgresql';
import { Seeder } from '@mikro-orm/seeder';
import { Note } from '../../modules/notes/note.entity';
import { User } from '../../modules/users/user.entity';
import { runAsSystem } from '../system-context';

/**
 * Dados de desenvolvimento: dois usuários com uma anotação cada, para testar o isolamento.
 * Idempotente: rodar de novo não duplica nada.
 * Rodar: `pnpm --filter @fluxo/api db:seed`
 */
export class DatabaseSeeder extends Seeder {
  async run(rootEm: EntityManager): Promise<void> {
    await runAsSystem(rootEm, async (em) => {
      const ana = await upsertUser(em, 'ana', 'Ana (seed)', 'admin');
      const bruno = await upsertUser(em, 'bruno', 'Bruno (seed)', 'user');

      for (const [owner, title] of [
        [ana, 'Anotação da Ana'],
        [bruno, 'Anotação do Bruno'],
      ] as const) {
        const exists = await em.count(Note, { ownerId: owner.id, title });
        if (!exists) em.create(Note, { ownerId: owner.id, title, body: 'Criada pelo seed.' });
      }
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
