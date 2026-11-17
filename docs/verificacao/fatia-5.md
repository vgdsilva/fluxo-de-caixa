# Verificação manual: fatia 5 (convites e proteções; sem 2FA)

Escopo ajustado pelo dono: só login com usuário e senha (ADR 0015). Critério: fluxo completo de convite feito à mão.

Escritas pela API exigem `-H "X-Requested-With: fluxo"`. Logue como admin com `curl -c admin.txt` e use `-b admin.txt`.

| #   | Ação                                                                                   | Esperado                                                                                                                               |
| --- | -------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------- |
| 1   | `POST /api/auth/login` sem `X-Requested-With`                                          | 403 "Requisição sem o cabeçalho anti-CSRF."                                                                                            |
| 2   | Setup/convite/reset com senha comum (`senha123456789`) ou que contém o usuário         | 400 com a mensagem da regra                                                                                                            |
| 3   | Admin: `POST /api/admin/invites` com `{"email":"x@exemplo.com"}`                       | Link `/convite?token=...` (aparece uma vez), expira em 72 h                                                                            |
| 4   | `GET /api/invites/<token>`                                                             | `{"valid":true,"email":"x@exemplo.com"}`                                                                                               |
| 5   | Aceitar com outro e-mail / com usuário já existente                                    | 400 "Este convite é para outro e-mail." / 409 "Este usuário já existe."                                                                |
| 6   | Abrir o link no navegador, preencher e "Criar minha conta"                             | Feed de sessão e o app abre logado                                                                                                     |
| 7   | Usar o mesmo link de novo                                                              | "Convite inválido, expirado ou já usado"                                                                                               |
| 8   | Usuário comum tenta `POST /api/admin/invites`                                          | 403                                                                                                                                    |
| 9   | Admin revoga um convite (`DELETE /api/admin/invites/:id`) e lista                      | Status `revoked`                                                                                                                       |
| 10  | 5 senhas erradas seguidas e depois a certa                                             | 5x 401 e depois 401 "Muitas tentativas erradas. Aguarde 15 minutos..."                                                                 |
| 11  | Mais de 10 logins em 1 minuto do mesmo IP                                              | 429 "Muitas tentativas em pouco tempo..."                                                                                              |
| 12  | Admin: `POST /api/admin/users/:id/password-reset` e abrir `/redefinir-senha?token=...` | Tela com o usuário; senha nova salva; "todas as suas sessões foram encerradas"                                                         |
| 13  | Sessão antiga do usuário depois do reset                                               | 401                                                                                                                                    |
| 14  | Login com a senha nova (mesmo bloqueado antes)                                         | 200                                                                                                                                    |
| 15  | `GET /api/audit` como admin                                                            | `create Invite`, `update Invite`, `login_failed` (`bad_password`/`locked`), `account_locked`, `create PasswordReset`, `password_reset` |

## Resultado em 17/11/2026

Passos 1 a 5 e 7 a 15 pela API num banco temporário; 6 e 12 também no navegador (Vite apontando para a API temporária): todos OK. O banco temporário foi apagado depois.
