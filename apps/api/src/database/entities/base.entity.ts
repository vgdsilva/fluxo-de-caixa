import type { Opt } from '@mikro-orm/core';
import { Entity, PrimaryKey, Property } from '@mikro-orm/decorators/legacy';
import { v7 as uuidv7 } from 'uuid';

/**
 * Base de toda entidade: id, datas, soft delete e concorrência otimista.
 * Equivale a uma classe base com Id/CreatedAt/UpdatedAt/DeletedAt + [ConcurrencyCheck] no EF Core.
 *
 * - `id` é UUID v7 (ordenado por tempo), gerado na aplicação, não no banco.
 * - Remover (`em.remove`) não apaga: o subscriber transforma em update de `deletedAt`.
 * - O filtro global `notDeleted` esconde registros removidos de todas as consultas.
 *
 * Os tipos de cada coluna são explícitos (`type: ...`) porque o CLI das migrations roda com tsx,
 * que não gera a metadata de tipos dos decorators.
 */
@Entity({ abstract: true })
export abstract class BaseEntity {
  @PrimaryKey({ type: 'uuid' })
  id: Opt<string> = uuidv7();

  @Property({ type: 'datetime', onCreate: () => new Date() })
  createdAt: Opt<Date> = new Date();

  @Property({ type: 'datetime', onCreate: () => new Date(), onUpdate: () => new Date() })
  updatedAt: Opt<Date> = new Date();

  @Property({ type: 'datetime', nullable: true, index: true })
  deletedAt: Opt<Date> | null = null;

  /** Incrementado a cada update; um update com versão antiga falha (OptimisticLockError). */
  @Property({ type: 'integer', version: true })
  version!: Opt<number>;
}
