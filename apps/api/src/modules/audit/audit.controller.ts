import { type AuditEntry, auditQuerySchema } from '@fluxo/contracts';
import { Controller, Get, Query } from '@nestjs/common';
import { createZodDto } from 'nestjs-zod';
import { Roles } from '../../common/guards/roles.decorator';
import { AuditService } from './audit.service';

class AuditQueryDto extends createZodDto(auditQuerySchema) {}

// GET /api/audit?entityType=User&action=update&limit=50 — só admin. A tela vem na fatia 6.
@Controller('audit')
@Roles('admin')
export class AuditController {
  constructor(private readonly audit: AuditService) {}

  @Get()
  search(@Query() query: AuditQueryDto): Promise<AuditEntry[]> {
    return this.audit.search(query);
  }
}
