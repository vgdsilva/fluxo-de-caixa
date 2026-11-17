import type { AdminUser, ChangePassword, SessionInfo } from '@fluxo/contracts';
import {
  BadRequestException,
  ForbiddenException,
  Injectable,
  NotFoundException,
} from '@nestjs/common';
import { AppContext } from '../../common/context/app-context';
import { assertStrongPassword } from '../../common/validators/password-policy';
import { AuditWriter } from '../audit/audit-writer.service';
import { UsersService } from '../users/users.service';
import { PasswordService } from './password.service';
import { SessionsService } from './sessions.service';

/**
 * Segurança da conta: o próprio usuário (trocar senha, ver e encerrar sessões) e o admin
 * (listar, desativar e reativar usuários). Fica no módulo auth porque mexe em senha e sessões.
 */
@Injectable()
export class AccountService {
  constructor(
    private readonly users: UsersService,
    private readonly passwords: PasswordService,
    private readonly sessions: SessionsService,
    private readonly audit: AuditWriter,
  ) {}

  /** Troca a senha conferindo a atual; encerra as sessões dos outros aparelhos. */
  async changePassword(input: ChangePassword): Promise<{ otherSessionsRevoked: number }> {
    const userId = AppContext.userId()!;
    const ok = await this.passwords.verify(
      await this.users.getPasswordHash(userId),
      input.currentPassword,
    );
    if (!ok) throw new BadRequestException('A senha atual não confere.');
    if (input.currentPassword === input.newPassword) {
      throw new BadRequestException('A senha nova precisa ser diferente da atual.');
    }
    const me = await this.users.getMe(userId);
    assertStrongPassword(input.newPassword, me);

    await this.users.setPassword(userId, await this.passwords.hash(input.newPassword));
    const revoked = await this.sessions.revokeOthers(userId, AppContext.sessionId()!);
    await this.audit.record({
      action: 'password_changed',
      channel: 'ui',
      entityType: 'User',
      entityId: userId,
      metadata: { otherSessionsRevoked: revoked },
    });
    return { otherSessionsRevoked: revoked };
  }

  async listMySessions(): Promise<SessionInfo[]> {
    const current = AppContext.sessionId();
    const sessions = await this.sessions.listActive(AppContext.userId()!);
    return sessions.map((s) => ({
      id: s.id,
      createdAt: s.createdAt.toISOString(),
      lastSeenAt: s.lastSeenAt.toISOString(),
      ip: s.ip,
      userAgent: s.userAgent,
      current: s.id === current,
    }));
  }

  async revokeMySession(sessionId: string): Promise<void> {
    if (!(await this.sessions.revokeOwn(AppContext.userId()!, sessionId))) {
      throw new NotFoundException('Sessão não encontrada.');
    }
    await this.audit.record({ action: 'session_revoked', channel: 'ui', metadata: { sessionId } });
  }

  async revokeMyOtherSessions(): Promise<{ revoked: number }> {
    const revoked = await this.sessions.revokeOthers(AppContext.userId()!, AppContext.sessionId()!);
    await this.audit.record({
      action: 'other_sessions_revoked',
      channel: 'ui',
      metadata: { revoked },
    });
    return { revoked };
  }

  listUsers(): Promise<AdminUser[]> {
    return this.users.listForAdmin();
  }

  /** Desativar encerra todas as sessões do usuário. O admin não pode desativar a si mesmo. */
  async setUserActive(userId: string, active: boolean): Promise<void> {
    if (!active && userId === AppContext.userId()) {
      throw new ForbiddenException('Você não pode desativar a própria conta.');
    }
    AppContext.setChannel('ui');
    await this.users.setActive(userId, active);
    if (!active) await this.sessions.revokeAllForUser(userId);
  }
}
