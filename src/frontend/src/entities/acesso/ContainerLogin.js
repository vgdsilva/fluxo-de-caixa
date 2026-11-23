class ContainerLoginEntity {
  constructor(defaultValues = {}) {
    this.login = defaultValues.login ?? "";
    this.password = defaultValues.password ?? "";
  }

  getLogin() {
    return this.login;
  }

  getPassword() {
    return this.password;
  }
}

export const ContainerLoginFactory = {
  create(defaultValues) {
    return new ContainerLoginEntity(defaultValues);
  },
  static: ContainerLoginEntity,
};
