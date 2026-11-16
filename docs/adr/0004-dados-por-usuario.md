# 0004 Dados por usuário, sem tenant

**Status:** aceita (16/11/2026)

**Contexto.** O casal quer ver a casa junto, mas cada um tem contas e cartões próprios e decide o que compartilha.

**Decisão.** O dono de todo dado é sempre um usuário (`ownerId`); não existe tenant. A visão da casa é um fluxo compartilhado: o criador convida participantes (por e-mail ou username) e cada participante escolhe quais contas e cartões leva e com que nível de detalhe. Implementado na fase 1.

**Consequências.** O filtro global `owner` nasce simples (fatia 2) e na fase 1 passa a incluir "ou compartilhado comigo num fluxo". Sair de um fluxo não apaga nada.
