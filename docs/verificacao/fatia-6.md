# Verificação manual: fatia 6 (perfil, admin e PWA)

Critério da spec: admin vê o login da conta de teste na auditoria; app instala no iPhone.

## Preparação

Um admin e um usuário comum (crie um convite em Admin > Convites e aceite em outra janela anônima).

## Passos

| #   | Ação                                                                   | Esperado                                                                                                          |
| --- | ---------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------- |
| 1   | Logado como admin                                                      | Menu: Início, Meu perfil, Usuários, Convites, Auditoria                                                           |
| 2   | Logado como usuário comum                                              | Menu só com Início e Meu perfil; abrir `/admin/usuarios` volta para o início; `GET /api/admin/users` responde 403 |
| 3   | Meu perfil: mudar o nome e salvar                                      | "Salvo." e o nome novo no cabeçalho                                                                               |
| 4   | Meu perfil: trocar a senha (atual errada / nova fraca / certa)         | "A senha atual não confere." / mensagem da regra / "Senha alterada. N sessão(ões)... encerradas"                  |
| 5   | Meu perfil: aparelhos                                                  | Lista com "este aparelho"; "Encerrar" em outro e "Sair de todos os outros aparelhos" funcionam                    |
| 6   | Admin > Convites: gerar com e-mail                                     | Caixa verde com o link, "Copiar link" e "Enviar pelo WhatsApp"; linha "Aguardando"                                |
| 7   | Revogar o convite                                                      | Situação "Revogado" e o link deixa de funcionar                                                                   |
| 8   | Admin > Usuários: "Redefinir senha"                                    | Link de redefinição com validade de 1 hora                                                                        |
| 9   | Admin > Usuários: "Desativar" o usuário comum                          | Situação "Desativado"; a sessão dele cai (401) e o login falha com a mensagem genérica                            |
| 10  | "Reativar"                                                             | Volta a "Ativo" e ele consegue entrar                                                                             |
| 11  | O admin tenta se desativar                                             | O botão não aparece para a própria linha; pela API, 403                                                           |
| 12  | Admin > Auditoria: filtrar "Login falhou"                              | Aparece o login do usuário desativado, com `reason: deactivated` nos detalhes                                     |
| 13  | Auditoria sem filtro                                                   | `login` da conta de teste, `create Invite`, `update User` (desativação) etc.                                      |
| 14  | Celular (390px)                                                        | Telas em uma coluna, sem rolagem horizontal; tabelas escondem colunas menos importantes                           |
| 15  | `pnpm build` e `vite preview`                                          | `manifest.webmanifest` com nome, ícones 192/512/maskable e `display: standalone`; `sw.js` gerado                  |
| 16  | Desligar a internet com o app aberto                                   | Aviso "Sem conexão..." no topo                                                                                    |
| 17  | iPhone pelo HTTPS do Funnel: Compartilhar > Adicionar à Tela de Início | Ícone "Fluxo" e o app abre em tela cheia                                                                          |

## Resultado em 17/11/2026

Passos 1 a 3 e 6 a 15 OK num banco temporário (API na porta 3001, build servido pelo `vite preview` na 5174), no navegador embutido do app em 1280px e 390px. Dos passos 4 e 5, foi testado o caminho principal (troca de senha certa, que encerrou a sessão aberta pelo terminal, e a lista de aparelhos); senha atual errada, senha nova fraca e "Encerrar" de um aparelho específico não foram testados. O navegador embutido recusa service workers ("erro desconhecido ao buscar o script", com o `/sw.js` respondendo 200), então o registro do service worker e os passos 16 e 17 ficam para conferir no celular; o 17 depende do HTTPS da fatia 8.
