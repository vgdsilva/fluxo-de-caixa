import { Module } from '@nestjs/common';
import { AuthModule } from '../auth/auth.module';
import { UsersModule } from '../users/users.module';
import { AdminInvitesController, InvitesController } from './invites.controller';
import { InvitesService } from './invites.service';

@Module({
  imports: [UsersModule, AuthModule],
  controllers: [AdminInvitesController, InvitesController],
  providers: [InvitesService],
})
export class InvitesModule {}
