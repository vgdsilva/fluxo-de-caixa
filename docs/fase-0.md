<!-- Cópia versionada do documento https://claude.ai/artifact/JXcNd6M533qxmwUAA6dt8T (16/11/2026). Ajustes de stack e decisões posteriores ficam nos ADRs em docs/adr/. -->

# Fluxo de Caixa: spec da fase 0 (fundação)

2026-11-16 · Vinicius Silva

## Objetivo e critério de pronto

A fase 0 entrega um app vazio, mas seguro e implantado: **você e a Re entram pelo celular, via Funnel, com 2FA, e toda ação fica auditada.** Nenhuma funcionalidade financeira ainda; o objetivo é que a fase 1 só precise adicionar domínio, sem mexer em base.

A fase está pronta quando:

- [ ] O app roda local contra o Postgres da sua máquina, com migrations aplicadas no start.
- [ ] O admin gera um convite, a conta convidada é criada, configura 2FA e entra.
- [ ] Sem convite não há como criar conta; sem segundo fator não há como entrar.
- [ ] Login, convite e alterações de perfil aparecem na tela de auditoria do admin.
- [ ] Verificação manual: logado como usuário A, não é possível ver nem alterar um registro do usuário B (inclusive trocando o id na URL).
- [ ] Por último, a implantação: `docker compose up -d` no notebook, app na URL do Funnel com HTTPS, você e a Re entram pelo celular, e um restart e um restore de backup foram testados.

## Escopo

| Entra na fase 0                                                                       | Fica para depois                                                                     |
| ------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------ |
| Solução, projetos, convenções                                                         | Contas, cartões, lançamentos (fase 1)                                                |
| Autenticação: convite, cadastro, login, 2FA TOTP, passkey, troca de senha             | Fluxo compartilhado e convites entre usuários (fase 1)                               |
| Perfis Admin e Usuário                                                                | Papéis dentro de um fluxo (fase 1)                                                   |
| Entidade base com dono, soft delete e auditoria automática                            | Projeção, importação, chat (fases 2 a 4)                                             |
| Telas: login, cadastro por convite, meu perfil, admin (usuários, convites, auditoria) | E-mail transacional: convite e reset de senha são por link que o admin copia e envia |
| Docker Compose, Funnel, pino + Seq, backup do banco                                   | Deploy automático; na fase 0 é um script                                             |
| CI só com build no GitHub Actions                                                     | Testes automatizados (unitários, integração, arquitetura) e alertas                  |

**Decisão para economizar:** sem servidor de e-mail. O convite e o reset de senha geram um link de uso único que o admin manda pelo WhatsApp. Isso tira SMTP, DNS e reputação de e-mail do caminho.

**PWA no iPhone (entra na fase 0):** `vite-plugin-pwa` gera manifest e service worker; ícones e tela cheia para instalar na tela de início. Como o front é uma SPA, a casca do app abre mesmo sem sinal e volta do segundo plano sem reconectar nada; só os dados dependem da API. Na fase 0, sem conexão o app mostra um aviso; guardar lançamentos offline para enviar depois fica como evolução.

## Estrutura da solução

**Monorepo TypeScript com pnpm workspaces: front em Vue 3, back em NestJS e um pacote de contratos compartilhado.** Em produção é um container só: o NestJS serve a API em `/api` e os arquivos estáticos do Vue no resto, na mesma origem. Assim não há CORS e o cookie de sessão funciona sem truques. No back, cada funcionalidade é um módulo Nest; no front, cada funcionalidade é uma pasta em `features/`.

```text
FluxoCaixa/
├─ CLAUDE.md
├─ package.json, pnpm-workspace.yaml, tsconfig.base.json
├─ docs/                       plano base, esta spec, adr/
├─ deploy/                     Dockerfile, docker-compose.yml, .env.example, scripts (fatia 8)
├─ apps/
│  ├─ api/                     NestJS
│  │  └─ src/
│  │     ├─ main.ts, app.module.ts
│  │     ├─ shared/            entidade base, CurrentUser, Result, relógio, guards
│  │     ├─ persistence/       config do MikroORM, filtros, subscribers, migrations/
│  │     └─ modules/
│  │        ├─ identity/       identity.module.ts, domain/, application/, http/
│  │        └─ auditing/       mesmo molde
│  └─ web/                     Vue 3 + Vite
│     └─ src/
│        ├─ app/               router, layout, Pinia, guarda de rotas
│        ├─ shared/            cliente HTTP, componentes base
│        └─ features/
│           ├─ auth/           login, cadastro por convite, setup de 2FA
│           ├─ profile/
│           └─ admin/          usuários, convites, auditoria
└─ packages/
   └─ contracts/               schemas zod e tipos usados pela api e pelo web
```

**Regras de dependência** (quando os testes entrarem, verificadas com dependency-cruiser):

- Um módulo Nest só usa de outro o que este exporta em `exports` (serviços de aplicação), nunca entidades ou repositórios internos.
- Controllers chamam serviços de aplicação (casos de uso); nunca usam o `EntityManager` direto. O chat da fase 4 vai chamar os mesmos serviços.
- `packages/contracts` é a única fonte dos formatos de request e response: a API valida com esses schemas zod e o front importa os mesmos tipos. Ninguém escreve tipo de API à mão no front.
- `web` só fala com `api` por HTTP; `shared/` (dos dois lados) não conhece nenhuma feature.

**Convenções:**

- Node 24 LTS, NestJS 11, Vue 3 com `<script setup>`, Vite, Pinia, Vue Router, TypeScript `strict` nos três pacotes. Biblioteca de componentes: PrimeVue.
- Validação com zod nas bordas (nestjs-zod na API, os mesmos schemas nos formulários do front).
- **Dinheiro nunca em `number` com centavos quebrados:** inteiro em centavos no código, `numeric(14,2)` no banco, conversão num só lugar. Datas de negócio como `'YYYY-MM-DD'`, instantes como `timestamptz` em UTC, fuso de exibição America/Sao_Paulo.
- Nomes de código em inglês, textos da interface em português (vue-i18n opcional; strings num arquivo por feature já basta).
- ESLint + Prettier na raiz; versões fixadas no lockfile do pnpm.

## Identidade e acesso

Autenticação própria no NestJS, com bibliotecas consolidadas para cada peça: senha com `argon2` (argon2id), TOTP com `otplib`, passkeys com `@simplewebauthn/server` e `@simplewebauthn/browser`, rate limit com `@nestjs/throttler`. Sessão no servidor (tabela no Postgres) com cookie, não JWT: dá para encerrar sessões na hora, que é o que o admin e o "sair de outros aparelhos" precisam. Regras centrais: **cadastro só por convite e segundo fator obrigatório**.

| História                | Critérios de aceite                                                                                                                                                                                                               |
| ----------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Primeiro admin          | No primeiro start com banco vazio, a API cria um link de configuração de uso único e o escreve no log. Depois que o admin existe, o link deixa de funcionar e a rota responde 404.                                                |
| Admin gera convite      | Token aleatório de 32 bytes, guardado só como hash, uso único, expira em 72 h, opcionalmente preso a um e-mail. A tela mostra o link uma vez, com botão de copiar. Convite pode ser revogado.                                     |
| Convidado cria a conta  | Abre o link, informa nome, username (único), e-mail e senha (mínimo 12 caracteres, checagem contra senhas comuns). Antes de acessar qualquer tela, configura passkey ou TOTP e recebe os códigos de recuperação.                  |
| Login                   | Senha + segundo fator, ou passkey direto. 5 tentativas erradas bloqueiam por 15 minutos; throttler no endpoint. Mensagem de erro genérica (não revela se o usuário existe).                                                       |
| Sessão                  | Cookie `Secure`, `HttpOnly`, `SameSite=Lax`, expiração deslizante de 7 dias, id de sessão trocado no login. Requisições que alteram dados exigem um cabeçalho anti-CSRF que o front envia. Sessões no banco sobrevivem a restart. |
| Esqueci a senha         | Sem e-mail: o admin gera um link de reset de uso único (expira em 1 h) e manda pelo WhatsApp. O segundo fator continua exigido.                                                                                                   |
| Perdi o celular         | Entra com código de recuperação, ou o admin reseta o 2FA, o que fica auditado.                                                                                                                                                    |
| Meu perfil              | Trocar nome, senha, gerenciar passkeys, regenerar códigos, ver sessões ativas e encerrar as outras.                                                                                                                               |
| Admin gerencia usuários | Lista, desativa e reativa usuários (desativar encerra sessões). Admin não vê dados financeiros de ninguém.                                                                                                                        |

**Perfis:** `admin` e `user`. Na API, guards globais (`SessionGuard`, `TwoFactorGuard`) e um decorator `@Roles('admin')` com `RolesGuard`; rotas públicas marcadas com `@Public()`. No front, a guarda do Vue Router só esconde telas; quem protege de verdade é a API. O admin também é um usuário comum com os próprios dados.

## Base de dados e auditoria

Tudo que a fase 1 vai criar herda três garantias da fase 0: **tem dono, nunca some e deixa rastro.** Elas ficam na camada de persistência, não em cada serviço, para a IA não ter como esquecer. ORM: **MikroORM** com PostgreSQL. Ele usa unit of work e change tracking como o EF Core, tem filtros globais e subscribers de flush nativos, que são exatamente as peças dessas três garantias.

**Entidade base (`OwnedEntity`, classe abstrata do MikroORM):**

| Campo                    | Tipo                            | Regra                                                                    |
| ------------------------ | ------------------------------- | ------------------------------------------------------------------------ |
| `id`                     | `uuid` (v7, ordenado por tempo) | Gerado na aplicação                                                      |
| `ownerId`                | `uuid`                          | Preenchido por um subscriber a partir do usuário da requisição; imutável |
| `createdAt`, `updatedAt` | `timestamptz`                   | Preenchidos automaticamente                                              |
| `deletedAt`              | `timestamptz` nulo              | Soft delete: um subscriber converte remoção em update                    |
| `version`                | inteiro                         | Concorrência otimista (`version: true` do MikroORM)                      |

**Filtros globais:** `owner` (`ownerId = usuário atual`) e `notDeleted` (`deletedAt is null`), ambos ligados por padrão. Um middleware abre um `RequestContext` por requisição e preenche o parâmetro do usuário. A fase 1 estende o filtro `owner` para "ou compartilhado comigo num fluxo". Jobs de sistema usam um `SystemContext` que faz `em.fork()` com filtros desligados; desligar filtro em qualquer outro lugar é proibido (regra no `CLAUDE.md` e, depois, lint).

**Auditoria (`audit.entries`):** um subscriber `onFlush` lê os change sets do MikroORM e grava, no mesmo flush e na mesma transação, uma linha por entidade alterada.

| Campo                    | Conteúdo                                                                                                      |
| ------------------------ | ------------------------------------------------------------------------------------------------------------- |
| `At`                     | Instante UTC                                                                                                  |
| `UserId`                 | Quem fez (ou `system`)                                                                                        |
| `Channel`                | `ui`, `chat`, `import`, `job`, `auth`                                                                         |
| `Action`                 | `create`, `update`, `delete`, ou evento de segurança (`login`, `login_failed`, `2fa_reset`, `invite_created`) |
| `EntityType`, `EntityId` | O que foi alterado                                                                                            |
| `Changes`                | JSON com antes e depois só dos campos alterados; campos marcados `como sensíveis` aparecem mascarados         |
| `CorrelationId`          | Liga a linha à requisição e aos logs                                                                          |

Eventos de login não passam por entidades, então são gravados por um `AuditWriter` chamado pelos serviços de autenticação. O `CorrelationId` vem de um interceptor do Nest e segue nos logs. A tabela de auditoria é só de inserção: o usuário do banco da aplicação não tem `UPDATE` nem `DELETE` nela.

## Infra no notebook

Só o app aparece no Funnel; banco, logs e backup ficam presos ao notebook.

| Serviço (Compose) | Imagem                                                                                           | Exposição                                                                  | Observações                                                                                            |
| ----------------- | ------------------------------------------------------------------------------------------------ | -------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------ |
| `app`             | Dockerfile multi-stage: build do `web` e da `api`, imagem final `node:24-slim`, usuário não root | `127.0.0.1:3000`, publicado pelo Funnel                                    | Nest serve `/api` e o build do Vue; roda migrations do MikroORM no start; `/api/health` para o Compose |
| `db`              | `postgres` (versão fixada)                                                                       | Nenhuma porta no host                                                      | Volume nomeado; usuário da aplicação separado do superusuário                                          |
| `seq`             | `datalust/seq`                                                                                   | `127.0.0.1:5341`, acesso pela tailnet com `tailscale serve` (nunca Funnel) | Recebe os logs do pino; retenção de 30 dias                                                            |
| `backup`          | Container com `pg_dump` + cron                                                                   | Nenhuma                                                                    | Dump diário comprimido, guarda 14 dias; cópia externa com restic quando houver destino                 |

**Tailscale:** instalado no host (não em container). `tailscale funnel --bg 3000` publica o app em `https://<máquina>.<tailnet>.ts.net` com certificado automático. A API configura `trust proxy` para loopback, para saber que está atrás de proxy HTTPS e marcar o cookie como seguro. Verificar na implementação se o Funnel repassa o IP do cliente; se não repassar, o throttler de login passa a ser por usuário e global em vez de por IP.

**Segredos e configuração:** `@nestjs/config` com as variáveis validadas por um schema zod no start (app não sobe com configuração faltando). Em desenvolvimento, `apps/api/.env` local; em produção, `deploy/.env`. Os dois fora do git, com `.env.example` versionado.

**Host:** Ubuntu Server ou Debian, atualizações automáticas de segurança, suspensão desligada ao fechar a tampa, containers com `restart: unless-stopped`, e, se a BIOS permitir, ligar sozinho após queda de energia.

**Deploy da fase 0:** `deploy/update.sh` faz `git pull`, `docker compose build` e `docker compose up -d`. `deploy/restore.sh` restaura um dump escolhido. Os dois documentados no `README` e testados uma vez antes de fechar a fase.

## Testes (depois da fase 0) e CI

Testes automatizados ficam para depois da fase 0. Na fase 0, cada fatia fecha com um roteiro de verificação manual contra o Postgres local. Os runners já nascem configurados (Vitest no `web`, Jest do template no `api`), só sem testes, para não precisar reestruturar depois. Quando os testes entrarem, esta é a lista que deve vir primeiro, porque cobre o que a IA mais tende a quebrar sem perceber:

| Suíte                                                  | O que cobre                                                            | Primeiros testes a escrever                                                                                                                                                           |
| ------------------------------------------------------ | ---------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Unitários (Jest na API, Vitest no web)                 | Regras puras: convite, expiração, hash de token, conversão de centavos | Convite expirado, usado ou revogado é recusado                                                                                                                                        |
| Integração (Testcontainers + Postgres real, supertest) | Filtros, subscribers, guards, endpoints                                | Usuário A não lê, altera nem apaga registro de B; remoção vira soft delete; toda alteração gera linha de auditoria; cadastro sem convite falha; rota sem 2FA configurado responde 403 |
| Arquitetura (dependency-cruiser)                       | Imports                                                                | Módulo não importa interno de outro; controller não importa `EntityManager`; `web` não importa nada de `api`                                                                          |

**CI (GitHub Actions) na fase 0:** `pnpm install --frozen-lockfile`, lint, `tsc --noEmit` e build dos três pacotes em cada push e PR. É barato e pega código que não compila antes de chegar no notebook.

O roteiro manual de cada fatia fica no PR dela. O mais importante é o do isolamento: com dois usuários, tentar abrir, editar e apagar um registro do outro, inclusive trocando o id na URL. Esse é o primeiro teste a automatizar quando a hora chegar.

## CLAUDE.md e ADRs

O `CLAUDE.md` é o que toda sessão de IA lê primeiro, então ele fica curto e aponta para os detalhes em `docs/`. Rascunho inicial:

```markdown
# FluxoCaixa

App pessoal de fluxo de caixa e projeção financeira. Monorepo pnpm: apps/web (Vue 3 + Vite + Pinia + PrimeVue, PWA), apps/api (NestJS 11 + MikroORM + PostgreSQL), packages/contracts (schemas zod compartilhados). Plano: docs/plano-base.md. Fase atual: docs/fase-0.md.

## Comandos

- Instalar: pnpm install
- Rodar local: pnpm dev (sobe api e web; Postgres local, conexão em apps/api/.env)
- Lint e tipos: pnpm lint && pnpm typecheck
- Nova migration: pnpm --filter api mikro-orm migration:create
- Testes automatizados: ainda não existem (entram depois da fase 0)

## Regras que não se quebram

- Todo dado tem dono (ownerId); nunca desligue os filtros owner/notDeleted fora do SystemContext.
- Nunca apague de verdade: remoção vira soft delete pelo subscriber.
- Controllers e o chat chamam serviços de aplicação; nunca o EntityManager direto.
- Um módulo só usa o que outro exporta.
- Formato de request/response só em packages/contracts; o front nunca escreve tipo de API à mão.
- Dinheiro em centavos inteiros no código, numeric(14,2) no banco; datas de negócio 'YYYY-MM-DD'; instantes timestamptz UTC.
- Toda fatia termina com um roteiro curto de verificação manual.

## Glossário (código = interface)

- OwnedEntity = registro com dono
- Invite = convite
- AuditEntry = registro de auditoria
- (fase 1) Statement = fatura, Installment = parcela, Planned/Actual = previsto/realizado, SharedFlow = fluxo compartilhado

## Como trabalhar

- Uma fatia vertical por sessão (contrato + API + tela); PR pequeno.
- Decisão de arquitetura nova = ADR em docs/adr antes do código.
```

**ADRs iniciais** (curtos: contexto, decisão, consequências):

1. `0001` Monorepo pnpm com `apps/web`, `apps/api` e `packages/contracts`; um container em produção, mesma origem.
2. `0002` PostgreSQL com MikroORM (unit of work, filtros e subscribers).
3. `0003` Front em Vue 3 SPA + PWA, back em NestJS.
4. `0004` Dados por usuário, sem tenant; compartilhamento por fluxo (implementado na fase 1).
5. `0005` Cadastro por convite, 2FA obrigatório, sessão no servidor com cookie, sem servidor de e-mail.
6. `0006` Acesso pelo Tailscale Funnel, só o app exposto.
7. `0007` Contratos zod compartilhados como fonte única dos formatos da API.

## Ordem de execução

Oito fatias, cada uma cabe numa sessão de IA e vira um PR. Tudo roda local contra o Postgres da sua máquina até a fatia 7; a implantação no notebook fica para o fim.

| #   | Fatia                                                                                                                                                    | Pronto quando                                                                                  |
| --- | -------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------- |
| 1   | Esqueleto: monorepo pnpm, `apps/api` (Nest CLI), `apps/web` (Vite + Vue), `packages/contracts`, ESLint/Prettier, `CLAUDE.md`, ADRs, CI                   | `pnpm dev` sobe os dois; o web chama `/api/health` via proxy do Vite; CI verde                 |
| 2   | Persistência: MikroORM com Postgres local, migrations, `OwnedEntity`, `RequestContext`, filtros `owner` e `notDeleted`, subscriber de dono e soft delete | Com dois usuários de seed, um não enxerga o registro do outro; remover só preenche `deletedAt` |
| 3   | Auditoria: subscriber `onFlush`, `AuditWriter`, schema `audit` só de inserção, interceptor de `CorrelationId`                                            | Uma alteração gera linha com antes e depois no banco                                           |
| 4   | Autenticação base: sessão no Postgres, argon2, guards globais, `@Public()`, `@Roles()`, primeiro admin por link no log, telas de login no web            | Admin criado no primeiro start e logado pelo front; o link morre depois                        |
| 5   | Convites, cadastro por convite, TOTP, passkeys, códigos de recuperação, lockout, throttler, anti-CSRF                                                    | Fluxo completo de convite feito à mão; rota sem 2FA responde 403 e o front leva ao setup       |
| 6   | Telas: meu perfil, admin (usuários, convites, auditoria com filtro), PWA com `vite-plugin-pwa`                                                           | Admin vê o login da conta de teste na auditoria; app instala no iPhone                         |
| 7   | Logs com pino (`nestjs-pino`) enviados ao Seq, `/api/health`                                                                                             | Logs com `CorrelationId` aparecem no Seq local                                                 |
| 8   | Implantação: Dockerfile multi-stage, Compose, backup e scripts, Tailscale Funnel no notebook, teste de restart e restore                                 | Checklist do objetivo todo marcado                                                             |

**Como pedir cada fatia para a IA:** cole o trecho desta spec da fatia, peça junto o roteiro de verificação manual, rode o roteiro e só então feche o PR. Ao fechar a fatia, atualize o `CLAUDE.md` se surgiu convenção nova.

**Para começar:** crie um repositório vazio no GitHub e me passe o nome. Eu faço a fatia 1 e abro o primeiro PR.
