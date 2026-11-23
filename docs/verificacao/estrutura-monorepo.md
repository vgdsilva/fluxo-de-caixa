# Verificação: estrutura do monorepo (ADR 0011)

1. Na raiz existem só `src/`, `tests/`, `docs/`, `deploy/`, `archive/`, `.github/` e os arquivos `README.md`, `CLAUDE.md`, `.gitignore`, `.gitattributes`, `.editorconfig`. `apps/`, `packages/`, `package.json` e `pnpm-lock.yaml` agora estão em `archive/stack-nest/`.
2. `src/backend`, `src/frontend`, `tests` e `deploy` têm um README que diz o que mora ali.
3. Banco: `cp deploy/.env.example deploy/.env`, `docker compose -f deploy/docker-compose.yml up -d` e `docker ps` mostra `fluxo-de-caixa-postgres` como `healthy`. `git status` não lista `deploy/.env`.
4. No PR, a CI tem os jobs `backend` e `frontend` verdes; enquanto as pastas só têm README, os dois terminam dizendo "nada a fazer".
5. Abra um `.cs` qualquer no editor (depois do PR do back): indentação de 4 espaços. Um `.vue` ou `.ts`: 2 espaços.
