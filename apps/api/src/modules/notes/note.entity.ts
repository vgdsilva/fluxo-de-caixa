import type { Opt } from '@mikro-orm/core';
import { Entity, Property } from '@mikro-orm/decorators/legacy';
import { OwnedEntity } from '../../database/entities/owned.entity';

/**
 * Entidade de exemplo: herda id, datas, soft delete, versão e dono de OwnedEntity.
 * Uma entidade nova da fase 1 segue o mesmo molde: estender OwnedEntity e declarar só os próprios campos.
 */
@Entity({ tableName: 'notes' })
export class Note extends OwnedEntity {
  @Property({ type: 'string', length: 120 })
  title!: string;

  @Property({ type: 'text', default: '' })
  body: Opt<string> = '';
}
