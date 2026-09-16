# Arquitetura do frontend

## Decisões

- React + TypeScript, compilados com Vite.
- Organização prioritariamente por feature/domínio; código específico permanece na feature proprietária.
- React Router centraliza a composição das rotas em `src/app/router` quando as rotas crescerem.
- TanStack Query é a fonte de verdade para server state; mutations invalidam apenas queries afetadas.
- Estado local usa `useState`/`useReducer`; Zustand só será introduzido quando houver necessidade real de estado global compartilhado.
- Formulários usam React Hook Form e validação com Zod, derivando tipos com `z.infer` quando aplicável.
- Contratos da API devem ser gerados a partir do OpenAPI do backend, separados de tipos específicos de UI.
- Infraestrutura HTTP compartilhada fica em `src/shared/api`; operações específicas ficam próximas da feature.
- Testes usam Vitest + Testing Library; fluxos E2E usam Playwright.
- Não serão criadas camadas genéricas, containers de DI ou estruturas de features vazias sem um caso de uso concreto.

## Estrutura inicial

```text
src/
├── app/
├── features/
├── shared/
├── assets/
└── main.tsx
```

As subpastas `api`, `components`, `hooks`, `pages`, `schemas` e `types` serão criadas dentro de cada feature somente quando necessárias.

## Alinhamento com o backend

As fronteiras funcionais acompanham conceitualmente as features do backend ASP.NET Core, sem reproduzir Commands, Queries ou Handlers no frontend. Endpoints correspondem a funções de API, queries a `useQuery` e commands a `useMutation`.
