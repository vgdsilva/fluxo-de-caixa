import { Module } from '@nestjs/common';
import { APP_GUARD } from '@nestjs/core';
import { ThrottlerGuard } from '@nestjs/throttler';
import { RolesGuard } from '../../common/guards/roles.guard';
import { SessionGuard } from '../../common/guards/session.guard';
import { AuditModule } from '../audit/audit.module';
import { UsersModule } from '../users/users.module';
import { AdminUsersController, MySecurityController } from './account.controller';
import { AccountService } from './account.service';
import { AuthController } from './auth.controller';
import { AuthService } from './auth.service';
import { AdminPasswordResetController, PasswordResetController } from './password-reset.controller';
import { PasswordResetService } from './password-reset.service';
import { PasswordService } from './password.service';
import { SessionsService } from './sessions.service';

/**
 * Autenticação. Registra os guards globais (APP_GUARD = vale para toda rota da API, como um
 * filtro global de autorização no ASP.NET). A ordem dos providers é a ordem de execução:
 * primeiro o limite de requisições, depois a sessão, depois o papel.
 */
@Module({
  imports: [UsersModule, AuditModule],
  controllers: [
    AuthController,
    AdminPasswordResetController,
    PasswordResetController,
    MySecurityController,
    AdminUsersController,
  ],
  providers: [
    AuthService,
    PasswordService,
    SessionsService,
    PasswordResetService,
    AccountService,
    // Limite de requisições por IP (rotas sensíveis baixam o limite com @Throttle).
    { provide: APP_GUARD, useClass: ThrottlerGuard },
    { provide: APP_GUARD, useClass: SessionGuard },
    { provide: APP_GUARD, useClass: RolesGuard },
  ],
  exports: [PasswordService, SessionsService],
})
export class AuthModule {}
