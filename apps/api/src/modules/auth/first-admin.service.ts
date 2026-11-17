import { createHash, randomBytes, timingSafeEqual } from 'node:crypto';
import { MikroORM, RequestContext } from '@mikro-orm/postgresql';
import { Injectable, Logger, type OnApplicationBootstrap } from '@nestjs/common';
import { ConfigService } from '@nestjs/config';
import type { Env } from '../../config/env';
import { UsersService } from '../users/users.service';

/**
 * Primeiro admin (spec da fase 0). No start, se o banco não tem nenhum usuário, gera um link de
 * configuração de uso único e escreve no log. O token vive só na memória: reiniciar a API gera
 * outro. Depois que o admin existe, o link deixa de valer e as rotas de setup respondem 404.
 */
@Injectable()
export class FirstAdminService implements OnApplicationBootstrap {
  private readonly logger = new Logger('PrimeiroAdmin');
  private tokenHash: Buffer | null = null;

  constructor(
    private readonly users: UsersService,
    private readonly config: ConfigService<Env, true>,
    private readonly orm: MikroORM,
  ) {}

  async onApplicationBootstrap(): Promise<void> {
    // Fora de uma requisição: abre um contexto do ORM só para esta consulta.
    const count = await RequestContext.create(this.orm.em, () => this.users.countAll());
    if (count > 0) return;
    const token = randomBytes(32).toString('base64url');
    this.tokenHash = sha256(token);
    const url = `${this.config.get('APP_URL', { infer: true })}/setup?token=${token}`;
    this.logger.warn(`Nenhum usuário cadastrado. Crie o primeiro admin em: ${url}`);
  }

  /** O setup ainda está aberto (não há usuário e o token foi gerado)? */
  isOpen(): boolean {
    return this.tokenHash !== null;
  }

  isValid(token: string): boolean {
    if (!this.tokenHash) return false;
    const candidate = sha256(token);
    return timingSafeEqual(candidate, this.tokenHash);
  }

  /** Chamado depois de criar o admin: o link morre. */
  close(): void {
    this.tokenHash = null;
  }
}

function sha256(value: string): Buffer {
  return createHash('sha256').update(value).digest();
}
