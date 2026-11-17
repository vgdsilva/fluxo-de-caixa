import { Module } from '@nestjs/common';
import { MeController } from './me.controller';
import { UsersService } from './users.service';

@Module({
  controllers: [MeController],
  providers: [UsersService],
  // Outros módulos só enxergam o serviço, nunca a entidade User direto.
  exports: [UsersService],
})
export class UsersModule {}
