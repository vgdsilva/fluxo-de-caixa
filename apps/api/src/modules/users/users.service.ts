import type { Me, UpdateMe } from '@fluxo/contracts';
import { EntityManager } from '@mikro-orm/postgresql';
import { ConflictException, Injectable, NotFoundException } from '@nestjs/common';
import type { Role } from '../../common/context/app-context';
import { User } from './user.entity';

/** O que a autenticação precisa saber de um usuário (o hash nunca sai do módulo auth). */
export interface UserCredentials {
  id: string;
  role: Role;
  name: string;
  passwordHash: string | null;
}

@Injectable()
export class UsersService {
  constructor(private readonly em: EntityManager) {}

  /** Para o login: usuário ativo pelo username, com o hash da senha. */
  async findCredentials(username: string): Promise<UserCredentials | undefined> {
    const user = await this.em.findOne(User, { username }, { populate: ['passwordHash'] });
    if (!user) return undefined;
    return { id: user.id, role: user.role, name: user.name, passwordHash: user.passwordHash };
  }

  /** Papel do usuário ativo (usado a cada requisição pelo SessionGuard). */
  async findRole(userId: string): Promise<Role | undefined> {
    const user = await this.em.findOne(User, { id: userId }, { fields: ['role'] });
    return user?.role;
  }

  async createUser(input: {
    username: string;
    name: string;
    role: Role;
    passwordHash: string;
  }): Promise<string> {
    const user = this.em.create(User, input);
    await this.em.flush();
    return user.id;
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
    role: user.role,
    version: user.version,
  };
}
