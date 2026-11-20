# Atalhos (Linux/macOS/WSL ou Windows com make). No Windows puro use build.ps1 / clean.ps1.
SOLUTION := WebAPI.sln
API      := WebAPI
DATA     := WebAPI.Data

.PHONY: restore build test run worker clean migration db-update

restore:
	dotnet restore $(SOLUTION)

build:
	dotnet build $(SOLUTION) -c Release

test:
	dotnet test $(SOLUTION)

run:
	dotnet run --project $(API)

worker:
	dotnet run --project WebAPI.Worker

clean:
	dotnet clean $(SOLUTION)
	find . -type d \( -name bin -o -name obj \) -prune -exec rm -rf {} +

# Uso: make migration NAME=Inicial
migration:
	dotnet ef migrations add $(NAME) --project $(DATA) --startup-project $(API) --output-dir Migrations

db-update:
	dotnet ef database update --project $(DATA) --startup-project $(API)
