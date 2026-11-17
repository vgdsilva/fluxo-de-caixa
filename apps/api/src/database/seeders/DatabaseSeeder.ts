import type { EntityManager } from '@mikro-orm/postgresql';
import { Seeder } from '@mikro-orm/seeder';

/**
 * Dados de exemplo para desenvolvimento. Por enquanto vazio: usuários não vêm do seed, o admin
 * é criado pela tela de primeiro acesso (SETUP_DB, ADR 0014) e os demais por convite (fatia 5).
 * Na fase 1 este seeder ganha contas e lançamentos fictícios para testar telas.
 * Rodar: `pnpm --filter @fluxo/api db:seed`
 */
export class DatabaseSeeder extends Seeder {
  async run(_em: EntityManager): Promise<void> {
    // Nada a semear ainda.
  }
}
