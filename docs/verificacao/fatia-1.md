# Verificação manual: fatia 1 (esqueleto)

Pré-requisitos: Node >= 22.12 (a CI usa 24), pnpm 11, git. Postgres ainda não é usado nesta fatia.

1. **Instalação limpa.** Na raiz: `pnpm install --frozen-lockfile`. Termina sem erro (avisos de dependências depreciadas são esperados).
2. **Configuração.** Copie `apps/api/.env.example` para `apps/api/.env`.
3. **Checagens da CI.** `pnpm format:check && pnpm lint && pnpm typecheck && pnpm build`. Tudo passa.
4. **Subir em dev.** `pnpm dev`. No log aparecem `API ouvindo em http://localhost:3000/api` e o Vite em `http://localhost:5173`.
5. **API direta.** Abra `http://localhost:3000/api/health`. Resposta: `{"status":"ok","time":"...","version":"0.1.0"}`.
6. **Front pelo proxy.** Abra `http://localhost:5173`. A tela mostra "API no ar (versão 0.1.0, ...)" em verde. Em `http://localhost:5173/api/health` o mesmo JSON do passo 5 (é o proxy do Vite).
7. **Erro visível.** Pare só a API (ou feche o terminal e rode `pnpm --filter @fluxo/web dev`). Clique em "Verificar de novo": aparece a mensagem vermelha "Não foi possível falar com a API" (o proxy do Vite responde 502).
8. **Config validada.** Com `PORT=abc` no `apps/api/.env`, `pnpm --filter @fluxo/api dev` não sobe e mostra "Configuração inválida" apontando `PORT`. Volte para `3000`.
9. **Fronteira web/api.** Em qualquer arquivo de `apps/web/src`, adicione `import '../../api/src/main';` e rode `pnpm lint`: falha com a regra `no-restricted-imports` ("O web só fala com a API por HTTP"). Desfaça.
10. **CI.** No GitHub, o workflow CI fica verde no push do branch.

Resultado em 16/11/2026 (Windows 11, Node 22.18, pnpm 11.28): passos 1 a 9 OK; passo 10 depende do push.
