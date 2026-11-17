# 0016 Telas de perfil e admin, desativação de usuário e PWA

**Status:** aceita (17/11/2026). Implementa a fatia 6 da spec (sem as partes de 2FA, fora de escopo pelo ADR 0015).

**Decisão.**

- **Menu** no cabeçalho do app (`AppLayout`): Início e Meu perfil para todos; Usuários, Convites e Auditoria só para admin. No celular, o menu é uma faixa que rola para o lado. A guarda de rotas confere `meta.roles` (e a API confere `@Roles('admin')` de novo).
- **Meu perfil** (`/perfil`): nome (o usuário e o e-mail são só leitura); trocar senha (`PATCH /api/me/password`, confere a senha atual, aplica a política de senha e encerra as sessões dos outros aparelhos); aparelhos conectados (`GET /api/me/sessions`, encerrar um com `DELETE /api/me/sessions/:id`, todos os outros com `POST /api/me/sessions/revoke-others`). Auditoria: `password_changed`, `session_revoked`, `other_sessions_revoked`.
- **Admin > Usuários** (`/admin/usuarios`, `GET /api/admin/users`): lista com situação e último acesso; gerar link de redefinição de senha (copiar ou mandar pelo WhatsApp); desativar e reativar (`POST /api/admin/users/:id/deactivate|reactivate`).
- **Desativar ≠ remover:** coluna `users.deactivated_at`. Desativado não entra (`login_failed` com `reason: deactivated`, mensagem genérica), todas as sessões são encerradas e o `SessionGuard` recusa na hora. Reversível. O admin não pode desativar a si mesmo. Soft delete (`deleted_at`) continua reservado para "nunca existiu mais".
- **Admin > Convites** (`/admin/convites`): gerar (com e-mail opcional), o link aparece uma vez com "Copiar" e "Enviar pelo WhatsApp", lista com situação e revogar.
- **Admin > Auditoria** (`/admin/auditoria`): filtros por ação, tipo de registro, quem e período (dias no fuso de São Paulo); clicar numa linha mostra o antes/depois ou os detalhes do evento e o CorrelationId.
- **`users.last_login_at`** (só informativo, fora da auditoria automática).
- **PWA** (`vite-plugin-pwa`): manifest (nome, ícones 192/512 e maskable, `display: standalone`), service worker gerado pelo Workbox com a casca do app em cache e fallback para `index.html`; `/api` nunca é cacheado. Metatags do iPhone (`apple-touch-icon`, `apple-mobile-web-app-capable`, título "Fluxo"). Ícones gerados por `apps/web/scripts/generate-icons.mjs`. Sem conexão, aparece um aviso fixo no topo (`OfflineBanner`).
- **Front:** `hooks/useResource.ts` (`useResource` para carregar e `useAction` para executar com `busy`/erro) é o padrão para as telas chamarem os `services/`.

**Consequências.** O service worker só existe no build (`pnpm build` + `vite preview`, ou produção); no `pnpm dev` ele fica desligado. Instalar no iPhone exige HTTPS, então o teste real de instalação acontece na fatia 8 (Funnel). Guardar lançamentos offline para enviar depois continua como evolução.
