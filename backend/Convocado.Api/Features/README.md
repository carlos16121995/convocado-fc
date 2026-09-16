# Features

Organize cada caso de uso em sua própria pasta, por exemplo
`Vehicles/Create` ou `Vehicles/Search`. Cada slice define seu endpoint Minimal
API, command ou query, handler, contrato de resposta e validador quando
necessário.

Use `partial class` para agrupar arquivos de uma mesma slice com a convenção
`Classe.Command.cs`, `Classe.Query.cs`, `Classe.Handler.cs`,
`Classe.Endpoint.cs`, `Classe.Response.cs` e `Classe.Validator.cs`.
