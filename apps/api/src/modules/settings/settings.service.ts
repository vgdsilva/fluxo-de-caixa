import { LockMode } from '@mikro-orm/core';
import { EntityManager } from '@mikro-orm/postgresql';
import { Injectable } from '@nestjs/common';
import { type SettingKey, SystemSetting } from './system-setting.entity';

/** Leitura e escrita das configurações do sistema (tabela system_settings). */
@Injectable()
export class SettingsService {
  constructor(private readonly em: EntityManager) {}

  async get(key: SettingKey): Promise<string | undefined> {
    const setting = await this.em.findOne(SystemSetting, { key });
    return setting?.value;
  }

  /**
   * Lê com trava de linha (SELECT ... FOR UPDATE): só pode ser chamado dentro de uma transação.
   * Usado para que duas requisições simultâneas não façam o setup duas vezes.
   */
  async getForUpdate(key: SettingKey): Promise<string | undefined> {
    const setting = await this.em.findOne(
      SystemSetting,
      { key },
      { lockMode: LockMode.PESSIMISTIC_WRITE },
    );
    return setting?.value;
  }

  /** Grava (cria se não existir). Entra no próximo flush do EntityManager atual. */
  async set(key: SettingKey, value: string): Promise<void> {
    const setting = await this.em.findOne(SystemSetting, { key });
    if (setting) {
      setting.value = value;
    } else {
      this.em.create(SystemSetting, { key, value });
    }
    await this.em.flush();
  }
}
