import type { SetupInput } from '@fluxo/contracts';
import { EntityManager } from '@mikro-orm/postgresql';
import { ConflictException, Injectable, NotFoundException } from '@nestjs/common';
import { AppContext } from '../../common/context/app-context';
import { AuditWriter } from '../audit/audit-writer.service';
import { PasswordService } from '../auth/password.service';
import { SettingsService } from '../settings/settings.service';
import { SettingKey } from '../settings/system-setting.entity';
import { UsersService } from '../users/users.service';

/**
 * Primeiro acesso. A configuração SETUP_DB (tabela system_settings) diz se o sistema já tem o
 * admin: '0' ou ausente = setup pendente (o front abre a tela de setup), '1' = concluído
 * (as rotas de setup passam a responder 404).
 *
 * O admin não é criado por migration nem por seed em produção: só por esta tela.
 */
@Injectable()
export class SetupService {
  constructor(
    private readonly em: EntityManager,
    private readonly settings: SettingsService,
    private readonly users: UsersService,
    private readonly passwords: PasswordService,
    private readonly audit: AuditWriter,
  ) {}

  async isRequired(): Promise<boolean> {
    return (await this.settings.get(SettingKey.SetupDb)) !== '1';
  }

  /**
   * Cria o admin e marca SETUP_DB = 1 na MESMA transação: ou as duas coisas acontecem, ou
   * nenhuma. A linha da configuração fica travada durante a transação, então dois cliques
   * simultâneos não criam dois admins.
   */
  async run(input: SetupInput): Promise<void> {
    const passwordHash = await this.passwords.hash(input.password);
    AppContext.setChannel('auth');

    const userId = await this.em.transactional(async () => {
      // Dentro do transactional, os serviços usam o EntityManager da transação automaticamente.
      if ((await this.settings.getForUpdate(SettingKey.SetupDb)) === '1') {
        throw new NotFoundException();
      }
      if (await this.users.findCredentials(input.username)) {
        throw new ConflictException('Este usuário já existe.');
      }
      const id = await this.users.createUser({
        username: input.username,
        name: input.name,
        role: 'admin',
        passwordHash,
      });
      await this.settings.set(SettingKey.SetupDb, '1');
      return id;
    });

    await this.audit.record({
      action: 'setup_completed',
      userId,
      entityType: 'User',
      entityId: userId,
    });
  }
}
