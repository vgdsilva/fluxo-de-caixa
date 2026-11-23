// Configuração fixa do app. Na referência este arquivo é gerado; aqui é escrito à mão.
export default {
  general: {
    applicationName: "fluxo",
    applicationTitle: "Fluxo de Caixa",
    version: "0.1.0",
  },
  auth: {
    // Renova o access token este tanto antes de ele expirar.
    refreshBeforeExpirationSeconds: 60,
  },
}
