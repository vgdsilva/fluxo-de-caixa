# Verificação: front-end base + login do redesign

Pré-requisito: `cd src/frontend`, `cp .env.example .env` e `npm install`.

## Sem back-end (API falsa)

1. `npm run dev:mock` e abra <http://localhost:8080>. Vai para `/auth/login`.
2. Desktop (largura ≥ 1000 px): à esquerda o painel verde com as prévias (Visão do mês, Gastos por
   categoria, Últimos lançamentos, Assistente IA); à direita "Seja bem-vindo(a)!" e "Entrar com e-mail".
   Celular: só o formulário. Tudo em Quicksand.
3. "Entrar com e-mail" → passo do e-mail. `admin@` mostra "Esse e-mail não parece válido.".
4. `admin@example.com` → passo da senha, com o e-mail e o link "Trocar".
5. Senha errada → "Validando sua sessão…" e depois "E-mail ou senha incorretos" com "Tentar novamente".
   "Tentar novamente" volta para a senha, com o campo vazio.
6. `admin123` → "Validando sua sessão…" → Início, com o menu e a engrenagem de administração.
7. Pare o dev server no meio do passo 6 (ou desligue a rede): aparece "Não foi possível validar sua sessão";
   "Tentar novamente" repete a validação sem pedir a senha de novo.
8. Meu perfil mostra até quando vale o acesso. O token do mock dura 2 min e é renovado sozinho perto do fim
   (veja `POST /api/auth/refresh` na aba Rede).
9. "Sair" leva para "Você saiu da sua conta"; recarregar a página continua deslogado.

## Com o back-end

1. API rodando (`dotnet run --project src/backend/WebAPI`) e `npm run dev`.
2. Entre com o e-mail do admin (`admin@local.test` em Development) e a senha de `Admin:Password`.
3. Na aba Rede: `POST /api/auth/login` 200 e `GET /api/me` 200 com `Authorization: Bearer`.
