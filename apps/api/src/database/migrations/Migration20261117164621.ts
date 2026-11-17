import { Migration } from '@mikro-orm/migrations';

export class Migration20261117164621 extends Migration {
  override name = 'Migration20261117164621';

  override up(): void | Promise<void> {
    this.addSql(
      `create table "notes" ("id" uuid not null, "created_at" timestamptz not null, "updated_at" timestamptz not null, "deleted_at" timestamptz null, "version" int not null default 1, "owner_id" uuid not null, "title" varchar(120) not null, "body" text not null default '', primary key ("id"));`,
    );
    this.addSql(`create index "notes_deleted_at_index" on "notes" ("deleted_at");`);
    this.addSql(`create index "notes_owner_id_index" on "notes" ("owner_id");`);

    this.addSql(
      `create table "users" ("id" uuid not null, "created_at" timestamptz not null, "updated_at" timestamptz not null, "deleted_at" timestamptz null, "version" int not null default 1, "username" varchar(40) not null, "name" varchar(120) not null, "role" text not null default 'user', primary key ("id"));`,
    );
    this.addSql(`create index "users_deleted_at_index" on "users" ("deleted_at");`);
    this.addSql(`alter table "users" add constraint "users_username_unique" unique ("username");`);
    this.addSql(
      `alter table "users" add constraint "users_role_check" check ("role" in ('admin', 'user'));`,
    );
  }
}
