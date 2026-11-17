# Verificação manual: fatia 4 (autenticação base)

Critério da spec: admin criado no primeiro start e logado pelo front; o link morre depois.

## Preparação

1. `apps/api/.env` com `SEED_PASSWORD` (veja `.env.example`).
2. `pnpm dev`. Na primeira vez, o log mostra `Migrations aplicadas: Migration20261117183407`.
3. `pnpm --filter @fluxo/api db:seed` (define a senha de `ana` e `bruno`).

## Telas (navegador em http://localhost:5173)

| #   | Ação                            | Esperado                                                                                                                                |
| --- | ------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------- |
| 1   | Abrir `/` sem estar logado      | Vai para `/login`: tela dividida, marca à esquerda, formulário à direita                                                                |
| 2   | Enviar com campos vazios        | Mensagens "Informe o usuário" / "Informe a senha" sob os campos, sem sair da tela                                                       |
| 3   | `ana` + senha errada, "Acessar" | Formulário some, feed de sessão aparece e em seguida vai para `/login/erro`: feed vermelho com "Usuário ou senha inválidos."            |
| 4   | "Tentar novamente"              | Volta para `/login` com `ana` já preenchido                                                                                             |
| 5   | `ana` + senha certa             | Feed com os três passos marcados, quadro verde e "Tudo certo! Bem-vindo(a), Ana."; depois abre `/` com "Olá, Ana" e o nome no cabeçalho |
| 6   | Recarregar a página             | Continua logado (sessão no banco, cookie HttpOnly invisível ao JavaScript)                                                              |
| 7   | "Sair"                          | Volta para `/login`; `GET /api/me` responde 401                                                                                         |
| 8   | Celular (375px)                 | Só o formulário, sem rolagem horizontal                                                                                                 |

## API (terminal)

| #   | Ação                                              | Esperado                                                                        |
| --- | ------------------------------------------------- | ------------------------------------------------------------------------------- |
| 9   | `GET /api/me` sem cookie                          | 401                                                                             |
| 10  | Login com usuário inexistente e com senha errada  | 401 com a mesma mensagem nos dois casos                                         |
| 11  | Login certo                                       | 200 + `Set-Cookie: fluxo_sid=...; HttpOnly; SameSite=Lax; Max-Age=604800`       |
| 12  | `GET /api/audit` como `ana` / como `bruno`        | 200 com `login`, `login_failed` (`reason: unknown_user` / `bad_password`) / 403 |
| 13  | Auditoria do seed                                 | `update` de `User` com `passwordHash: "***"`                                    |
| 14  | Logout e repetir `GET /api/me` com o mesmo cookie | 204, depois 401                                                                 |

## Primeiro admin pela flag SETUP_DB (ADR 0014)

| #   | Ação                                                                 | Esperado                                                            |
| --- | -------------------------------------------------------------------- | ------------------------------------------------------------------- |
| 15  | Banco novo: subir a API                                              | Migration cria `system_settings` com `SETUP_DB = 0`; nenhum usuário |
| 16  | Abrir qualquer tela do app                                           | Vai para `/setup` ("Primeiro acesso")                               |
| 17  | Senha com menos de 12 caracteres ou confirmação diferente            | Erro de validação na tela                                           |
| 18  | Criar o admin                                                        | Feed de sessão e depois "Olá, <nome>"; `SETUP_DB` vira 1            |
| 19  | `GET /api/setup/status` / `POST /api/setup` de novo / abrir `/setup` | `required: false` / 404 / vai para o login                          |
| 20  | Banco que já tinha usuários, ao aplicar a migration                  | `SETUP_DB = 1` direto                                               |

Para testar sem mexer no banco de dev: crie um banco vazio, suba uma segunda API (`DB_NAME=<banco vazio> PORT=3001 node dist/main.js` em `apps/api`) e um Vite apontando para ela (`API_PROXY_TARGET=http://localhost:3001 npx vite --port 5174` em `apps/web`).

## Resultado em 17/11/2026

Passos 1 e 3 a 8 no navegador embutido (desktop 1280x800 e celular 375px), 9 a 14 pela API com curl, 15 a 19 com um banco temporário (navegador na porta 5174) e 20 no banco de dev: todos OK. O passo 2 (campos vazios) não foi testado no navegador.
