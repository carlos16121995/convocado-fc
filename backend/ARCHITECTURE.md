# Arquitetura do backend

## Decisão

O backend é uma aplicação única ASP.NET Core (`Convocado.Api`) em .NET 10,
organizada por Vertical Slice Architecture, Minimal APIs e CQRS conceitual.

## Estrutura

```text
Convocado.Api/
├── Features/        # Casos de uso e respectivos endpoints
├── Domain/          # Modelo e regras de negócio compartilhados
├── Infrastructure/  # Persistência e integrações técnicas
└── Program.cs       # Composição da aplicação e DI
```

## Convenções

- Uma feature agrupa tudo que pertence a um caso de uso, e não a uma camada
  técnica. Exemplo: `Features/Vehicles/Create`.
- Commands alteram estado; queries apenas leem dados. Um handler recebe somente
  suas dependências via injeção de dependência nativa do ASP.NET Core.
- Quando uma query usar Entity Framework Core sem necessidade de rastreamento,
  ela deve aplicar `AsNoTracking()`.
- A persistência usa dois contextos injetados: `QueryDbContext` tem
  `QueryTrackingBehavior.NoTracking` como padrão; `CommandDbContext` usa
  `QueryTrackingBehavior.TrackAll` e é o contexto de migrations.
- Ambos os contextos compartilham a configuração Fluent API. A configuração
  base declara a extensão PostgreSQL `postgis`; migrations ficam em
  `Infrastructure/Migrations` e são criadas pelo `dotnet-ef` versionado no
  manifesto local de ferramentas.
- Respostas HTTP de negócio usam `ApiResponse<T>` ou `ApiResponse`, com UUID
  de correlação, erros de campo e paginação opcional. O middleware propaga o
  identificador no header `X-Correlation-Id`; erros preservam o status HTTP
  correspondente, em vez de serem convertidos para sucesso.
- Cada feature expõe seus próprios endpoints Minimal API.

## Convenções de dados e comandos

- Commands persistentes usam transação no `CommandDbContext`; chamadas a
  provedores externos acontecem somente depois do commit ou por outbox.
- Valores monetários usam `decimal(19,4)` e moeda explícita. A configuração
  Fluent API da entidade proprietária define essa precisão antes da migration.
- Instantes são armazenados em UTC. Calendário e competência usam o fuso IANA
  do time; enquanto o time ainda não existir, o padrão configurável é
  `America/Sao_Paulo`.
- Operações idempotentes futuras recebem `Idempotency-Key`, com escopo da
  identidade autenticada e do caso de uso, retenção definida pela feature e
  comparação de payload; uma chave repetida com payload diferente falha.
- Recursos correlatos podem usar `partial class` nos arquivos
  `Classe.Command.cs`, `Classe.Query.cs`, `Classe.Handler.cs`,
  `Classe.Endpoint.cs`, `Classe.Response.cs` e `Classe.Validator.cs`.
