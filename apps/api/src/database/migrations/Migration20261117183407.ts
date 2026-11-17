import { Migration } from '@mikro-orm/migrations';

export class Migration20261117183407 extends Migration {
  override name = 'Migration20261117183407';

  override up(): void | Promise<void> {
    this.addSql(
      `create table "sessions" ("id" uuid not null, "token_hash" varchar(64) not null, "user_id" uuid not null, "created_at" timestamptz not null, "last_seen_at" timestamptz not null, "expires_at" timestamptz not null, "revoked_at" timestamptz null, "ip" varchar(64) null, "user_agent" varchar(300) null, primary key ("id"));`,
    );
    this.addSql(
      `alter table "sessions" add constraint "sessions_token_hash_unique" unique ("token_hash");`,
    );
    this.addSql(`create index "sessions_expires_at_index" on "sessions" ("expires_at");`);
    this.addSql(
      `create index "sessions_user_id_revoked_at_index" on "sessions" ("user_id", "revoked_at");`,
    );

    this.addSql(`alter table "users" add "password_hash" text null;`);
  }

  override down(): void | Promise<void> {
    this.addSql(`drop table if exists "sessions" cascade;`);

    this.addSql(`alter table "users" drop column "password_hash";`);
  }
}
