import type { AuditEntry as AuditEntryResponse, AuditQuery } from '@fluxo/contracts';
import type { FilterQuery } from '@mikro-orm/core';
import { EntityManager } from '@mikro-orm/postgresql';
import { Injectable } from '@nestjs/common';
import { AuditEntry } from './audit-entry.entity';

/** Consulta da auditoria para a tela do admin (o admin vê tudo, decisão de 17/11/2026). */
@Injectable()
export class AuditService {
  constructor(private readonly em: EntityManager) {}

  async search(query: AuditQuery): Promise<AuditEntryResponse[]> {
    const where: FilterQuery<AuditEntry> = {};
    if (query.userId) where.userId = query.userId;
    if (query.recordOwnerId) where.recordOwnerId = query.recordOwnerId;
    if (query.entityType) where.entityType = query.entityType;
    if (query.entityId) where.entityId = query.entityId;
    if (query.action) where.action = query.action;
    if (query.channel) where.channel = query.channel;
    if (query.correlationId) where.correlationId = query.correlationId;
    if (query.from || query.to) {
      where.at = {
        ...(query.from ? { $gte: new Date(query.from) } : {}),
        ...(query.to ? { $lte: new Date(query.to) } : {}),
      };
    }

    const entries = await this.em.find(AuditEntry, where, {
      orderBy: { at: 'desc', id: 'desc' },
      limit: query.limit,
    });
    return entries.map((e) => ({
      id: e.id,
      at: e.at.toISOString(),
      userId: e.userId,
      recordOwnerId: e.recordOwnerId,
      channel: e.channel,
      action: e.action,
      entityType: e.entityType,
      entityId: e.entityId,
      changes: e.changes,
      metadata: e.metadata,
      correlationId: e.correlationId,
    }));
  }
}
