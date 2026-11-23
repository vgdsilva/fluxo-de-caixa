# deploy

Tudo o que sobe o sistema fora do código: containers, variáveis de ambiente de exemplo e scripts de operação.

```text
deploy/
  docker-compose.yml   PostgreSQL local para desenvolvimento
  .env.example         variáveis do compose (copie para deploy/.env, que fica fora do git)
  scripts/             scripts de operação (backup, restore, publicação no servidor de casa)
```

## Banco local

```bash
cp deploy/.env.example deploy/.env    # troque a senha
docker compose -f deploy/docker-compose.yml up -d
```

A connection string do back-end aponta para esse banco (`Host=localhost;Port=5432;Database=fluxo_de_caixa;...`), configurada em `src/backend` por user-secrets ou variável de ambiente, nunca commitada.

## O que entra aqui depois

- `Dockerfile` da API e do front (ou um só, servindo o build do Vue pela API), quando houver deploy no notebook.
- Compose de produção (`docker-compose.prod.yml`) com API, front e banco.
- Scripts em `scripts/`: backup do Postgres, restore e atualização do servidor.
