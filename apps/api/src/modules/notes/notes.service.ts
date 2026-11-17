import type { CreateNote, Note as NoteResponse, UpdateNote } from '@fluxo/contracts';
import { EntityManager } from '@mikro-orm/postgresql';
import { ConflictException, Injectable, NotFoundException } from '@nestjs/common';
import { Note } from './note.entity';

/**
 * Casos de uso de anotações. Repare que nenhuma consulta filtra por dono nem por deletedAt:
 * os filtros globais fazem isso. Um id de outro usuário simplesmente "não existe" (404).
 */
@Injectable()
export class NotesService {
  // EntityManager por requisição (o MikroORM cria um fork por request, como um DbContext scoped).
  constructor(private readonly em: EntityManager) {}

  async list(): Promise<NoteResponse[]> {
    const notes = await this.em.find(Note, {}, { orderBy: { createdAt: 'desc' } });
    return notes.map(toResponse);
  }

  async get(id: string): Promise<NoteResponse> {
    return toResponse(await this.findOrThrow(id));
  }

  async create(input: CreateNote): Promise<NoteResponse> {
    const note = this.em.create(Note, { title: input.title, body: input.body });
    await this.em.flush();
    return toResponse(note);
  }

  async update(id: string, input: UpdateNote): Promise<NoteResponse> {
    const note = await this.findOrThrow(id);
    if (note.version !== input.version) {
      throw new ConflictException(
        'A anotação foi alterada por outra pessoa. Recarregue e tente de novo.',
      );
    }
    if (input.title !== undefined) note.title = input.title;
    if (input.body !== undefined) note.body = input.body;
    await this.em.flush();
    return toResponse(note);
  }

  async remove(id: string): Promise<void> {
    const note = await this.findOrThrow(id);
    // Parece um delete, mas o subscriber transforma em update de deletedAt.
    this.em.remove(note);
    await this.em.flush();
  }

  private async findOrThrow(id: string): Promise<Note> {
    const note = await this.em.findOne(Note, { id });
    if (!note) throw new NotFoundException('Anotação não encontrada.');
    return note;
  }
}

function toResponse(note: Note): NoteResponse {
  return {
    id: note.id,
    title: note.title,
    body: note.body,
    version: note.version,
    createdAt: note.createdAt.toISOString(),
    updatedAt: note.updatedAt.toISOString(),
  };
}
