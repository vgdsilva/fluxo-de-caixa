# tests

Testes que ficam fora do código de produção.

```text
tests/
  WebAPI.Test/     xUnit do back-end (referencia ../../src/backend/WebAPI)
  e2e/             testes ponta a ponta (front + API), quando existirem
```

- Os projetos de teste .NET entram na solução `src/backend/WebAPI.slnx`, então `dotnet test src/backend/WebAPI.slnx` roda tudo.
- Testes que usam banco leem a connection string de `WEBAPI_TEST_POSTGRES` e são pulados quando ela não existe.
- Testes de unidade do front ficam dentro de `src/frontend`, perto do componente, como é o costume no Vue.
