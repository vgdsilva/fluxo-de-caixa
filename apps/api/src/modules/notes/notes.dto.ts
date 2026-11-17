import { createNoteSchema, updateNoteSchema } from '@fluxo/contracts';
import { createZodDto } from 'nestjs-zod';

// DTOs gerados a partir dos schemas de @fluxo/contracts. O ZodValidationPipe global valida o body
// com o schema; nada de class-validator nem tipo escrito à mão.
export class CreateNoteDto extends createZodDto(createNoteSchema) {}
export class UpdateNoteDto extends createZodDto(updateNoteSchema) {}
