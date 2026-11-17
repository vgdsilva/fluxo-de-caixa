import type { Me, UpdateMe } from '@fluxo/contracts';
import { EntityManager } from '@mikro-orm/postgresql';
import { ConflictException, Injectable, NotFoundException } from '@nestjs/common';
import type { Role } from '../../common/context/app-context';
import { User } from './user.entity';

@Injectable()
export class UsersService {
  constructor(private readonly em: EntityManager) {}

  /** Id e papel do usuário ativo com este username, ou undefined. */
  async findIdentityByUsername(username: string): Promise<{ id: string; role: Role } | undefined> {
    const user = await this.em.findOne(User, { username }, { fields: ['id', 'role'] });
    return user ? { id: user.id, role: user.role } : undefined;
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
