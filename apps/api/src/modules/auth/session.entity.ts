import type { Opt } from '@mikro-orm/core';
import { Entity, Index, PrimaryKey, Property } from '@mikro-orm/decorators/legacy';
import { v7 as uuidv7 } from 'uuid';

/**
 * Sessão de login, guardada no Postgres (não JWT): dá para encerrar na hora (logout, admin,
 * "sair de outros aparelhos") e sobrevive a restart da API.
 *
 * O cookie leva um token aleatório; aqui fica só o hash SHA-256 dele, então quem lê o banco
 * não consegue se passar pelo usuário.
 *
 * Não herda de BaseEntity: cada requisição atualiza lastSeenAt/expiresAt (expiração deslizante),
 * e isso não deve encher a auditoria. Login e logout são auditados pelo AuditWriter.
 */
@Entity({ tableName: 'sessions' })
@Index({ properties: ['userId', 'revokedAt'] })
export class Session {
  @PrimaryKey({ type: 'uuid' })
  id: Opt<string> = uuidv7();

  @Property({ type: 'string', length: 64, unique: true })
  tokenHash!: string;

  @Property({ type: 'uuid' })
  userId!: string;

  @Property({ type: 'datetime' })
  createdAt: Opt<Date> = new Date();

  @Property({ type: 'datetime' })
  lastSeenAt: Opt<Date> = new Date();

  @Property({ type: 'datetime', index: true })
  expiresAt!: Date;

  @Property({ type: 'datetime', nullable: true })
  revokedAt: Opt<Date> | null = null;

  @Property({ type: 'string', length: 64, nullable: true })
  ip: Opt<string> | null = null;

  @Property({ type: 'string', length: 300, nullable: true })
  userAgent: Opt<string> | null = null;
}
