import { z } from 'zod';

/**
 * Anotações: entidade de exemplo da fatia 2, só para provar dono, soft delete e concorrência.
 * Serve de modelo para as entidades da fase 1 (contas, cartões, lançamentos).
 */
export const noteSchema = z.object({
  id: z.uuid(),
  title: z.string(),
  body: z.string(),
  /** Versão para concorrência otimista: mande de volta no PATCH. */
  version: z.number().int(),
  createdAt: z.iso.datetime(),
  updatedAt: z.iso.datetime(),
});
export type Note = z.infer<typeof noteSchema>;

export const createNoteSchema = z.object({
  title: z.string().trim().min(1, 'Informe um título').max(120),
  body: z.string().max(5000).default(''),
});
export type CreateNote = z.infer<typeof createNoteSchema>;

export const updateNoteSchema = createNoteSchema.partial().extend({
  version: z.number().int(),
});
export type UpdateNote = z.infer<typeof updateNoteSchema>;
