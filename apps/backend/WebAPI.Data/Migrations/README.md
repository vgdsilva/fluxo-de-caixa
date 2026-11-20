# Migrations

Gerar a primeira migration (a partir da raiz da solução):

```bash
dotnet tool install --global dotnet-ef
dotnet ef migrations add Inicial --project WebAPI.Data --startup-project WebAPI --output-dir Migrations
dotnet ef database update --project WebAPI.Data --startup-project WebAPI
```
