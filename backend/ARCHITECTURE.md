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
- Cada feature expõe seus próprios endpoints Minimal API.
- Recursos correlatos podem usar `partial class` nos arquivos
  `Classe.Command.cs`, `Classe.Query.cs`, `Classe.Handler.cs`,
  `Classe.Endpoint.cs`, `Classe.Response.cs` e `Classe.Validator.cs`.
