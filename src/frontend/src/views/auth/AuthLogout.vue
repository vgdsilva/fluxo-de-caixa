<script>
// Tela mostrada depois de sair pelo botão "Sair".
export default {};
</script>

<template>
  <div class="auth-message">
    <Logo :size="32" />
    <h1>Você saiu da sua conta</h1>
    <p class="text-muted">Sua sessão foi encerrada neste aparelho.</p>
    <Button label="Entrar novamente" size="lg" block @click="$router.replace({ name: 'auth.login' })" />
  </div>
</template>

<style scoped>
.auth-message {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 12px;
  width: 100%;
  max-width: 380px;
  text-align: center;
}
.auth-message h1 {
  font-size: 20px;
  margin-top: 8px;
}
.auth-message p {
  margin: 0 0 8px;
}
</style>
