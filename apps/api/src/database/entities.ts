import { AuditEntry } from '../modules/audit/audit-entry.entity';
import { User } from '../modules/users/user.entity';
import { BaseEntity } from './entities/base.entity';
import { OwnedEntity } from './entities/owned.entity';

/**
 * Registro de todas as entidades, como os DbSet de um DbContext.
 * Entidade nova: crie em modules/<feature>/<nome>.entity.ts e adicione aqui.
 * As bases abstratas precisam estar na lista para o MikroORM herdar as colunas delas.
 */
export const entities = [BaseEntity, OwnedEntity, User, AuditEntry];
