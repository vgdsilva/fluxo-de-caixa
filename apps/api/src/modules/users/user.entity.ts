import type { Opt } from '@mikro-orm/core';
import { Entity, Enum, Property } from '@mikro-orm/decorators/legacy';
import { BaseEntity } from '../../database/entities/base.entity';

export type UserRole = 'admin' | 'user';

/**
 * Usuário do sistema. Herda de BaseEntity (não de OwnedEntity): o usuário não tem dono, ele é o dono.
 */
@Entity({ tableName: 'users' })
export class User extends BaseEntity {
  @Property({ type: 'string', length: 40, unique: true })
  username!: string;

  @Property({ type: 'string', length: 120 })
  name!: string;

  /** Informado no cadastro por convite. Não há envio de e-mail; serve para contato e conferência. */
  @Property({ type: 'string', length: 254, nullable: true })
  email: Opt<string> | null = null;

  @Enum({ items: ['admin', 'user'], default: 'user' })
  role: Opt<UserRole> = 'user';

  /**
   * Hash argon2id da senha. `hidden`: nunca sai em serialização e aparece como "***" na auditoria.
   * null = usuário ainda sem senha definida (não consegue entrar).
   */
  @Property({ type: 'text', nullable: true, hidden: true, lazy: true })
  passwordHash: Opt<string> | null = null;

  /** Senhas erradas seguidas. Na 5ª, a conta fica bloqueada por 15 minutos. */
  @Property({ type: 'integer', default: 0 })
  failedLoginCount: Opt<number> = 0;

  @Property({ type: 'datetime', nullable: true })
  lockedUntil: Opt<Date> | null = null;

  /** Desativado pelo admin: não entra e todas as sessões são encerradas. Reversível. */
  @Property({ type: 'datetime', nullable: true })
  deactivatedAt: Opt<Date> | null = null;

  /** Último login com sucesso (só informativo, para a tela do admin). */
  @Property({ type: 'datetime', nullable: true })
  lastLoginAt: Opt<Date> | null = null;
}
