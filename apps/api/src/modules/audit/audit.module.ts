import { Module } from '@nestjs/common';
import { AuditController } from './audit.controller';
import { AuditService } from './audit.service';
import { AuditWriter } from './audit-writer.service';

/**
 * Auditoria. A gravação automática das alterações não passa por aqui: é o AuditSubscriber,
 * registrado direto no MikroORM (database/mikro-orm.config.ts). Este módulo expõe o AuditWriter
 * (eventos de login etc.) para os outros módulos e a consulta para o admin.
 */
@Module({
  controllers: [AuditController],
  providers: [AuditService, AuditWriter],
  exports: [AuditWriter],
})
export class AuditModule {}
