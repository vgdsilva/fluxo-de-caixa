<script>
import { mapGetters } from "vuex";
import { getAuthSession, onAuthSessionChange } from "@/system/authSessionHelper.js";

// Meu perfil. Edição de nome e troca de senha entram quando o backend tiver os endpoints.
export default {
  data() {
    return {
      session: getAuthSession(),
      off: null,
    };
  },
  computed: {
    ...mapGetters(["usuario"]),
  },
  mounted() {
    this.off = onAuthSessionChange((session) => (this.session = session));
  },
  unmounted() {
    this.off?.();
  },
};
</script>

<template>
  <div class="page perfil">
    <div class="page__header">
      <h1 class="page__title">Meu perfil</h1>
    </div>

    <div class="grid grid--2">
      <Card title="Seus dados" subtitle="O usuário e o e-mail não mudam por aqui.">
        <div class="perfil__form">
          <Input :model-value="usuario?.name" label="Nome" readonly />
          <Input :model-value="usuario?.login" label="Usuário" readonly />
          <Input :model-value="usuario?.email" label="E-mail" readonly />
        </div>
      </Card>

      <Card title="Sessão" subtitle="O acesso renova sozinho enquanto você usa o app.">
        <dl class="perfil__session">
          <dt>Acesso atual vale até</dt>
          <dd>{{ formatDateTime(session?.accessTokenExpiresAt) || "—" }}</dd>
          <dt>Sessão termina em</dt>
          <dd>{{ formatDateTime(session?.refreshTokenExpiresAt) || "—" }}</dd>
        </dl>
      </Card>
    </div>
  </div>
</template>

<style scoped>
.perfil__form {
  display: flex;
  flex-direction: column;
  gap: 14px;
}
.perfil__session {
  display: grid;
  grid-template-columns: auto 1fr;
  gap: 8px 16px;
  margin: 0;
}
.perfil__session dt {
  color: var(--text-muted);
}
.perfil__session dd {
  margin: 0;
  font-weight: 600;
}
</style>
