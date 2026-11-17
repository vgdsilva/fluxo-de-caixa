import type { Opt } from '@mikro-orm/core';
import { Entity, Property } from '@mikro-orm/decorators/legacy';
import { BaseEntity } from '../../database/entities/base.entity';

/**
 * Link de redefinição de senha. Sem servidor de e-mail: o admin gera e manda pelo WhatsApp.
 * Uso único, expira em 1 hora. Só o hash do token fica no banco. Gerar um novo link invalida
 * os anteriores do mesmo usuário.
 */
@Entity({ tableName: 'password_resets' })
export class PasswordReset extends BaseEntity {
  @Property({ type: 'string', length: 64, unique: true, hidden: true })
  tokenHash!: string;

  @Property({ type: 'uuid', index: true })
  userId!: string;

  @Property({ type: 'uuid' })
  createdByUserId!: string;

  @Property({ type: 'datetime' })
  expiresAt!: Date;

  @Property({ type: 'datetime', nullable: true })
  usedAt: Opt<Date> | null = null;
}
