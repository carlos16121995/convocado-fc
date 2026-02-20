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
