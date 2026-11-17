import { EntityManager } from '@mikro-orm/postgresql';
import { Injectable } from '@nestjs/common';
import { generateToken, hashToken } from '../../common/utils/token';
import { Session } from './session.entity';

export const SESSION_COOKIE = 'fluxo_sid';
/** Expiração deslizante: 7 dias sem usar, a sessão morre. */
export const SESSION_TTL_MS = 7 * 24 * 60 * 60 * 1000;
/** Para não escrever no banco a cada requisição, só renova se a última renovação tem mais de 5 min. */
const TOUCH_AFTER_MS = 5 * 60 * 1000;

export interface ActiveSession {
  sessionId: string;
  userId: string;
}

@Injectable()
export class SessionsService {
  constructor(private readonly em: EntityManager) {}

  /** Cria a sessão e devolve o token que vai no cookie (só existe aqui e no navegador). */
  async create(userId: string, info: { ip?: string; userAgent?: string }): Promise<string> {
    const { token, hash } = generateToken();
    const em = this.em.fork();
    em.create(Session, {
      tokenHash: hash,
      userId,
      expiresAt: new Date(Date.now() + SESSION_TTL_MS),
      ip: info.ip?.slice(0, 64) ?? null,
      userAgent: info.userAgent?.slice(0, 300) ?? null,
    });
    await em.flush();
    return token;
  }

  /** Sessão válida para o token, renovando a expiração; undefined se expirada, revogada ou inexistente. */
  async validate(token: string): Promise<ActiveSession | undefined> {
    const em = this.em.fork();
    const session = await em.findOne(Session, { tokenHash: hashToken(token), revokedAt: null });
    const now = Date.now();
    if (!session || session.expiresAt.getTime() <= now) return undefined;

    if (now - session.lastSeenAt.getTime() > TOUCH_AFTER_MS) {
      session.lastSeenAt = new Date(now);
      session.expiresAt = new Date(now + SESSION_TTL_MS);
      await em.flush();
    }
    return { sessionId: session.id, userId: session.userId };
  }

  async revoke(token: string): Promise<ActiveSession | undefined> {
    const em = this.em.fork();
    const session = await em.findOne(Session, { tokenHash: hashToken(token), revokedAt: null });
    if (!session) return undefined;
    session.revokedAt = new Date();
    await em.flush();
    return { sessionId: session.id, userId: session.userId };
  }

  /** Sessões abertas (não revogadas e não expiradas) do usuário, mais recentes primeiro. */
  async listActive(userId: string): Promise<Session[]> {
    return this.em.find(
      Session,
      { userId, revokedAt: null, expiresAt: { $gt: new Date() } },
      { orderBy: { lastSeenAt: 'desc' } },
    );
  }

  /** Encerra uma sessão do próprio usuário (o filtro por userId impede mexer na de outro). */
  async revokeOwn(userId: string, sessionId: string): Promise<boolean> {
    const em = this.em.fork();
    const count = await em.nativeUpdate(
      Session,
      { id: sessionId, userId, revokedAt: null },
      { revokedAt: new Date() },
    );
    return count > 0;
  }

  /** Encerra todas as sessões do usuário menos a informada ("sair dos outros aparelhos"). */
  async revokeOthers(userId: string, keepSessionId: string): Promise<number> {
    const em = this.em.fork();
    return em.nativeUpdate(
      Session,
      { userId, revokedAt: null, id: { $ne: keepSessionId } },
      { revokedAt: new Date() },
    );
  }

  /** Encerra todas as sessões ativas do usuário (troca de senha, reset de 2FA, desativação). */
  async revokeAllForUser(userId: string): Promise<number> {
    const em = this.em.fork();
    return em.nativeUpdate(Session, { userId, revokedAt: null }, { revokedAt: new Date() });
  }
}
