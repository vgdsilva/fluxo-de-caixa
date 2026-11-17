import {
  type ChangeSet,
  ChangeSetType,
  type EventSubscriber,
  type FlushEventArgs,
} from '@mikro-orm/core';
import { AppContext } from '../../common/context/app-context';
import { BaseEntity } from '../../database/entities/base.entity';
import { OwnedEntity } from '../../database/entities/owned.entity';
import { type AuditChange, AuditEntry } from './audit-entry.entity';

/** Campos que mudam em todo update e só poluiriam o histórico. */
const IGNORED = new Set(['id', 'createdAt', 'updatedAt', 'version']);
const MASK = '***';

/**
 * Auditoria automática: para cada entidade alterada no flush, grava uma AuditEntry no MESMO flush
 * e na MESMA transação. Se a alteração falhar, a auditoria some junto, e vice-versa.
 *
 * Roda depois do BaseEntitySubscriber (ordem em mikro-orm.config.ts), então já enxerga o ownerId
 * preenchido e o remove convertido em soft delete.
 *
 * Campos marcados como `hidden: true` na entidade (hash de senha, segredo de 2FA) aparecem
 * mascarados como "***".
 */
export class AuditSubscriber implements EventSubscriber {
  onFlush({ em, uow }: FlushEventArgs): void {
    // Cópia: vamos adicionar change sets (as próprias AuditEntry) durante o laço.
    const changeSets = [...uow.getChangeSets()];

    for (const changeSet of changeSets) {
      const entity = changeSet.entity as object;
      if (!(entity instanceof BaseEntity)) continue;

      const audit = toAudit(changeSet as unknown as ChangeSet<BaseEntity>);
      if (!audit) continue;

      const entry = em.create(AuditEntry, {
        userId: AppContext.userId() ?? null,
        recordOwnerId: entity instanceof OwnedEntity ? entity.ownerId : null,
        channel: AppContext.channel(),
        action: audit.action,
        entityType: changeSet.meta.className,
        entityId: entity.id,
        changes: audit.changes,
        correlationId: AppContext.correlationId() ?? entity.id,
      });
      // Entidade criada dentro do onFlush precisa ter o change set calculado à mão.
      uow.computeChangeSet(entry);
    }
  }
}

function toAudit(
  changeSet: ChangeSet<BaseEntity>,
): { action: string; changes: Record<string, AuditChange> } | undefined {
  const payload = changeSet.payload as Record<string, unknown>;
  const original = (changeSet.originalEntity ?? {}) as Record<string, unknown>;
  const isCreate = changeSet.type === ChangeSetType.CREATE;

  const changes: Record<string, AuditChange> = {};
  for (const [prop, newValue] of Object.entries(payload)) {
    if (IGNORED.has(prop)) continue;
    const hidden = changeSet.meta.properties[prop as keyof BaseEntity]?.hidden === true;
    const from = isCreate ? null : normalize(original[prop]);
    const to = normalize(newValue);
    // Na criação, campo vazio não é informação; no update, só o que de fato mudou.
    if (isCreate ? to === null : JSON.stringify(from) === JSON.stringify(to)) continue;
    changes[prop] = hidden ? { from: from === null ? null : MASK, to: MASK } : { from, to };
  }

  if (isCreate) return { action: 'create', changes };
  if (changeSet.type !== ChangeSetType.UPDATE) return undefined;
  if (Object.keys(changes).length === 0) return undefined;

  let action = 'update';
  if ('deletedAt' in changes) {
    action = changes.deletedAt.to === null ? 'restore' : 'delete';
  }
  return { action, changes };
}

function normalize(value: unknown): unknown {
  if (value === undefined) return null;
  if (value instanceof Date) return value.toISOString();
  return value;
}
