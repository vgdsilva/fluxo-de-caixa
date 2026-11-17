import { EntityManager } from '@mikro-orm/postgresql';
import { Injectable } from '@nestjs/common';
import { User } from './user.entity';

@Injectable()
export class UsersService {
  constructor(private readonly em: EntityManager) {}

  /** Id do usuário ativo com este username, ou undefined. */
  async findIdByUsername(username: string): Promise<string | undefined> {
    const user = await this.em.findOne(User, { username }, { fields: ['id'] });
    return user?.id;
  }
}
