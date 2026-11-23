# deploy/scripts

Scripts de operação (backup e restore do Postgres, publicação no servidor de casa). Ainda vazio.

Convenções:

- PowerShell (`.ps1`) para o que roda no Windows; `.sh` para o que roda no servidor Linux ou em container.
- Cada script começa com um comentário dizendo o que faz e como rodar.
- Nenhum segredo no script: leia de `deploy/.env` ou de variável de ambiente.
