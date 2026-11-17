import { Migration } from '@mikro-orm/migrations';

export class Migration20261117194401 extends Migration {
  override name = 'Migration20261117194401';

  override up(): void | Promise<void> {
    this.addSql(
      `create table "system_settings" ("key" varchar(60) not null, "value" text not null, "updated_at" timestamptz not null, primary key ("key"));`,
    );
    // Flag do primeiro acesso. A migration NÃO cria usuário: só registra se o setup ainda falta.
    // Banco novo = 0 (o front abre a tela de setup). Banco que já tem usuários = 1.
    this.addSql(
      `insert into "system_settings" ("key", "value", "updated_at") select 'SETUP_DB', case when exists (select 1 from "users") then '1' else '0' end, now();`,
    );
  }

  override down(): void | Promise<void> {
    this.addSql(`drop table if exists "system_settings" cascade;`);
  }
}
