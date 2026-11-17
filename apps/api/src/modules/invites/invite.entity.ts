import type { Opt } from '@mikro-orm/core';
import { Entity, Property } from '@mikro-orm/decorators/legacy';
import { BaseEntity } from '../../database/entities/base.entity';

/**
 * Convite para criar conta. Cadastro só existe por convite (spec da fase 0).
 * Uso único, expira em 72 h, pode ser revogado e opcionalmente fica preso a um e-mail.
 * Do token só fica o hash (o link aparece uma vez, na criação). Herda de BaseEntity, então
 * criação, uso e revogação vão para a auditoria.
 */
@Entity({ tableName: 'invites' })
export class Invite extends BaseEntity {
  @Property({ type: 'string', length: 64, unique: true, hidden: true })
  tokenHash!: string;

  /** Se preenchido, o cadastro só aceita este e-mail. */
  @Property({ type: 'string', length: 254, nullable: true })
  email: Opt<string> | null = null;

  @Property({ type: 'datetime' })
  expiresAt!: Date;

  @Property({ type: 'uuid' })
  createdByUserId!: string;

  @Property({ type: 'datetime', nullable: true })
  usedAt: Opt<Date> | null = null;

  @Property({ type: 'uuid', nullable: true })
  usedByUserId: Opt<string> | null = null;

  @Property({ type: 'datetime', nullable: true })
  revokedAt: Opt<Date> | null = null;
}
