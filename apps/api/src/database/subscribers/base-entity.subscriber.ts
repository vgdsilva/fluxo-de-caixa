import { ChangeSetType, type EventSubscriber, type FlushEventArgs } from '@mikro-orm/core';
import { CurrentUserContext } from '../../common/context/current-user.context';
import { BaseEntity } from '../entities/base.entity';
import { OwnedEntity } from '../entities/owned.entity';

/**
 * Roda dentro de todo flush (o SaveChanges do MikroORM), antes de o SQL ser gerado.
 * É o equivalente a um SaveChangesInterceptor do EF Core. Garante, para todas as entidades:
 *
 * 1. Dono: um OwnedEntity novo recebe o usuário da requisição como ownerId.
 * 2. Dono imutável: mudar ownerId de um registro existente é erro.
 * 3. Soft delete: remover vira update de deletedAt; nada é apagado de verdade.
 */
export class BaseEntitySubscriber implements EventSubscriber {
  onFlush({ uow }: FlushEventArgs): void {
    for (const changeSet of uow.getChangeSets()) {
      const entity = changeSet.entity as object;

      if (changeSet.type === ChangeSetType.CREATE && entity instanceof OwnedEntity) {
        if (!entity.ownerId) {
          const userId = CurrentUserContext.userId();
          if (!userId) {
            throw new Error(
              `${changeSet.meta.className} criado sem usuário na requisição; informe ownerId só em runAsSystem.`,
            );
          }
          entity.ownerId = userId;
          uow.recomputeSingleChangeSet(entity);
        } else if (
          !CurrentUserContext.isSystem() &&
          entity.ownerId !== CurrentUserContext.userId()
        ) {
          throw new Error(
            `${changeSet.meta.className} não pode ser criado em nome de outro usuário.`,
          );
        }
      }

      if (
        changeSet.type === ChangeSetType.UPDATE &&
        entity instanceof OwnedEntity &&
        'ownerId' in changeSet.payload
      ) {
        throw new Error(`ownerId de ${changeSet.meta.className} é imutável.`);
      }

      if (changeSet.type === ChangeSetType.DELETE && entity instanceof BaseEntity) {
        changeSet.type = ChangeSetType.UPDATE;
        entity.deletedAt = new Date();
        uow.recomputeSingleChangeSet(entity);
      }
    }
  }
}
