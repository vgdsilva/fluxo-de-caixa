import { Migration } from '@mikro-orm/migrations';

export class Migration20261117201021 extends Migration {
  override name = 'Migration20261117201021';

  override up(): void | Promise<void> {
    this.addSql(
      `create table "invites" ("id" uuid not null, "created_at" timestamptz not null, "updated_at" timestamptz not null, "deleted_at" timestamptz null, "version" int not null default 1, "token_hash" varchar(64) not null, "email" varchar(254) null, "expires_at" timestamptz not null, "created_by_user_id" uuid not null, "used_at" timestamptz null, "used_by_user_id" uuid null, "revoked_at" timestamptz null, primary key ("id"));`,
    );
    this.addSql(`create index "invites_deleted_at_index" on "invites" ("deleted_at");`);
    this.addSql(
      `alter table "invites" add constraint "invites_token_hash_unique" unique ("token_hash");`,
    );

    this.addSql(
      `create table "password_resets" ("id" uuid not null, "created_at" timestamptz not null, "updated_at" timestamptz not null, "deleted_at" timestamptz null, "version" int not null default 1, "token_hash" varchar(64) not null, "user_id" uuid not null, "created_by_user_id" uuid not null, "expires_at" timestamptz not null, "used_at" timestamptz null, primary key ("id"));`,
    );
    this.addSql(
      `create index "password_resets_deleted_at_index" on "password_resets" ("deleted_at");`,
    );
    this.addSql(
      `alter table "password_resets" add constraint "password_resets_token_hash_unique" unique ("token_hash");`,
    );
    this.addSql(`create index "password_resets_user_id_index" on "password_resets" ("user_id");`);

    this.addSql(
      `alter table "users" add "email" varchar(254) null, add "failed_login_count" int not null default 0, add "locked_until" timestamptz null;`,
    );
  }

  override down(): void | Promise<void> {
    this.addSql(`drop table if exists "invites" cascade;`);
    this.addSql(`drop table if exists "password_resets" cascade;`);

    this.addSql(
      `alter table "users" drop column "email", drop column "failed_login_count", drop column "locked_until";`,
    );
  }
}
