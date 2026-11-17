import { Migration } from '@mikro-orm/migrations';

export class Migration20261117220718 extends Migration {
  override name = 'Migration20261117220718';

  override up(): void | Promise<void> {
    this.addSql(
      `alter table "users" add "deactivated_at" timestamptz null, add "last_login_at" timestamptz null;`,
    );
  }

  override down(): void | Promise<void> {
    this.addSql(`alter table "users" drop column "deactivated_at", drop column "last_login_at";`);
  }
}
