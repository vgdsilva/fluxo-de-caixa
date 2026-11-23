<script>
import { mapGetters, mapState } from "vuex";
import Logo from "@/components/base/Logo.vue";
import Icon from "@/components/base/Icon.vue";

/*
 * Layout das telas logadas (design aprovado):
 *  - Desktop: barra verde-escura no topo com logo, menu, engrenagem (admin), nome e Sair.
 *  - Celular: barra do topo compacta e menu na barra inferior.
 */
export default {
  name: "Main",
  components: { Logo, Icon },
  data() {
    return { gearOpen: false };
  },
  computed: {
    ...mapState("ItensMenuModule", ["itensMenu", "itensAdmin"]),
    ...mapGetters(["usuario"]),
  },
  watch: {
    $route() {
      this.gearOpen = false;
    },
  },
  mounted() {
    document.addEventListener("click", this.onDocumentClick);
  },
  unmounted() {
    document.removeEventListener("click", this.onDocumentClick);
  },
  methods: {
    isActive(item) {
      if (item.route === "/index/") return this.$route.path === "/index/" || this.$route.path === "/index";
      return this.$route.path.startsWith(item.route);
    },
    onDocumentClick(event) {
      if (this.gearOpen && !this.$refs.gear?.contains(event.target)) this.gearOpen = false;
    },
    logout() {
      this.$store.dispatch("logout");
    },
  },
};
</script>

<template>
  <div class="main">
    <a href="#conteudo" class="main__skip">Pular para o conteúdo</a>

    <header class="topbar">
      <div class="topbar__inner">
        <RouterLink to="/index/" class="topbar__logo" aria-label="Início">
          <Logo variant="light" />
        </RouterLink>

        <nav class="topbar__nav" aria-label="Menu principal">
          <RouterLink
            v-for="item in itensMenu"
            :key="item.key"
            :to="item.route"
            class="topbar__link"
            :class="{ 'topbar__link--active': isActive(item) }"
            :aria-current="isActive(item) ? 'page' : undefined"
          >
            {{ item.label }}
          </RouterLink>
        </nav>

        <div class="topbar__right">
          <div v-if="itensAdmin.length" ref="gear" class="topbar__gear">
            <button
              type="button"
              class="topbar__icon-btn"
              aria-label="Configurações"
              aria-haspopup="menu"
              :aria-expanded="gearOpen"
              @click="gearOpen = !gearOpen"
            >
              <Icon name="settings" />
            </button>
            <div v-if="gearOpen" class="topbar__menu" role="menu">
              <RouterLink v-for="item in itensAdmin" :key="item.key" :to="item.route" class="topbar__menu-item" role="menuitem">
                <Icon :name="item.icon" :size="16" /> {{ item.label }}
              </RouterLink>
            </div>
          </div>

          <RouterLink to="/index/perfil" class="topbar__user" :title="usuario?.name">
            <span class="topbar__avatar" aria-hidden="true">{{ usuario?.getInitials() }}</span>
            <span class="topbar__user-name">{{ usuario?.getFirstName() }}</span>
          </RouterLink>

          <button type="button" class="topbar__logout" aria-label="Sair" @click="logout">
            <Icon name="log-out" :size="16" />
            <span class="topbar__logout-text">Sair</span>
          </button>
        </div>
      </div>
    </header>

    <main id="conteudo" class="main__content" tabindex="-1">
      <RouterView />
    </main>

    <nav class="bottombar" aria-label="Menu principal">
      <RouterLink
        v-for="item in itensMenu"
        :key="item.key"
        :to="item.route"
        class="bottombar__item"
        :class="{ 'bottombar__item--active': isActive(item) }"
        :aria-current="isActive(item) ? 'page' : undefined"
      >
        <Icon :name="item.icon" :size="20" />
        <span>{{ item.label }}</span>
      </RouterLink>
    </nav>
  </div>
</template>

<style scoped>
.main {
  display: flex;
  flex-direction: column;
  min-height: 100%;
}
.main__skip {
  position: absolute;
  left: -9999px;
}
.main__skip:focus {
  left: 16px;
  top: 8px;
  z-index: 100;
  padding: 8px 12px;
  background: var(--surface);
  border-radius: var(--radius-sm);
}
.main__content {
  flex: 1;
  outline: none;
  padding-bottom: var(--bottombar-height);
}

.topbar {
  position: sticky;
  top: 0;
  z-index: 50;
  background: var(--brand-800);
  color: var(--text-on-brand);
}
.topbar__inner {
  display: flex;
  align-items: center;
  gap: 24px;
  max-width: var(--content-max);
  height: var(--topbar-height);
  margin: 0 auto;
  padding: 0 16px;
}
.topbar__logo {
  text-decoration: none;
  border-radius: var(--radius-sm);
}
.topbar__nav {
  display: none;
  gap: 4px;
}
.topbar__link {
  padding: 8px 12px;
  border-radius: var(--radius-sm);
  color: rgb(255 255 255 / 0.78);
  text-decoration: none;
  font-weight: 500;
}
.topbar__link:hover {
  color: #fff;
  background: rgb(255 255 255 / 0.08);
}
.topbar__link--active {
  color: #fff;
  background: rgb(255 255 255 / 0.14);
}
.topbar__right {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-left: auto;
}
.topbar__icon-btn,
.topbar__logout {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  height: 36px;
  padding: 0 10px;
  border: 0;
  border-radius: var(--radius-sm);
  background: transparent;
  color: rgb(255 255 255 / 0.85);
  font: inherit;
  cursor: pointer;
}
.topbar__icon-btn:hover,
.topbar__logout:hover {
  color: #fff;
  background: rgb(255 255 255 / 0.1);
}
.topbar__gear {
  position: relative;
}
.topbar__menu {
  position: absolute;
  right: 0;
  top: calc(100% + 6px);
  min-width: 200px;
  padding: 6px;
  background: var(--surface);
  color: var(--text);
  border: 1px solid var(--border);
  border-radius: var(--radius);
  box-shadow: var(--shadow);
}
.topbar__menu-item {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 10px;
  border-radius: var(--radius-sm);
  text-decoration: none;
}
.topbar__menu-item:hover {
  background: var(--surface-muted);
}
.topbar__user {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  padding: 4px 8px 4px 4px;
  border-radius: 999px;
  text-decoration: none;
  color: #fff;
}
.topbar__user:hover {
  background: rgb(255 255 255 / 0.1);
}
.topbar__avatar {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 30px;
  height: 30px;
  border-radius: 50%;
  background: var(--brand-300);
  color: var(--brand-900);
  font-size: 12px;
  font-weight: 700;
}
.topbar__user-name,
.topbar__logout-text {
  display: none;
}

.bottombar {
  position: fixed;
  left: 0;
  right: 0;
  bottom: 0;
  z-index: 50;
  display: flex;
  height: var(--bottombar-height);
  padding-bottom: env(safe-area-inset-bottom);
  background: var(--surface);
  border-top: 1px solid var(--border);
}
.bottombar__item {
  display: flex;
  flex: 1;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 4px;
  font-size: 11px;
  color: var(--text-muted);
  text-decoration: none;
}
.bottombar__item--active {
  color: var(--brand-700);
  font-weight: 600;
}

@media (min-width: 768px) {
  .topbar__nav {
    display: flex;
  }
  .topbar__user-name,
  .topbar__logout-text {
    display: inline;
  }
  .bottombar {
    display: none;
  }
  .main__content {
    padding-bottom: 0;
  }
}
</style>
