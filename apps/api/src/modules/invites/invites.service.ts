import type {
  AcceptInvite,
  CreatedInvite,
  CreateInvite,
  Invite as InviteResponse,
  InviteCheck,
} from '@fluxo/contracts';
import { LockMode } from '@mikro-orm/core';
import { EntityManager } from '@mikro-orm/postgresql';
import { BadRequestException, Injectable, NotFoundException } from '@nestjs/common';
import { ConfigService } from '@nestjs/config';
import { AppContext } from '../../common/context/app-context';
import { generateToken, hashToken } from '../../common/utils/token';
import { assertStrongPassword } from '../../common/validators/password-policy';
import type { Env } from '../../config/env';
import { PasswordService } from '../auth/password.service';
import { UsersService } from '../users/users.service';
import { Invite } from './invite.entity';

const INVITE_TTL_MS = 72 * 60 * 60 * 1000;
const INVALID = 'Convite inválido, expirado ou já usado.';

/**
 * Convites (spec da fase 0): cadastro só existe por convite. O admin cria, copia o link (que só
 * aparece uma vez) e manda pelo WhatsApp. Uso único, 72 h, revogável, opcionalmente preso a um
 * e-mail. Criação, uso e revogação ficam na auditoria (Invite herda de BaseEntity).
 */
@Injectable()
export class InvitesService {
  constructor(
    private readonly em: EntityManager,
    private readonly users: UsersService,
    private readonly passwords: PasswordService,
    private readonly config: ConfigService<Env, true>,
  ) {}

  async create(input: CreateInvite): Promise<CreatedInvite> {
    const { token, hash } = generateToken();
    const expiresAt = new Date(Date.now() + INVITE_TTL_MS);
    const invite = this.em.create(Invite, {
      tokenHash: hash,
      email: input.email?.toLowerCase() ?? null,
      expiresAt,
      createdByUserId: AppContext.userId()!,
    });
    await this.em.flush();
    return {
      id: invite.id,
      link: `${this.config.get('APP_URL', { infer: true })}/convite?token=${token}`,
      expiresAt: expiresAt.toISOString(),
    };
  }

  async list(): Promise<InviteResponse[]> {
    const invites = await this.em.find(Invite, {}, { orderBy: { createdAt: 'desc' } });
    const now = Date.now();
    return invites.map((i) => ({
      id: i.id,
      email: i.email,
      status: i.usedAt
        ? 'used'
        : i.revokedAt
          ? 'revoked'
          : i.expiresAt.getTime() <= now
            ? 'expired'
            : 'pending',
      createdAt: i.createdAt.toISOString(),
      expiresAt: i.expiresAt.toISOString(),
      usedAt: i.usedAt?.toISOString() ?? null,
    }));
  }

  async revoke(id: string): Promise<void> {
    const invite = await this.em.findOne(Invite, { id });
    if (!invite) throw new NotFoundException('Convite não encontrado.');
    if (invite.usedAt) throw new BadRequestException('Este convite já foi usado.');
    if (!invite.revokedAt) {
      invite.revokedAt = new Date();
      await this.em.flush();
    }
  }

  async check(token: string): Promise<InviteCheck> {
    const invite = await this.findValid(token);
    return { valid: !!invite, email: invite?.email ?? null };
  }

  /**
   * O convidado cria a conta (papel user). Convite marcado como usado e usuário criado na mesma
   * transação, com a linha do convite travada: o mesmo link não cria duas contas.
   */
  async accept(input: AcceptInvite): Promise<void> {
    assertStrongPassword(input.password, input);
    const passwordHash = await this.passwords.hash(input.password);
    AppContext.setChannel('auth');

    await this.em.transactional(async (em) => {
      const invite = await this.findValid(input.token, em, true);
      if (!invite) throw new BadRequestException(INVALID);
      if (invite.email && invite.email !== input.email.toLowerCase()) {
        throw new BadRequestException('Este convite é para outro e-mail.');
      }
      const userId = await this.users.createUser({
        username: input.username,
        name: input.name,
        email: input.email.toLowerCase(),
        role: 'user',
        passwordHash,
      });
      invite.usedAt = new Date();
      invite.usedByUserId = userId;
      await em.flush();
    });
  }

  private findValid(token: string, em = this.em, lock = false): Promise<Invite | null> {
    return em.findOne(
      Invite,
      {
        tokenHash: hashToken(token),
        usedAt: null,
        revokedAt: null,
        expiresAt: { $gt: new Date() },
      },
      lock ? { lockMode: LockMode.PESSIMISTIC_WRITE } : {},
    );
  }
}
