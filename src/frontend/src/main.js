import { createApp } from "vue";
import App from "./App.vue";
import router from "./router";
import store from "./store";
import { abilitiesPlugin, Can } from "@casl/vue";
import ability from "./services/ability.js";
import { setGlobalRouter } from "@/router/globalRouter.js";
import { startCrossTabSync, onAuthSessionChange } from "@/system/authSessionHelper.js";
import globalMixin from "@/viewsMixins/global/globalMixin.js";
import {
  Icon,
  Logo,
  Button,
  Input,
  InputPassword,
  Select,
  Checkbox,
  Card,
  InlineNotification,
  InlineSpinner,
  Empty,
  ModalCenter,
  Table,
} from "./index.js";
import "@/assets/styles/main.css";

store.commit("setSystemLoading", true);

function bootstrap() {
  setGlobalRouter(router);

  const app = createApp(App);

  app.use(store);
  app.use(abilitiesPlugin, ability, { useGlobalProperties: true });
  app.use(router);
  app.mixin(globalMixin);

  app.component("Can", Can);
  app.component("Icon", Icon);
  app.component("Logo", Logo);
  app.component("Button", Button);
  app.component("Input", Input);
  app.component("InputPassword", InputPassword);
  app.component("Select", Select);
  app.component("Checkbox", Checkbox);
  app.component("Card", Card);
  app.component("InlineNotification", InlineNotification);
  app.component("InlineSpinner", InlineSpinner);
  app.component("Empty", Empty);
  app.component("ModalCenter", ModalCenter);
  app.component("Table", Table);

  // Logout ou sessão expirada em outra aba: esta aba também sai.
  startCrossTabSync();
  onAuthSessionChange((session) => {
    if (!session && store.state.user) {
      store.dispatch("sessionEnded");
      if (!router.currentRoute.value.meta.public) router.replace({ name: "auth.session" });
    }
  });

  app.mount("#app");
}

bootstrap();
