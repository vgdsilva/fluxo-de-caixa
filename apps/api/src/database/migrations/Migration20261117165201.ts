import { Migration } from '@mikro-orm/migrations';

export class Migration20261117165201 extends Migration {
  override name = 'Migration20261117165201';

  override up(): void | Promise<void> {
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
