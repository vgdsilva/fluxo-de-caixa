import type { FilterDef } from '@mikro-orm/core';
import { AppContext } from '../common/context/app-context';

/** true se a entidade tem a propriedade (ex.: só OwnedEntity tem ownerId). */
function hasProperty(em: unknown, entityName: unknown, property: string): boolean {
  const meta = (
    em as { getMetadata(): { find(name: unknown): { properties: object } | undefined } }
  )
    .getMetadata()
    .find(entityName);
  return !!meta && property in meta.properties;
}

/**
 * Filtros globais, ligados por padrão em toda consulta (find, count, update e delete em massa).
 * Equivalem aos HasQueryFilter do EF Core. Desligar filtro só é permitido no runAsSystem.
 */
export const globalFilters: Record<string, FilterDef> = {
  /** Esconde registros com soft delete. */
  notDeleted: {
    name: 'notDeleted',
    default: true,
    args: false,
    cond: (_args, _type, em, _options, entityName) => {
      if (!hasProperty(em, entityName, 'deletedAt')) return {};
      return { deletedAt: null };
    },
  },

  /** Só registros do usuário atual. Sem usuário na requisição, falha em vez de devolver tudo. */
  owner: {
    name: 'owner',
    default: true,
    args: false,
    cond: (_args, _type, em, _options, entityName) => {
      if (!hasProperty(em, entityName, 'ownerId')) return {};
      if (AppContext.isSystem()) return {};
      const userId = AppContext.userId();
      if (!userId) {
        throw new Error(
          `Consulta a ${String(entityName)} sem usuário na requisição (filtro owner). Use runAsSystem para jobs.`,
        );
      }
      return { ownerId: userId };
    },
  },
};
