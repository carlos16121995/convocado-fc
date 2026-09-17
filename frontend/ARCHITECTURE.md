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
- O Design System público fica em `src/components/ui`: tokens CSS semânticos,
  `ThemeProvider` e componentes reutilizáveis. Componentes consomem tokens e
  não possuem ramificações específicas para temas claros ou escuros.
- A rota interna `/design-system` é a documentação executável do sistema visual;
  ela compõe os componentes públicos, mas não contém regras de negócio.
- A paleta base usa `#03BB85`, `#1E8768`, `#235446`, `#20332D`, `#2A3330`,
  `#2D383A` e `#F8F8FF`. `warning` representa aviso em amarelo, `alert`
  representa alerta em laranja, `danger` representa erro em vermelho,
  `success` é um verde distinto da cor de marca e `processing` é azul.
  `info` permanece como alias de compatibilidade para `processing`.
- Listas usam o componente público `Table` para ordenação local e detalhes
  expansíveis. Seleção pesquisável, accordion e confirmação inline também são
  componentes públicos em `src/components/ui`; a confirmação inline não
  substitui o diálogo modal em casos que exijam bloqueio de fluxo.
- Accordion e detalhes de tabela usam transições de 200ms e respeitam
  `prefers-reduced-motion`. `DateInput` apresenta um calendário próprio,
  localizado e baseado nos tokens do Design System, sem delegar a interação ao
  seletor nativo do navegador.
- `Select`, `AutocompleteSelect` e `DateInput` compartilham a mesma moldura,
  espaçamento e área de ação à direita para manter os controles de seleção
  visualmente consistentes. `Select` também usa lista própria, preservando um
  campo oculto somente para integração de formulários.
- A iconografia pública fica em `src/components/ui/icons.tsx`, baseada em
  `lucide-react` e em `currentColor`, para reagir aos tokens sem bifurcações de
  tema. Bola, cone, luva de goleiro, chuteira com travas, grama, traves e
  placar são vetores próprios; a caneleira usa o ícone `ShieldCheck`.
  `JerseyIcon` cobre números de camisa de 1 a 999 centralizados.

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
