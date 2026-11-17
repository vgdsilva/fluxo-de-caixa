import { type AuditEntry, auditQuerySchema } from '@fluxo/contracts';
import { Controller, Get, Query, UseGuards } from '@nestjs/common';
import { createZodDto } from 'nestjs-zod';
import { RequireAdminGuard } from '../../common/guards/require-admin.guard';
import { AuditService } from './audit.service';

class AuditQueryDto extends createZodDto(auditQuerySchema) {}

// GET /api/audit?entityType=User&action=update&limit=50 — só admin. A tela vem na fatia 6.
@Controller('audit')
@UseGuards(RequireAdminGuard)
export class AuditController {
  constructor(private readonly audit: AuditService) {}

  @Get()
  search(@Query() query: AuditQueryDto): Promise<AuditEntry[]> {
    return this.audit.search(query);
  }
}
