import { z } from 'zod';

export const auditChannelSchema = z.enum(['ui', 'chat', 'import', 'job', 'auth']);
export type AuditChannel = z.infer<typeof auditChannelSchema>;

/** Uma linha da auditoria. `changes` traz só os campos alterados, como { campo: { from, to } }. */
export const auditEntrySchema = z.object({
  id: z.uuid(),
  at: z.iso.datetime(),
  userId: z.uuid().nullable(),
  recordOwnerId: z.uuid().nullable(),
  channel: auditChannelSchema,
  action: z.string(),
  entityType: z.string().nullable(),
  entityId: z.string().nullable(),
  changes: z.record(z.string(), z.object({ from: z.unknown(), to: z.unknown() })).nullable(),
  metadata: z.record(z.string(), z.unknown()).nullable(),
  correlationId: z.uuid(),
});
export type AuditEntry = z.infer<typeof auditEntrySchema>;

/** Filtros de GET /api/audit (query string). Tudo opcional; mais recentes primeiro. */
export const auditQuerySchema = z.object({
  userId: z.uuid().optional(),
  recordOwnerId: z.uuid().optional(),
  entityType: z.string().max(80).optional(),
  entityId: z.string().max(80).optional(),
  action: z.string().max(40).optional(),
  channel: auditChannelSchema.optional(),
  correlationId: z.uuid().optional(),
  from: z.iso.datetime().optional(),
  to: z.iso.datetime().optional(),
  limit: z.coerce.number().int().min(1).max(500).default(100),
});
export type AuditQuery = z.infer<typeof auditQuerySchema>;
