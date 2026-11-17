import type { LoginInput, Me, SetupInput } from '@fluxo/contracts';
import { Injectable, NotFoundException, UnauthorizedException } from '@nestjs/common';
import { AppContext } from '../../common/context/app-context';
import { AuditWriter } from '../audit/audit-writer.service';
import { UsersService } from '../users/users.service';
import { FirstAdminService } from './first-admin.service';
import { PasswordService } from './password.service';
import { SessionsService } from './sessions.service';

export interface ClientInfo {
  ip?: string;
  userAgent?: string;
}

/** Mensagem única para usuário inexistente e senha errada: não revela se o usuário existe. */
const INVALID_CREDENTIALS = 'Usuário ou senha inválidos.';

@Injectable()
export class AuthService {
  constructor(
    private readonly users: UsersService,
    private readonly passwords: PasswordService,
    private readonly sessions: SessionsService,
    private readonly firstAdmin: FirstAdminService,
    private readonly audit: AuditWriter,
  ) {}

  /**
   * Senha confere: abre uma sessão nova (o id de sessão muda a cada login) e devolve o token do
   * cookie. Login e falha ficam na auditoria. O segundo fator entra na fatia 5.
   */
  async login(input: LoginInput, client: ClientInfo): Promise<{ token: string; me: Me }> {
    const user = await this.users.findCredentials(input.username);
    const ok = await this.passwords.verify(user?.passwordHash, input.password);

    if (!user || !ok) {
      await this.audit.record({
        action: 'login_failed',
        userId: user?.id ?? null,
        metadata: {
          username: input.username,
          reason: !user ? 'unknown_user' : 'bad_password',
          ip: client.ip ?? null,
          userAgent: client.userAgent ?? null,
        },
      });
      throw new UnauthorizedException(INVALID_CREDENTIALS);
    }

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

  setupStatus(token: string): boolean {
    if (!this.firstAdmin.isOpen()) throw new NotFoundException();
    return this.firstAdmin.isValid(token);
  }

  /** Cria o primeiro admin a partir do link do log. Depois disso o setup fecha para sempre. */
  async setup(input: SetupInput): Promise<void> {
    if (!this.firstAdmin.isOpen()) throw new NotFoundException();
    if (!this.firstAdmin.isValid(input.token)) {
      throw new UnauthorizedException('Link de configuração inválido.');
    }
    if ((await this.users.countAll()) > 0) {
      this.firstAdmin.close();
      throw new NotFoundException();
    }
    AppContext.setChannel('auth');
    const userId = await this.users.createUser({
      username: input.username,
      name: input.name,
      role: 'admin',
      passwordHash: await this.passwords.hash(input.password),
    });
    this.firstAdmin.close();
    await this.audit.record({
      action: 'first_admin_created',
      userId,
      entityType: 'User',
      entityId: userId,
    });
  }
}
