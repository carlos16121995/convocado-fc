# Plano de Tarefas por Especialista - Sistema de Gestão Futebol Society

Versão: 1.0
Data: 2026-06-02
Responsável: Product Owner
Base: Documento de Requisitos Reestruturado - Futebol Society

## 1. Objetivo

Este arquivo transforma o documento de requisitos em um plano de tarefas separado por especialista do conselho. Ele não autoriza desenvolvimento imediato de código de produto. Antes de implementação, o motor do conselho exige PoC técnica, benchmark, Gate de Processo, Gate de Git, branch a partir de `dev`, plano de commits e validações locais.

Prioridade estratégica definida pelo Dono do Negócio: financeiro primeiro; partidas e matchmaking depois.

## 2. Releases de Referência

| Release | Foco | Resultado esperado |
| --- | --- | --- |
| R0 | Fundação e PoCs técnicas | Arquitetura, auth/RBAC, livro-caixa e matchmaking provados isoladamente |
| R1 | MVP financeiro | Times, usuários, permissões, mensalidades, tesoureiro, caixa e despesas básicas |
| R2 | Partidas e presença | Inscrição, presença, QR/manual, geolocalização configurável e recorrência |
| R3 | Matchmaking e transparência | Algoritmo auditável, semente pública, relatórios liberáveis pelo Dono do Time |
| R4 | SaaS e monetização | Planos, limites, métricas, cobrança da plataforma e escala |

## 3. Tarefas do Product Owner

| ID | Prioridade | Tarefa | Entregável | Critério de aceite |
| --- | --- | --- | --- | --- |
| PO-01 | Alta | Validar escopo do MVP financeiro com o Dono do Negócio | Backlog MVP assinado | Financeiro aparece antes de partidas/matchmaking e não há requisito crítico pendente |
| PO-02 | Alta | Definir personas e jornadas principais | Jornadas de Dono, Tesoureiro, Moderador e Jogador | Cada jornada possui objetivo, pré-condição, fluxo feliz e exceções |
| PO-03 | Alta | Priorizar módulos M01, M03, M05 e M06 | Matriz de prioridade | Cada módulo tem valor, risco, dependência e release |
| PO-04 | Média | Definir política de transparência financeira | Regras de visibilidade configurável | Jogadores só veem dados liberados pelo Dono do Time |
| PO-05 | Média | Definir política comercial de planos | Tabela de planos e limites | Gratuito, Initial, Pro e Smart possuem limites claros |
| PO-06 | Média | Manter perguntas e decisões do Dono do Negócio rastreadas | Registro de decisões | INCREMENTO-001 e INCREMENTO-002 aparecem incorporados |
| PO-07 | Alta | Quebrar épicos em histórias por release | Backlog funcional | Toda história tem regra, aceite, área responsável e dependência |

## 4. Tarefas do Guardião do Processo

| ID | Prioridade | Tarefa | Entregável | Critério de aceite |
| --- | --- | --- | --- | --- |
| GP-01 | Alta | Validar que a entrada incremental foi registrada | Gate de entrada incremental | INCREMENTO-002 existe e está classificado |
| GP-02 | Alta | Validar se tarefas não autorizam desenvolvimento antes da PoC | Parecer processual | Documento declara bloqueio de código antes da PoC |
| GP-03 | Alta | Conferir artefatos obrigatórios antes de execução futura | Checklist de gates | Gates de PoC, tarefas, Git, QA e PR aparecem como obrigatórios |
| GP-04 | Média | Auditar separação por especialista | Aprovação de tarefas | Nenhuma tarefa fica sem dono |
| GP-05 | Média | Bloquear avanço se houver tarefa sem critério | Lista de pendências | Tarefa sem aceite volta ao PO |

## 5. Tarefas do Advogado do Diabo

| ID | Prioridade | Tarefa | Entregável | Critério de aceite |
| --- | --- | --- | --- | --- |
| AD-01 | Alta | Atacar premissas do MVP financeiro | Lista de riscos e alternativas | Todo risco possui impacto, evidência, mitigação e dono |
| AD-02 | Alta | Revisar se o Tesoureiro cria brecha de abuso | Parecer crítico | Permissões financeiras são mínimas e auditáveis |
| AD-03 | Alta | Questionar viabilidade de custo quase zero | Parecer de custo | Free tier tem limites e gatilhos de migração |
| AD-04 | Média | Revisar exposição social das tags financeiras | Parecer LGPD/UX | Tags ficam restritas a ADM/autorizados e jogador |
| AD-05 | Média | Revisar se matchmaking ficou cedo demais no roadmap | Parecer de priorização | Financeiro continua prioridade do MVP |
| AD-06 | Média | Revisar se sorteio com semente pública é auditável | Parecer de auditoria | Empate tem regra rastreável e reproduzível |

## 6. Tarefas de Arquitetura de Software e Dados

| ID | Prioridade | Tarefa | Entregável | Critério de aceite |
| --- | --- | --- | --- | --- |
| ARQ-01 | Alta | Definir arquitetura modular do sistema | Diagrama de módulos e dependências | M01 a M11 possuem fronteiras claras |
| ARQ-02 | Alta | Definir modelo de autorização por escopo de time | Decisão arquitetural | Backend sempre valida timeId, usuário e papel |
| ARQ-03 | Alta | Definir estratégia de livro-caixa imutável | ADR financeiro | Caixa é derivado de lançamentos auditáveis |
| ARQ-04 | Alta | Definir estratégia de outbox/jobs | ADR de eventos | Mensalidades, partidas recorrentes e notificações são idempotentes |
| ARQ-05 | Média | Definir estratégia de storage de comprovantes | ADR de arquivos | Há limite, retenção, acesso e classificação de dado |
| ARQ-06 | Média | Definir estratégia de real-time | ADR real-time | MVP usa in-app/SSE/polling; SignalR só com escala planejada |
| ARQ-07 | Média | Definir observabilidade mínima | Plano de logs/métricas/traces | Logs não expõem dados sensíveis |

## 7. Tarefas de Backend

| ID | Prioridade | Tarefa | Entregável | Critério de aceite |
| --- | --- | --- | --- | --- |
| BE-01 | Alta | Criar contratos de autenticação e sessão | OpenAPI/contratos | Login, recuperação, refresh/logout e usuário logado definidos |
| BE-02 | Alta | Criar RBAC por time | API de papéis e permissões | Dono, Moderador, Tesoureiro e Jogador têm escopo por time |
| BE-03 | Alta | Criar contratos de times e convites | OpenAPI de M03 | Convite por link/QR tem token, expiração e revogação |
| BE-04 | Alta | Criar contratos de mensalidade manual | OpenAPI de M05 | Comprovante, aprovação, rejeição, quitação e histórico definidos |
| BE-05 | Alta | Implementar regra de voto de confiança | Serviço de mensalidade | Jogador joga só quando voto está ativo e deve quitar duas cobranças na próxima mensalidade |
| BE-06 | Alta | Implementar permissão de Tesoureiro | Policies/handlers | Tesoureiro gerencia finanças sem poderes globais do Dono |
| BE-07 | Alta | Criar livro-caixa imutável | Serviços e contratos | Cada entrada/saída tem origem, autor, data, valor e motivo |
| BE-08 | Média | Criar contratos de despesas e metas | OpenAPI de M06 | Despesa paga gera saída; meta recebe superávit conforme prioridade |
| BE-09 | Média | Criar relatório de transparência liberável | API de relatório | Jogador recebe só campos liberados pelo Dono |
| BE-10 | Média | Criar contratos de partidas e presença | OpenAPI de M07 | Raio de presença configurável por partida |
| BE-11 | Média | Criar PoC isolada do matchmaking | Biblioteca/testes | Escassez, empate, semente pública e improviso cobertos |
| BE-12 | Média | Criar jobs idempotentes | Workers/endpoints seguros | Geração N+1 não duplica mensalidade ou partida |
| BE-13 | Alta | Criar testes de autorização e IDOR | Testes automatizados | Usuário de um time não acessa dados de outro |

## 8. Tarefas de Frontend

| ID | Prioridade | Tarefa | Entregável | Critério de aceite |
| --- | --- | --- | --- | --- |
| FE-01 | Alta | Definir design system PWA | Guia UI e componentes | Paleta, tipografia, botões, tabelas e estados definidos |
| FE-02 | Alta | Criar fluxo de login e recuperação | Protótipo/telas | Usuário entende erro, sucesso, loading e sessão expirada |
| FE-03 | Alta | Criar dashboard do time | Tela principal | Dono/Tesoureiro/Jogador veem ações conforme permissão |
| FE-04 | Alta | Criar telas de jogadores e convites | Listas e formulários | Link/QR/WhatsApp e solicitação têm estados claros |
| FE-05 | Alta | Criar módulo financeiro do MVP | Telas de mensalidade, comprovante e caixa | Jogador envia comprovante; Tesoureiro/Dono valida |
| FE-06 | Alta | Criar tela de Tesoureiro/permissões | UI de delegação | Dono atribui/remover Tesoureiro com feedback e confirmação |
| FE-07 | Média | Criar relatório de transparência configurável | Tela de configuração e visualização | Jogador vê somente o que foi liberado |
| FE-08 | Média | Criar telas de despesas e metas | CRUD financeiro | Estados vazio, erro, sucesso e carregando cobertos |
| FE-09 | Média | Criar fluxo de partidas e presença | Telas de inscrição/check-in | Raio configurável, QR/manual e erro de localização tratados |
| FE-10 | Média | Criar visualização do matchmaking | Tela de times gerados | Explica escassez, improviso, empate e semente pública |
| FE-11 | Alta | Garantir responsividade mobile-first | Evidências visuais | Fluxos críticos funcionam bem no celular |
| FE-12 | Alta | Criar testes E2E dos fluxos críticos | Testes Playwright | Login, financeiro, tesoureiro e acesso negado cobertos |

## 9. Tarefas de Banco de Dados

| ID | Prioridade | Tarefa | Entregável | Critério de aceite |
| --- | --- | --- | --- | --- |
| DBA-01 | Alta | Modelar usuários, times e vínculos | DER inicial | Usuário pode ter papéis diferentes por time |
| DBA-02 | Alta | Modelar papéis e permissões por time | Tabelas/constraints | Dono, Moderador, Tesoureiro e Jogador têm integridade |
| DBA-03 | Alta | Modelar mensalidades e comprovantes | Schema M05 | Mensalidade única por jogador/time/período |
| DBA-04 | Alta | Modelar livro-caixa | Schema de ledger | Lançamentos são imutáveis e referenciam origem |
| DBA-05 | Média | Modelar despesas e metas | Schema M06 | Superávit, déficit e metas são rastreáveis |
| DBA-06 | Média | Modelar partidas, inscrições e presença | Schema M07 | Presença, ausência, atraso e justificativa preservam histórico |
| DBA-07 | Média | Modelar matchmaking | Schema M08 | Run, assignments, semente pública e justificativas persistem |
| DBA-08 | Alta | Definir índices iniciais | Plano de índices | Consultas por time, período, status e usuário são cobertas |
| DBA-09 | Alta | Definir constraints críticas | Plano de integridade | Não há duplicidade de cobrança, convite ou vínculo ativo |
| DBA-10 | Média | Definir retenção de comprovantes | Política de dados | Retenção respeita LGPD e custo de storage |

## 10. Tarefas de QA

| ID | Prioridade | Tarefa | Entregável | Critério de aceite |
| --- | --- | --- | --- | --- |
| QA-01 | Alta | Criar matriz de testes do MVP financeiro | Plano de QA | Mensalidade, comprovante, tesoureiro e caixa cobertos |
| QA-02 | Alta | Criar cenários de permissão | Casos de teste | Dono, Moderador, Tesoureiro e Jogador testados |
| QA-03 | Alta | Criar testes de fluxo de voto de confiança | Casos de teste | Ativo permite jogar; não quitar duas mensalidades bloqueia |
| QA-04 | Alta | Criar cenários de acesso negado/IDOR | Casos de abuso | Usuário não acessa outro time via API |
| QA-05 | Média | Criar cenários de relatório de transparência | Casos de teste | Jogador vê somente dados liberados |
| QA-06 | Média | Criar cenários de presença configurável | Casos de teste | Raio por partida, QR e manual cobertos |
| QA-07 | Média | Criar cenários de matchmaking | Casos de teste | Escassez, empate, semente pública e improviso cobertos |
| QA-08 | Alta | Definir massa de dados local | Plano de massa | Times, jogadores, papéis, mensalidades e despesas prontos |
| QA-09 | Alta | Definir regressão visual/responsiva | Checklist UI | Fluxos críticos testados em mobile e desktop |
| QA-10 | Média | Definir critérios de aceite por release | Matriz de aceite | Cada release tem aceite funcional e não funcional |

## 11. Tarefas de Segurança

| ID | Prioridade | Tarefa | Entregável | Critério de aceite |
| --- | --- | --- | --- | --- |
| SEG-01 | Alta | Criar matriz de ameaças | Threat model | IDOR, abuso financeiro, convite e comprovante cobertos |
| SEG-02 | Alta | Definir controles de RBAC por time | Matriz de permissões | Backend valida papel e escopo em todas as ações |
| SEG-03 | Alta | Definir política de dados financeiros | Política LGPD | Tags financeiras restritas a ADM/autorizados e jogador |
| SEG-04 | Alta | Definir segurança de comprovantes | Política de storage | Arquivo privado, com retenção, limite e auditoria |
| SEG-05 | Alta | Definir segurança de convites/QR | Regras de token | Token expira, pode ser revogado e não expõe dados internos |
| SEG-06 | Média | Definir logs seguros | Guia de logging | Logs não armazenam senha, token, comprovante ou chave PIX completa |
| SEG-07 | Média | Definir cenários de abuso para QA | Checklist de abuso | Bypass de permissão, API direta e alteração de timeId cobertos |
| SEG-08 | Média | Avaliar ferramentas gratuitas de segurança | Plano de ferramentas | Gitleaks, Semgrep e OWASP ZAP definidos para pipeline/local |
| SEG-09 | Média | Definir política de semente pública no matchmaking | Parecer de auditabilidade | Semente não expõe dado pessoal e permite reproduzir empate |

## 12. Dependências Críticas entre Especialistas

| Dependência | Origem | Destino | Motivo |
| --- | --- | --- | --- |
| Matriz de papéis por time | PO/Segurança | Backend/Frontend/DBA/QA | Define quem pode executar cada ação |
| Modelo de dados financeiro | DBA/Arquitetura | Backend/QA | Necessário para mensalidade, caixa e despesas |
| Contratos de API | Backend | Frontend/QA | Frontend e QA dependem dos payloads e status codes |
| Política de transparência | PO/Segurança | Backend/Frontend/QA | Define campos visíveis para jogadores |
| PoC de matchmaking | Backend/Arquitetura/QA | Frontend/DBA | Define saída, persistência e explicação visual |
| Gatilhos de escala | Arquitetura/DBA/Segurança | PO | Define custo e roadmap |

## 13. Ordem Recomendada de Execução

1. PO valida backlog MVP financeiro e decisões finais.
2. Segurança e Arquitetura fecham matriz de papéis, threat model e autorização por time.
3. DBA modela usuários, times, permissões, mensalidades e livro-caixa.
4. Backend cria contratos e PoCs técnicas de auth/RBAC, livro-caixa e jobs.
5. Frontend prototipa dashboard, financeiro, comprovante e tesoureiro.
6. QA cria matriz de testes e massa local.
7. Conselho revisa PoC e benchmark antes de qualquer implementação de produto.
8. Só depois: Gate de Processo, Gate de Git, branch a partir de `dev`, commits por módulo e desenvolvimento.

## 14. Bloqueios Antes do Desenvolvimento

- PoC técnica de autorização por time ainda precisa ser executada.
- PoC técnica do livro-caixa imutável ainda precisa ser executada.
- PoC técnica do matchmaking ainda precisa ser executada.
- Contratos de API ainda precisam ser aprovados.
- Modelo de dados ainda precisa ser aprovado.
- Gate de Git ainda não se aplica porque esta entrega é documental.
- Testes locais e QA funcional ainda não se aplicam porque não houve código de produto.

## 15. Definição de Pronto para Iniciar Código

- Backlog MVP financeiro aprovado pelo Dono do Negócio.
- PoCs técnicas aprovadas pelo conselho com benchmark.
- Tarefas por especialista aprovadas pelo Guardião do Processo.
- Matriz de permissões fechada.
- Contratos de API e modelo de dados aprovados.
- Plano de testes criado.
- Gate de Git executado no repositório alvo.
- Branch criada a partir de `dev`.
