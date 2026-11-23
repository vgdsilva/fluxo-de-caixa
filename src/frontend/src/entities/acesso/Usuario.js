class UsuarioEntity {
  constructor(defaultValues = {}) {
    this.id = defaultValues.id ?? null;
    this.name = defaultValues.name ?? "";
    this.login = defaultValues.login ?? defaultValues.username ?? "";
    this.email = defaultValues.email ?? "";
    this.role = defaultValues.role ?? "User";
  }

  getId() {
    return this.id;
  }

  getName() {
    return this.name;
  }

  getFirstName() {
    return (this.name || this.login || "").split(" ")[0];
  }

  getInitials() {
    const parts = (this.name || this.login || "?").trim().split(/\s+/);
    return ((parts[0]?.[0] ?? "") + (parts.length > 1 ? parts[parts.length - 1][0] : "")).toUpperCase();
  }
}

export const UsuarioFactory = {
  create(defaultValues) {
    return new UsuarioEntity(defaultValues);
  },
  static: UsuarioEntity,
};
