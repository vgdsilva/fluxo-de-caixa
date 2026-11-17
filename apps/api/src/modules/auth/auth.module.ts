import { Module } from '@nestjs/common';
import { APP_GUARD } from '@nestjs/core';
import { RolesGuard } from '../../common/guards/roles.guard';
import { SessionGuard } from '../../common/guards/session.guard';
import { AuditModule } from '../audit/audit.module';
import { UsersModule } from '../users/users.module';
import { AuthController } from './auth.controller';
import { AuthService } from './auth.service';
import { FirstAdminService } from './first-admin.service';
import { PasswordService } from './password.service';
import { SessionsService } from './sessions.service';

/**
 * Autenticação. Registra os guards globais (APP_GUARD = vale para toda rota da API, como um
 * filtro global de autorização no ASP.NET). A ordem dos providers é a ordem de execução:
 * primeiro a sessão, depois o papel.
 */
@Module({
  imports: [UsersModule, AuditModule],
  controllers: [AuthController],
  providers: [
    AuthService,
    PasswordService,
    SessionsService,
    FirstAdminService,
    { provide: APP_GUARD, useClass: SessionGuard },
    { provide: APP_GUARD, useClass: RolesGuard },
  ],
  exports: [PasswordService],
})
export class AuthModule {}
