# Infinite Content AI

## Ambiente de desenvolvimento

Copie `.env.example` para `.env` se precisar alterar os padrões e inicie o
PostgreSQL:

```powershell
docker compose up -d
```

Depois aplique as migrations e execute a API:

```powershell
dotnet ef database update --project src/InfiniteContentAI.Data --startup-project src/InfiniteContentAI.Api
dotnet run --project src/InfiniteContentAI.Api
```
