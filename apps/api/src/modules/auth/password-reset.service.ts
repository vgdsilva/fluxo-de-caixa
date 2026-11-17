import type { CreatedPasswordReset, PasswordResetCheck, ResetPassword } from '@fluxo/contracts';
import { EntityManager } from '@mikro-orm/postgresql';
import { BadRequestException, Injectable, NotFoundException } from '@nestjs/common';
import { ConfigService } from '@nestjs/config';
import { AppContext } from '../../common/context/app-context';
import { generateToken, hashToken } from '../../common/utils/token';
import { assertStrongPassword } from '../../common/validators/password-policy';
import type { Env } from '../../config/env';
import { AuditWriter } from '../audit/audit-writer.service';
import { UsersService } from '../users/users.service';
import { PasswordReset } from './password-reset.entity';
import { PasswordService } from './password.service';
import { SessionsService } from './sessions.service';

const RESET_TTL_MS = 60 * 60 * 1000;

/**
 * "Esqueci a senha" sem e-mail: o admin gera um link de uso único (1 h) e manda pelo WhatsApp.
 * Ao redefinir, todas as sessões do usuário são encerradas. O segundo fator continua exigido
 * no próximo login (fatia 5, parte 2).
 */
@Injectable()
export class PasswordResetService {
  constructor(
    private readonly em: EntityManager,
    private readonly users: UsersService,
    private readonly passwords: PasswordService,
    private readonly sessions: SessionsService,
    private readonly audit: AuditWriter,
    private readonly config: ConfigService<Env, true>,
  ) {}

  async create(userId: string): Promise<CreatedPasswordReset> {
    const user = await this.users.findSummary(userId);
    if (!user) throw new NotFoundException('Usuário não encontrado.');

    // Um link novo invalida os anteriores ainda não usados.
    await this.em.nativeUpdate(
      PasswordReset,
      { userId, usedAt: null, expiresAt: { $gt: new Date() } },
      { expiresAt: new Date() },
    );

    const { token, hash } = generateToken();
    const expiresAt = new Date(Date.now() + RESET_TTL_MS);
    this.em.create(PasswordReset, {
      tokenHash: hash,
      userId,
      createdByUserId: AppContext.userId()!,
      expiresAt,
    });
    await this.em.flush();

    return {
      link: `${this.config.get('APP_URL', { infer: true })}/redefinir-senha?token=${token}`,
      expiresAt: expiresAt.toISOString(),
    };
  }

  async check(token: string): Promise<PasswordResetCheck> {
    const reset = await this.findValid(token);
    if (!reset) return { valid: false, username: null };
    const user = await this.users.findSummary(reset.userId);
    return { valid: !!user, username: user?.username ?? null };
  }

  async reset(input: ResetPassword): Promise<void> {
    const reset = await this.findValid(input.token);
    if (!reset) throw new BadRequestException('Link inválido, expirado ou já usado.');
    const user = await this.users.findSummary(reset.userId);
    if (!user) throw new BadRequestException('Link inválido, expirado ou já usado.');

    assertStrongPassword(input.password, user);
    AppContext.setChannel('auth');
    reset.usedAt = new Date();
    await this.users.setPassword(user.id, await this.passwords.hash(input.password));
    await this.em.flush();
    const revoked = await this.sessions.revokeAllForUser(user.id);

    await this.audit.record({
      action: 'password_reset',
      userId: user.id,
      entityType: 'User',
      entityId: user.id,
      metadata: { sessionsRevoked: revoked },
    });
  }

  private async findValid(token: string): Promise<PasswordReset | null> {
    return this.em.findOne(PasswordReset, {
      tokenHash: hashToken(token),
      usedAt: null,
      expiresAt: { $gt: new Date() },
    });
  }
}
