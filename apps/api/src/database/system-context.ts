import type { EntityManager } from '@mikro-orm/postgresql';
import { CurrentUserContext } from '../common/context/current-user.context';

/**
 * Único jeito permitido de trabalhar sem o filtro de dono: jobs de sistema, seeds e manutenção.
 * Abre um EntityManager novo (fork, como um DbContext novo) num contexto marcado como sistema.
 * O filtro notDeleted continua ligado; para ver removidos, passe `filters: { notDeleted: false }`
 * na própria consulta, também só aqui dentro.
 */
export function runAsSystem<T>(
  em: EntityManager,
  fn: (em: EntityManager) => Promise<T>,
): Promise<T> {
  return CurrentUserContext.runAsSystemUnsafe(() => fn(em.fork()));
}
