import type { Note } from '@fluxo/contracts';
import {
  Body,
  Controller,
  Delete,
  Get,
  HttpCode,
  Param,
  ParseUUIDPipe,
  Patch,
  Post,
  UseGuards,
} from '@nestjs/common';
import { RequireUserGuard } from '../../common/guards/require-user.guard';
import { CreateNoteDto, UpdateNoteDto } from './notes.dto';
import { NotesService } from './notes.service';

// /api/notes. O controller só traduz HTTP para o serviço; regra fica no NotesService.
@Controller('notes')
@UseGuards(RequireUserGuard)
export class NotesController {
  constructor(private readonly notes: NotesService) {}

  @Get()
  list(): Promise<Note[]> {
    return this.notes.list();
  }

  @Get(':id')
  get(@Param('id', ParseUUIDPipe) id: string): Promise<Note> {
    return this.notes.get(id);
  }

  @Post()
  create(@Body() body: CreateNoteDto): Promise<Note> {
    return this.notes.create(body);
  }

  @Patch(':id')
  update(@Param('id', ParseUUIDPipe) id: string, @Body() body: UpdateNoteDto): Promise<Note> {
    return this.notes.update(id, body);
  }

  @Delete(':id')
  @HttpCode(204)
  remove(@Param('id', ParseUUIDPipe) id: string): Promise<void> {
    return this.notes.remove(id);
  }
}
