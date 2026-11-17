import type { Opt } from '@mikro-orm/core';
import { Entity, Property } from '@mikro-orm/decorators/legacy';
import { BaseEntity } from './base.entity';

/**
 * Base de todo dado de usuário (contas, cartões, lançamentos...). Herda de BaseEntity e
 * acrescenta o dono.
 *
 * - `ownerId` é preenchido pelo subscriber com o usuário da requisição e não pode mudar depois.
 * - O filtro global `owner` faz toda consulta enxergar só os registros do usuário atual.
 *   Na fase 1 ele passa a incluir "ou compartilhado comigo num fluxo".
 */
@Entity({ abstract: true })
export abstract class OwnedEntity extends BaseEntity {
  /** Opt: o subscriber preenche; não é obrigatório no em.create. */
  @Property({ type: 'uuid', index: true })
  ownerId!: Opt<string>;
}
