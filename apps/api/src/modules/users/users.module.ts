import { Module } from '@nestjs/common';
import { UsersService } from './users.service';

@Module({
  providers: [UsersService],
  // Outros módulos só enxergam o serviço, nunca a entidade User direto.
  exports: [UsersService],
})
export class UsersModule {}
