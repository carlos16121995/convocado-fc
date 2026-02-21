# Preferências e Diretrizes do Projeto

## 1. Stack Tecnológica
- Runtime: .NET 10 (C#)
- ORM principal: Entity Framework Core
- Performance/Queries: Linq2Db
- Banco de dados: PostgreSQL (PostGIS ativado)
- Validação: FluentValidation
- Padrão de execução: totalmente assíncrono (async/await)
- Injeção de dependência: nativa do .NET
- Linguagem do código: inglês
- Linguagem das mensagens para usuários: português

## 2. Arquitetura e Camadas (Clean Architecture + SoC)
- Controller: Controllers, Request/Response Models, Middlewares
  - Recebe requests, valida contratos e retorna HTTP
- Application: Handlers, Validators
  - Orquestra fluxo, regras de negócio e valida integridade
- Domain: DTOs (records), Entidades
  - Coração do sistema e modelos de dados
- Infrastructure: Contexto (DB), Repositórios
  - Persistência e integrações externas

### Organização por Módulos
- Cada módulo deve ser separado por pastas.
- Cada módulo deve possuir sua estrutura de pastas individuais para garantir organização e isolamento.

### Estrutura Atual por Módulos (estado do projeto)
- **Application**: Handlers por módulo em `ConvocadoFc.Application/Handlers/Modules/[Modulo]/` com subpastas `Interfaces`, `Implementations`, `Models`.
  - Users: `IRegisterUserHandler` + `RegisterUserHandler` + models de comando/resultado.
  - Authentication: `IJwtTokenService`, `IRefreshTokenManager` e `AuthConstants`.
  - Notifications: `INotificationService`, `INotificationProvider`, `IMessageTransport`, `IEmailTemplateRenderer` + models de notificação.
  - Shared: `IAppUrlProvider` para URLs do App.
- **Domain**: modelos por módulo em `ConvocadoFc.Domain/Models/Modules/[Modulo]/`.
  - Users (Identity) e Notifications (logs/reasons/channels).
- **Infrastructure**: implementações por módulo em `ConvocadoFc.Infrastructure/Modules/[Modulo]/`.
  - Authentication (JWT, refresh tokens) e Notifications (email provider, templates).
- **WebApi**: endpoints por módulo em `ConvocadoFc.WebApi/Modules/[Modulo]/`.
  - Authentication e Users (register, roles, auth flows).

### Schemas por módulo (Banco de Dados)
- Cada módulo possui schema dedicado no banco.
- Mapeamento atual no EF Core:
  - Users schema: tabelas de Identity.
  - Notifications schema: `NotificationLogs`.

## 3. Padrões de Comunicação e Resposta
### HTTP
- GET: recuperação
- POST: criação
- PUT: atualização total
- PATCH: atualização parcial
- DELETE: remoção

### Contratos de Resposta
**Com dados (genérica)**
```csharp
public record ApiResponse<T>
{
    public int StatusCode { get; init; }
    public string Message { get; init; }
    public bool Success { get; init; }
    public List<ValidationFailure> Errors { get; init; } = new();
    public T? Data { get; init; }
}
```

**Sem dados (ações/comandos)**
```csharp
public record ApiResponse
{
    public int StatusCode { get; init; }
    public string Message { get; init; }
    public bool Success { get; init; }
    public List<ValidationFailure> Errors { get; init; } = new();
}
```

**Detalhes de erro**
```csharp
public class ValidationFailure
{
    public string PropertyName { get; set; }
    public string ErrorMessage { get; set; }
}
```

### Paginação (respostas em lista)
- Query: Coluna ordenadora, Quantidade por página, Página
- Resposta: Quantidade total de itens e lista paginada

## 4. Fluxo de Validação e Consistência
- **Validação de contrato (Controller):** FluentValidation para obrigatoriedade, tipos, tamanhos
- **Validação de integridade (Application):** regras de negócio no início dos handlers
- **Transações:** operações de escrita devem usar transações (considerar Outbox quando necessário)
- **Imutabilidade:** usar records na camada Domain para DTOs

## 5. Performance e Processamento
- Tempo de resposta: até 2 segundos
- Processamentos longos: delegar para filas (queues)
- Agendamentos: tarefas recorrentes via jobs

## 6. Diretrizes de Desenvolvimento
- Clean Code: nomes claros e funções pequenas
- DRY: reutilização e abstrações genéricas
- Desempenho: usar Linq2Db para leituras complexas e críticas
- Enums: todos os enums devem começar com o prefixo "E" (ex.: ETeamMemberRole)
