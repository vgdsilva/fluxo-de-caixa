import { createMongoAbility, AbilityBuilder } from "@casl/ability";

/*
 * Permissões (CASL), como na referência: telas usam <Can> e $can.
 * Aqui as regras vêm do papel do usuário (Admin ou User), não de perfis de acesso.
 */
const ability = createMongoAbility();

export function defineRulesFor(user) {
  const { can, rules } = new AbilityBuilder(createMongoAbility);
  if (!user) return rules;

  can("access", "app");
  if (isAdmin(user)) {
    can("manage", "all");
  }
  return rules;
}

export function isAdmin(user) {
  const role = user?.role;
  return role === "Admin" || role === "admin" || role === 1;
}

export default ability;
