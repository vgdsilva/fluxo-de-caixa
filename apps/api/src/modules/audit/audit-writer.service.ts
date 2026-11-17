import { EntityManager } from '@mikro-orm/postgresql';
import { Injectable } from '@nestjs/common';
import { AppContext, type Channel } from '../../common/context/app-context';
import { AuditEntry } from './audit-entry.entity';

export interface AuditEvent {
  /** login, login_failed, logout, 2fa_reset, invite_created, invite_revoked... */
  action: string;
  channel?: Channel;
  /** Quem fez; por padrão o usuário da requisição. */
  userId?: string | null;
  entityType?: string;
  entityId?: string;
  metadata?: Record<string, unknown>;
}

/**
 * Grava eventos que não passam por entidades (login, login falho, reset de 2FA...).
 * Usado pelos serviços de autenticação a partir da fatia 4.
 *
 * Grava num EntityManager próprio e na hora: o evento fica registrado mesmo que a requisição
 * termine em erro (um login falho, por exemplo, responde 401 mas precisa ficar na auditoria).
 */
@Injectable()
export class AuditWriter {
  constructor(private readonly em: EntityManager) {}

  async record(event: AuditEvent): Promise<void> {
    const em = this.em.fork();
    em.create(AuditEntry, {
      userId: event.userId === undefined ? (AppContext.userId() ?? null) : event.userId,
      channel: event.channel ?? 'auth',
      action: event.action,
      entityType: event.entityType ?? null,
      entityId: event.entityId ?? null,
      metadata: event.metadata ?? null,
      correlationId: AppContext.correlationId() ?? crypto.randomUUID(),
    });
    await em.flush();
  }
}
