import type { LoginInput, Me } from '@fluxo/contracts';
import { Injectable, UnauthorizedException } from '@nestjs/common';
import { AppContext } from '../../common/context/app-context';
import { AuditWriter } from '../audit/audit-writer.service';
import { LOCK_DURATION_MS, UsersService } from '../users/users.service';
import { PasswordService } from './password.service';
import { SessionsService } from './sessions.service';

export interface ClientInfo {
  ip?: string;
  userAgent?: string;
}

/** Mensagem única para usuário inexistente e senha errada: não revela se o usuário existe. */
const INVALID_CREDENTIALS = 'Usuário ou senha inválidos.';
const LOCKED = 'Muitas tentativas erradas. Aguarde 15 minutos e tente de novo.';

@Injectable()
export class AuthService {
  constructor(
    private readonly users: UsersService,
    private readonly passwords: PasswordService,
    private readonly sessions: SessionsService,
    private readonly audit: AuditWriter,
  ) {}

  /**
   * Senha confere: abre uma sessão nova (o id de sessão muda a cada login) e devolve o token do
   * cookie. Login e falha ficam na auditoria. 5 senhas erradas seguidas bloqueiam por 15 min.
   */
  async login(input: LoginInput, client: ClientInfo): Promise<{ token: string; me: Me }> {
    const user = await this.users.findCredentials(input.username);
    const clientMeta = { ip: client.ip ?? null, userAgent: client.userAgent ?? null };

    // Conta bloqueada por tentativas: nem confere a senha.
    if (user?.lockedUntil && user.lockedUntil.getTime() > Date.now()) {
      await this.audit.record({
        action: 'login_failed',
        userId: user.id,
        metadata: { username: input.username, reason: 'locked', ...clientMeta },
      });
      throw new UnauthorizedException(LOCKED);
    }

    const ok = await this.passwords.verify(user?.passwordHash, input.password);

    if (!user || !ok) {
      await this.audit.record({
        action: 'login_failed',
        userId: user?.id ?? null,
        metadata: {
          username: input.username,
          reason: !user ? 'unknown_user' : 'bad_password',
          ...clientMeta,
        },
      });
      if (user) {
        const { locked } = await this.users.registerFailedLogin(user.id);
        if (locked) {
          await this.audit.record({
            action: 'account_locked',
            userId: user.id,
            entityType: 'User',
            entityId: user.id,
            metadata: { minutes: LOCK_DURATION_MS / 60000, ...clientMeta },
          });
          throw new UnauthorizedException(LOCKED);
        }
      }
      throw new UnauthorizedException(INVALID_CREDENTIALS);
    }

    await this.users.clearFailedLogins(user.id);
    const token = await this.sessions.create(user.id, client);
    AppContext.setUser(user.id, user.role);
    await this.audit.record({
      action: 'login',
      userId: user.id,
      entityType: 'User',
      entityId: user.id,
      metadata: { ip: client.ip ?? null, userAgent: client.userAgent ?? null },
    });
    return { token, me: await this.users.getMe(user.id) };
  }

  async logout(token: string | undefined): Promise<void> {
    if (!token) return;
    const session = await this.sessions.revoke(token);
    if (session) {
      await this.audit.record({
        action: 'logout',
        userId: session.userId,
        entityType: 'User',
        entityId: session.userId,
      });
    }
  }
}
