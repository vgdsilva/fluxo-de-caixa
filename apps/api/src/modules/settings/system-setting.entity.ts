import type { Opt } from '@mikro-orm/core';
import { Entity, PrimaryKey, Property } from '@mikro-orm/decorators/legacy';

/** Chaves conhecidas de configuração do sistema. */
export const SettingKey = {
  /** '0' = ainda falta criar o primeiro admin (tela de setup); '1' = setup concluído. */
  SetupDb: 'SETUP_DB',
} as const;
export type SettingKey = (typeof SettingKey)[keyof typeof SettingKey];

/**
 * Configuração do sistema guardada no banco (chave/valor), não do usuário. Não herda de
 * BaseEntity: a chave é o próprio identificador. Mudanças relevantes são auditadas pelo
 * serviço que as faz (ex.: setup_completed).
 */
@Entity({ tableName: 'system_settings' })
export class SystemSetting {
  @PrimaryKey({ type: 'string', length: 60 })
  key!: string;

  @Property({ type: 'text' })
  value!: string;

  @Property({ type: 'datetime', onCreate: () => new Date(), onUpdate: () => new Date() })
  updatedAt: Opt<Date> = new Date();
}
