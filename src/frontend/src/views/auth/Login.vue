<script>
import SessaoService from "@/services/acesso/SessaoService.js";
import { ContainerLoginFactory } from "@/entities/acesso/ContainerLogin.js";
import { ServiceUtils } from "@/serviceUtils.js";
import { clearAuthSession } from "@/system/authSessionHelper.js";

/*
 * Login do redesign, em passos:
 *   inicio    -> botão "Entrar com e-mail"
 *   email     -> pede o e-mail
 *   senha     -> pede a senha
 *   validando -> "Validando sua sessão…" (login + carga do sistema)
 *   erro      -> mensagem e "Tentar novamente"
 * Se der certo, vai para o Início (ou para a rota que pediu o login).
 */
const EMAIL_REGEX = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
// Tempo mínimo da tela de validação, para ela não piscar quando o backend responde rápido.
const MIN_VALIDATING_MS = 700;

export default {
  data() {
    return {
      passo: "inicio",
      containerLogin: ContainerLoginFactory.create(),
      erroCampo: "",
      erro: { titulo: "", texto: "", credenciais: false },
    };
  },
  created() {
    clearAuthSession();
  },
  methods: {
    irPara(passo) {
      this.passo = passo;
      this.erroCampo = "";
      this.$nextTick(() => {
        if (passo === "email") this.$refs.email?.focus();
        if (passo === "senha") this.$refs.senha?.focus();
        if (passo === "inicio") this.$refs.entrarEmail?.$el?.focus();
      });
    },
    continuarEmail() {
      const email = (this.containerLogin.login || "").trim();
      if (!email) {
        this.erroCampo = "Informe seu e-mail.";
        return;
      }
      if (!EMAIL_REGEX.test(email)) {
        this.erroCampo = "Esse e-mail não parece válido.";
        return;
      }
      this.containerLogin.login = email;
      this.irPara("senha");
    },
    entrar() {
      if (!this.containerLogin.password) {
        this.erroCampo = "Informe sua senha.";
        return;
      }
      this.validarSessao();
    },
    async validarSessao() {
      this.passo = "validando";
      const inicio = Date.now();
      try {
        const response = await SessaoService.sAuthorize(this.containerLogin);
        await this.$store.dispatch("loginSuccessful", response.data);
        await this.esperarMinimo(inicio);
        this.containerLogin.password = "";
        const redirect = this.$route.query.redirect;
        this.$router.replace(typeof redirect === "string" && redirect.startsWith("/index") ? redirect : "/index/");
      } catch (error) {
        await this.esperarMinimo(inicio);
        this.mostrarErro(error);
      }
    },
    esperarMinimo(inicio) {
      const resta = MIN_VALIDATING_MS - (Date.now() - inicio);
      return resta > 0 ? new Promise((r) => setTimeout(r, resta)) : Promise.resolve();
    },
    mostrarErro(error) {
      const status = error?.response?.status;
      if (status === 401 || status === 400) {
        this.erro = {
          titulo: "E-mail ou senha incorretos",
          texto: "Confira os dados e tente de novo.",
          credenciais: true,
        };
      } else if (status === 423 || status === 429) {
        this.erro = {
          titulo: "Acesso bloqueado por um tempo",
          texto: ServiceUtils.getErrorMessage(error),
          credenciais: true,
        };
      } else {
        this.erro = {
          titulo: "Não foi possível validar sua sessão",
          texto: ServiceUtils.getErrorMessage(error),
          credenciais: false,
        };
      }
      this.passo = "erro";
      this.$nextTick(() => this.$refs.tentarNovamente?.$el?.focus());
    },
    tentarNovamente() {
      // Erro de senha: volta para digitar a senha. Erro de conexão/servidor: repete a validação.
      if (this.erro.credenciais) {
        this.containerLogin.password = "";
        this.irPara("senha");
      } else {
        this.validarSessao();
      }
    },
  },
};
</script>

<template>
  <div class="login">
    <Logo class="login__logo" :size="36" />

    <!-- Início -->
    <section v-if="passo === 'inicio'" class="login__step" aria-labelledby="login-titulo">
      <div class="login__head">
        <h1 id="login-titulo" class="login__title">Seja bem-vindo(a)!</h1>
        <p class="login__subtitle">Entre para acompanhar seu fluxo de caixa.</p>
      </div>
      <Button ref="entrarEmail" size="lg" icon="mail" block label="Entrar com e-mail" @click="irPara('email')" />
    </section>

    <!-- E-mail -->
    <form v-else-if="passo === 'email'" class="login__step" novalidate aria-labelledby="login-titulo" @submit.prevent="continuarEmail">
      <button type="button" class="login__back" @click="irPara('inicio')">
        <Icon name="arrow-left" :size="16" /> Voltar
      </button>
      <div class="login__head">
        <h1 id="login-titulo" class="login__title">Qual é o seu e-mail?</h1>
        <p class="login__subtitle">Use o e-mail da sua conta.</p>
      </div>
      <Input
        ref="email"
        v-model="containerLogin.login"
        label="E-mail"
        type="email"
        placeholder="voce@exemplo.com"
        autocomplete="username"
        required
        :error="erroCampo"
      />
      <Button type="submit" size="lg" block label="Continuar" />
    </form>

    <!-- Senha -->
    <form v-else-if="passo === 'senha'" class="login__step" novalidate aria-labelledby="login-titulo" @submit.prevent="entrar">
      <button type="button" class="login__back" @click="irPara('email')">
        <Icon name="arrow-left" :size="16" /> Voltar
      </button>
      <div class="login__head">
        <h1 id="login-titulo" class="login__title">Digite sua senha</h1>
        <p class="login__account">
          <Icon name="user" :size="14" />
          <span>{{ containerLogin.login }}</span>
          <button type="button" class="login__link" @click="irPara('email')">Trocar</button>
        </p>
      </div>
      <!-- Campo oculto para o gerenciador de senhas associar a senha ao e-mail. -->
      <input class="sr-only" type="email" autocomplete="username" :value="containerLogin.login" tabindex="-1" aria-hidden="true" readonly />
      <InputPassword
        ref="senha"
        v-model="containerLogin.password"
        label="Senha"
        placeholder="Sua senha"
        autocomplete="current-password"
        required
        :error="erroCampo"
      />
      <p class="login__hint">Esqueceu a senha? Peça ao administrador um link para criar uma nova.</p>
      <Button type="submit" size="lg" block label="Entrar" />
    </form>

    <!-- Validando -->
    <section v-else-if="passo === 'validando'" class="login__step login__status" role="status" aria-live="polite">
      <span class="login__status-icon"><InlineSpinner :size="28" label="Validando" /></span>
      <h1 class="login__title">Validando sua sessão…</h1>
      <p class="login__subtitle">Isso leva só um instante.</p>
    </section>

    <!-- Erro -->
    <section v-else-if="passo === 'erro'" class="login__step login__status" role="alert">
      <span class="login__status-icon login__status-icon--error"><Icon name="alert-circle" :size="28" /></span>
      <h1 class="login__title">{{ erro.titulo }}</h1>
      <p class="login__subtitle">{{ erro.texto }}</p>
      <Button ref="tentarNovamente" size="lg" block label="Tentar novamente" @click="tentarNovamente" />
      <button type="button" class="login__link" @click="irPara('email')">Usar outro e-mail</button>
    </section>
  </div>
</template>

<style scoped>
.login {
  display: flex;
  flex-direction: column;
  gap: 28px;
  width: 100%;
  max-width: 380px;
}
.login__logo {
  align-self: flex-start;
}
.login__step {
  display: flex;
  flex-direction: column;
  gap: 18px;
}
.login__head {
  display: flex;
  flex-direction: column;
  gap: 6px;
}
.login__title {
  font-size: 24px;
  font-weight: 700;
}
.login__subtitle {
  margin: 0;
  color: var(--text-muted);
}
.login__back {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  align-self: flex-start;
  padding: 4px 6px;
  margin-left: -6px;
  border: 0;
  border-radius: var(--radius-sm);
  background: transparent;
  color: var(--text-muted);
  font: inherit;
  font-weight: 600;
  cursor: pointer;
}
.login__back:hover {
  color: var(--text);
}
.login__account {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  margin: 0;
  color: var(--text-muted);
}
.login__link {
  padding: 0;
  border: 0;
  background: transparent;
  color: var(--brand-700);
  font: inherit;
  font-weight: 700;
  cursor: pointer;
  text-decoration: underline;
  text-underline-offset: 2px;
}
.login__hint {
  margin: -6px 0 0;
  font-size: 12px;
  color: var(--text-muted);
}
.login__status {
  align-items: center;
  text-align: center;
}
.login__status-icon {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 64px;
  height: 64px;
  border-radius: 50%;
  background: var(--brand-100);
  color: var(--brand-600);
}
.login__status-icon--error {
  background: var(--danger-bg);
  color: var(--danger);
}
</style>
