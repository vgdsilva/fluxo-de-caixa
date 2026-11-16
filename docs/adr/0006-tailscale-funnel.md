# 0006 Acesso pelo Tailscale Funnel

**Status:** aceita (16/11/2026)

**Contexto.** Os celulares precisam acessar o notebook de casa sem instalar nada e sem abrir portas no roteador.

**Decisão.** Tailscale instalado no host; `tailscale funnel --bg 3000` publica só o app com HTTPS automático. Banco, Seq e backup ficam presos ao notebook (Seq só pela tailnet com `tailscale serve`). A API configura `trust proxy` para loopback.

**Consequências.** O app fica na internet aberta, então todas as camadas de segurança do ADR 0005 são requisito do dia 1. Se o Funnel não repassar o IP do cliente, o throttler de login passa a ser por usuário e global. Fatia 8.
