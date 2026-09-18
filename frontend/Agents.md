# Instruções para agentes

## Configuração e segredos

- Centralize as variáveis de ambiente e os valores de configuração em um único
  arquivo de configuração do frontend, para facilitar mudanças futuras.
- Não espalhe leituras de `import.meta.env` pelas features ou componentes; use
  o módulo centralizado para expor somente a configuração necessária.
- O bundle do frontend é público: nenhum segredo real deve ser incluído nele.
  Credenciais sensíveis devem permanecer no backend, mesmo que sua referência
  de configuração esteja centralizada no frontend.

## Responsividade e compatibilidade mobile

O frontend deve ser desenvolvido com abordagem responsive-first e permanecer preparado para futura distribuição como aplicativo Android/iOS usando Capacitor, sem exigir uma reescrita significativa.

### Regras obrigatórias

- Toda nova tela, componente ou fluxo deve funcionar adequadamente em desktop, tablet e dispositivos móveis.
- Não crie layouts dependentes exclusivamente de resoluções desktop.
- Evite dimensões fixas que prejudiquem a responsividade. Prefira layouts fluidos, Flexbox, Grid e breakpoints quando necessário.
- Considere interação por toque em todos os componentes interativos. Funcionalidades essenciais não podem depender exclusivamente de hover, mouse ou teclado.
- Não acople features diretamente a APIs específicas do navegador quando houver possibilidade de dependência da plataforma.
- Encapsule recursos relacionados a dispositivo ou plataforma atrás de uma abstração própria.

Exemplo de direção arquitetural:

```text
features/
        ↓
shared/platform/
        ↓
 ┌──────┴──────┐
 │             │
 Web       Capacitor
```

Evite espalhar chamadas como estas diretamente pelas features:

```ts
navigator.geolocation
navigator.clipboard
window.localStorage
Notification
```

Quando um recurso tiver impacto relevante de plataforma, exponha uma API própria, por exemplo:

```ts
locationService.getCurrentPosition()
storageService.get()
notificationService.requestPermission()
```

- Não adicione dependências do Capacitor antes de existir uma necessidade concreta.
- Não utilize APIs nativas como requisito para funcionalidades que também precisam funcionar na versão web, salvo solicitação explícita.
- Preserve a separação entre lógica de negócio, interface e recursos específicos da plataforma.
- Componentes compartilhados devem considerar responsividade e interação por toque desde sua criação.
- A futura adoção do Capacitor deve exigir principalmente a implementação ou adaptação da camada de plataforma, e não alterações generalizadas nas features.

### Organização e contratos

- Mantenha a organização prioritariamente por feature ou domínio.
- Coloque infraestrutura HTTP compartilhada em `src/shared/api` e abstrações de plataforma em `src/shared/platform`.
- Use TanStack Query para server state; invalide somente as queries afetadas por mutations.
- Use `useState` ou `useReducer` para estado local e introduza estado global somente quando houver necessidade real.
- Use React Hook Form e Zod para formulários e derive tipos com `z.infer` quando aplicável.
- Mantenha contratos de API separados de tipos específicos de UI e, quando possível, derivados do OpenAPI do backend.
- Não crie camadas genéricas, containers de DI ou estruturas vazias sem um caso de uso concreto.

### Validação antes de concluir

- Antes de concluir alterações de UI, verifique o comportamento em uma viewport mobile e em uma viewport desktop.
- Confirme que conteúdo, controles e textos não transbordam nem se sobrepõem em telas estreitas.
- Confirme que os fluxos essenciais podem ser executados por toque, sem depender de hover.
- Execute `npm run build` e, quando aplicável, `npm run lint` e os testes direcionados.

## Design System

Toda interface deve utilizar os componentes e design tokens definidos pelo
Design System.

É proibido:

- utilizar cores hex/rgb diretamente em features;
- criar espaçamentos arbitrários;
- criar componentes locais quando existir equivalente no Design System;
- implementar estilos específicos para dark mode dentro de features;
- duplicar componentes do Design System.

Antes de criar um novo componente visual, verifique se o Design System já
possui um componente equivalente.

Todo componente interativo deve suportar, quando aplicável:

- tema claro e escuro;
- navegação por teclado;
- foco visível;
- estado desabilitado;
- layouts responsivos;
- interação mobile/toque;
- estados de loading, vazio e erro;
- semântica HTML e atributos ARIA necessários.

## Objetivo arquitetural

Preserve a evolução do frontend nesta direção:

```text
React + TypeScript
        ↓
       Web
```

para:

```text
React + TypeScript
        ↓
     Capacitor
      ↙     ↘
 Android   iOS
```

As mesmas features, componentes, contratos de API, validações, gerenciamento de server state e regras de negócio devem ser mantidos sempre que possível.
