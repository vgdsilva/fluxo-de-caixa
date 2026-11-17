import type { Me, UpdateMe } from '@fluxo/contracts';
import { EntityManager } from '@mikro-orm/postgresql';
import { ConflictException, Injectable, NotFoundException } from '@nestjs/common';
import type { Role } from '../../common/context/app-context';
import { User } from './user.entity';

/** O que a autenticação precisa saber de um usuário (o hash nunca sai do módulo auth). */
export interface UserCredentials {
  id: string;
  username: string;
  role: Role;
  name: string;
  passwordHash: string | null;
  lockedUntil: Date | null;
}

export interface UserSummary {
  id: string;
  username: string;
  name: string;
  email: string | null;
}

/** Senhas erradas seguidas até bloquear, e por quanto tempo. */
export const MAX_FAILED_LOGINS = 5;
export const LOCK_DURATION_MS = 15 * 60 * 1000;

@Injectable()
export class UsersService {
  constructor(private readonly em: EntityManager) {}

  /** Para o login: usuário ativo pelo username, com o hash da senha e o bloqueio. */
  async findCredentials(username: string): Promise<UserCredentials | undefined> {
    const user = await this.em.findOne(User, { username }, { populate: ['passwordHash'] });
    if (!user) return undefined;
    return {
      id: user.id,
      username: user.username,
      role: user.role,
      name: user.name,
      passwordHash: user.passwordHash,
      lockedUntil: user.lockedUntil,
    };
  }

  async findSummary(userId: string): Promise<UserSummary | undefined> {
    const user = await this.em.findOne(User, { id: userId });
    if (!user) return undefined;
    return { id: user.id, username: user.username, name: user.name, email: user.email };
  }

  /** Papel do usuário ativo (usado a cada requisição pelo SessionGuard). */
  async findRole(userId: string): Promise<Role | undefined> {
    const user = await this.em.findOne(User, { id: userId }, { fields: ['role'] });
    return user?.role;
  }

  /** O username já foi usado, inclusive por usuário removido (o índice único vale para todos). */
  async usernameTaken(username: string): Promise<boolean> {
    const count = await this.em.count(User, { username }, { filters: { notDeleted: false } });
    return count > 0;
  }

  async createUser(input: {
    username: string;
    name: string;
    email?: string | null;
    role: Role;
    passwordHash: string;
  }): Promise<string> {
    if (await this.usernameTaken(input.username)) {
      throw new ConflictException('Este usuário já existe. Escolha outro.');
    }
    const user = this.em.create(User, { ...input, email: input.email ?? null });
    await this.em.flush();
    return user.id;
  }

  /** Conta uma senha errada; na 5ª seguida, bloqueia por 15 minutos e zera a contagem. */
  async registerFailedLogin(userId: string): Promise<{ locked: boolean }> {
    const user = await this.em.findOneOrFail(User, { id: userId });
    user.failedLoginCount += 1;
    let locked = false;
    if (user.failedLoginCount >= MAX_FAILED_LOGINS) {
      user.lockedUntil = new Date(Date.now() + LOCK_DURATION_MS);
      user.failedLoginCount = 0;
      locked = true;
    }
    await this.em.flush();
    return { locked };
  }

  async clearFailedLogins(userId: string): Promise<void> {
    const user = await this.em.findOneOrFail(User, { id: userId });
    if (user.failedLoginCount === 0 && user.lockedUntil === null) return;
    user.failedLoginCount = 0;
    user.lockedUntil = null;
    await this.em.flush();
  }

  /** Troca a senha (reset). Também tira o bloqueio por tentativas. */
  async setPassword(userId: string, passwordHash: string): Promise<void> {
    const user = await this.em.findOneOrFail(User, { id: userId });
    user.passwordHash = passwordHash;
    user.failedLoginCount = 0;
    user.lockedUntil = null;
    await this.em.flush();
  }

  async getMe(userId: string): Promise<Me> {
    return toMe(await this.findOrThrow(userId));
  }

  /** Alteração de perfil. Gera linha de auditoria automaticamente (AuditSubscriber). */
  async updateMe(userId: string, input: UpdateMe): Promise<Me> {
    const user = await this.findOrThrow(userId);
    if (user.version !== input.version) {
      throw new ConflictException(
        'Seu perfil foi alterado em outro lugar. Recarregue e tente de novo.',
      );
    }
    user.name = input.name;
    await this.em.flush();
    return toMe(user);
  }

  private async findOrThrow(id: string): Promise<User> {
    const user = await this.em.findOne(User, { id });
    if (!user) throw new NotFoundException('Usuário não encontrado.');
    return user;
  }
}

function toMe(user: User): Me {
  return {
    id: user.id,
    username: user.username,
    name: user.name,
    email: user.email,
    role: user.role,
    version: user.version,
  };
}
