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

  @Enum({ items: ['admin', 'user'], default: 'user' })
  role: Opt<UserRole> = 'user';

  /**
   * Hash argon2id da senha. `hidden`: nunca sai em serialização e aparece como "***" na auditoria.
   * null = usuário ainda sem senha definida (não consegue entrar).
   */
  @Property({ type: 'text', nullable: true, hidden: true, lazy: true })
  passwordHash: Opt<string> | null = null;
}
