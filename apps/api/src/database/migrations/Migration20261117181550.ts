import { Migration } from '@mikro-orm/migrations';

export class Migration20261117181550 extends Migration {
  override name = 'Migration20261117181550';

  override up(): void | Promise<void> {
    this.addSql(`create schema if not exists "audit";`);
    this.addSql(
      `create table "audit"."entries" ("id" uuid not null, "at" timestamptz not null, "user_id" uuid null, "record_owner_id" uuid null, "channel" varchar(16) not null, "action" varchar(40) not null, "entity_type" varchar(80) null, "entity_id" varchar(80) null, "changes" jsonb null, "metadata" jsonb null, "correlation_id" uuid not null, primary key ("id"));`,
    );
    this.addSql(`create index "entries_at_index" on "audit"."entries" ("at");`);
    this.addSql(
      `create index "entries_correlation_id_index" on "audit"."entries" ("correlation_id");`,
    );
    this.addSql(`create index "entries_action_at_index" on "audit"."entries" ("action", "at");`);
    this.addSql(`create index "entries_user_id_at_index" on "audit"."entries" ("user_id", "at");`);
    this.addSql(
      `create index "entries_record_owner_id_at_index" on "audit"."entries" ("record_owner_id", "at");`,
    );
    this.addSql(
      `create index "entries_entity_type_entity_id_at_index" on "audit"."entries" ("entity_type", "entity_id", "at");`,
    );
  }

  override down(): void | Promise<void> {
    this.addSql(`drop table if exists "audit"."entries" cascade;`);

    this.addSql(`drop schema if exists "audit";`);
  }
}
