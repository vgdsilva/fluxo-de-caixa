import { Module } from '@nestjs/common';
import { AuditModule } from '../audit/audit.module';
import { AuthModule } from '../auth/auth.module';
import { SettingsModule } from '../settings/settings.module';
import { UsersModule } from '../users/users.module';
import { SetupController } from './setup.controller';
import { SetupService } from './setup.service';

@Module({
  imports: [SettingsModule, UsersModule, AuthModule, AuditModule],
  controllers: [SetupController],
  providers: [SetupService],
})
export class SetupModule {}
