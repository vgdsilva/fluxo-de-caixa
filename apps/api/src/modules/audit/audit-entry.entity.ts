import type { Opt } from '@mikro-orm/core';
import { Entity, Index, PrimaryKey, Property } from '@mikro-orm/decorators/legacy';
import { v7 as uuidv7 } from 'uuid';
import type { Channel } from '../../common/context/app-context';

export interface AuditChange {
  from: unknown;
  to: unknown;
}

/**
 * Uma linha de auditoria (schema `audit`, tabela `entries`). Só inserção.
 *
 * Não herda de BaseEntity de propósito: não tem soft delete, versão nem updatedAt, e não passa
 * pelos filtros globais. O dono do registro alterado fica em `recordOwnerId` (e não `ownerId`)
 * justamente para o filtro `owner` não se aplicar: o admin vê toda a auditoria.
 */
@Entity({ schema: 'audit', tableName: 'entries' })
@Index({ properties: ['entityType', 'entityId', 'at'] })
@Index({ properties: ['recordOwnerId', 'at'] })
@Index({ properties: ['userId', 'at'] })
@Index({ properties: ['action', 'at'] })
export class AuditEntry {
  @PrimaryKey({ type: 'uuid' })
  id: Opt<string> = uuidv7();

  @Property({ type: 'datetime', index: true })
  at: Opt<Date> = new Date();

  /** Quem fez. null = sistema (job, seed) ou alguém não identificado (ex.: login falho). */
  @Property({ type: 'uuid', nullable: true })
  userId: Opt<string> | null = null;

  /** Dono do registro alterado (OwnedEntity). null em eventos sem entidade dona. */
  @Property({ type: 'uuid', nullable: true })
  recordOwnerId: Opt<string> | null = null;

  @Property({ type: 'string', length: 16 })
  channel!: Channel;

  /** create | update | delete | restore, ou evento: login, login_failed, 2fa_reset, invite_created... */
  @Property({ type: 'string', length: 40 })
  action!: string;

  @Property({ type: 'string', length: 80, nullable: true })
  entityType: Opt<string> | null = null;

  @Property({ type: 'string', length: 80, nullable: true })
  entityId: Opt<string> | null = null;

  /** Só os campos alterados: { campo: { from, to } }. Campos sensíveis vêm como "***". */
  @Property({ type: 'json', nullable: true })
  changes: Opt<Record<string, AuditChange>> | null = null;

  /** Extras do evento: ip, aparelho, motivo de falha. */
  @Property({ type: 'json', nullable: true })
  metadata: Opt<Record<string, unknown>> | null = null;

  @Property({ type: 'uuid', index: true })
  correlationId!: string;
}
