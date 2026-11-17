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

## Primeiro admin (banco vazio)

| #   | Ação                               | Esperado                                                                                          |
| --- | ---------------------------------- | ------------------------------------------------------------------------------------------------- |
| 15  | Subir a API num banco sem usuários | Log: `Nenhum usuário cadastrado. Crie o primeiro admin em: http://localhost:5173/setup?token=...` |
| 16  | Abrir o link                       | Formulário "Primeiro acesso"; com token errado, "Link inválido ou já usado"                       |
| 17  | Senha com menos de 12 caracteres   | Erro de validação                                                                                 |
| 18  | Criar o admin                      | Entra direto pelo feed de sessão                                                                  |
| 19  | Abrir o link de novo               | 404 na API; tela "Link inválido ou já usado"                                                      |

Para testar sem apagar o banco de dev, suba uma segunda API com outro banco: `DB_NAME=<banco vazio> PORT=3001 node dist/main.js` (variáveis de ambiente valem mais que o `.env`).

## Resultado em 17/11/2026

Passos 1 e 3 a 8 no navegador embutido (desktop 1280x800 e celular 375px) e 9 a 19 pela API com curl: todos OK. O passo 2 (campos vazios) não foi testado no navegador. O primeiro admin foi testado num banco temporário (`cmdb_fluxo_setup_tmp`), apagado depois. As telas do passo 16 a 18 foram verificadas só pela API.
