# Instruções para agentes

## Configuração e segredos

- Mantenha variáveis e valores de configuração não sensíveis nos arquivos
  `appsettings*.json` do projeto `Convocado.Api`.
- Connection strings sem credenciais podem permanecer no `appsettings.json`.
  Em desenvolvimento, credenciais devem ficar no User Secrets do .NET, que
  sobrescreve a connection string sem expô-las no repositório.
- Centralize a leitura dessas configurações na composição da aplicação; não
  replique valores de configuração ou segredos nas features, entidades ou
  integrações.

## Persistência e Entity Framework Core

- Toda alteração de estrutura ou dados da base deve ser aplicada por uma
  migration do Entity Framework Core. Não altere a base manualmente.
- Crie migrations com os comandos do EF Core e mantenha os arquivos gerados em
  `Convocado.Api/Infrastructure/Migrations`.
- Configure todas as entidades com Fluent API. As configurações devem estar
  disponíveis ao EF Core quando a migration for criada e aplicada.
- Toda entidade persistida deve pertencer explicitamente a um schema de banco
  de dados, definido pela configuração Fluent API.
