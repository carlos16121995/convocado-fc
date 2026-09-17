# Documento de Requisitos Reestruturado - Sistema de Gestão Futebol Society

Versão: 1.0
Data: 2026-06-02
Origem: unificação dos documentos `Convocado.docx` e `Proposta de Sistema de Gestão Futebol Society.docx`
Dono do Negócio: Carlos Alcantara

## 1. Decisão Executiva

O produto deve ser tratado como uma plataforma SaaS para gestão de times de futebol society, começando por um MVP gratuito ou de custo quase zero para validar o uso real em até 1 time e 40 jogadores, mas já com fundações para multi-times, cobrança por planos, controle financeiro, presença, convocações e matchmaking.

A recomendação do conselho é dividir o sistema em módulos independentes, implementar primeiro o núcleo de identidade, times, jogadores e financeiro básico, e só depois liberar matchmaking avançado, relatórios e automações de escala. Essa ordem reduz risco porque o valor de negócio inicial está em organizar mensalidades, presença e comunicação, enquanto o algoritmo de times precisa de histórico e dados confiáveis para ser justo.

Decisões aprovadas:

- Arquitetura: API .NET 10, PWA React + Vite + TypeScript, PostgreSQL com PostGIS e módulos por domínio.
- Infra inicial: Firebase Hosting para frontend, Cloud Run para API, Supabase PostgreSQL/PostGIS para banco, GitHub Actions para CI e ferramentas gratuitas de teste e segurança.
- Auth recomendada para MVP: Supabase Auth ou Firebase Auth como provedor de identidade gratuito, com autorização por time controlada no backend. ASP.NET Core Identity fica como alternativa quando houver necessidade de controle total e menor dependência de provedor.
- Real-time no MVP: usar notificações in-app persistidas e polling/SSE para estados simples. SignalR só deve entrar quando houver necessidade real de tempo real bidirecional; para escalar, usar backplane/serviço gerenciado.
- Financeiro: toda movimentação deve passar por livro-caixa imutável. Mensalidade quitada, despesa paga, meta debitada ou ajuste manual geram lançamento auditável.
- LGPD e reputação financeira: tags financeiras devem ser visíveis apenas para administradores autorizados e para o próprio jogador.

## 2. Problema de Negócio

Grupos de futebol society administram jogadores, mensalidades, despesas, presença, cobranças, escalação e comunicação usando ferramentas dispersas como WhatsApp, planilhas e memória dos organizadores. Isso gera atrasos, conflitos, baixa transparência financeira, pouca previsibilidade de caixa e dificuldade para montar times equilibrados.

O sistema deve reduzir trabalho operacional do dono/moderador do time, aumentar transparência, melhorar presença nos jogos, tornar cobranças rastreáveis e criar uma experiência escalável para que a solução se torne rentável por assinatura.

## 3. Objetivos

- Centralizar cadastro de times, jogadores, perfis, convites e permissões.
- Controlar mensalidades, comprovantes, despesas, metas, caixa e relatórios.
- Bloquear inscrição em partidas para jogadores sem elegibilidade financeira, salvo exceções configuradas.
- Gerenciar partidas únicas e recorrentes com presença por geolocalização, QR Code e validação manual.
- Gerar times automaticamente por regras de preferência, histórico, pontuação dinâmica e proteção por escassez.
- Notificar jogadores e administradores por canais gratuitos ou de baixo custo.
- Viabilizar monetização por planos, limites de uso e serviços premium.
- Manter segurança, privacidade, auditoria e capacidade de escala desde o MVP.

## 4. Perfis de Usuário e Papéis

### 4.1 Papéis Globais

Super Admin do Sistema:
- Mantém parâmetros globais, suporte, auditoria, planos, banimentos e intervenção excepcional.
- Não deve administrar rotinas de times sem registro auditável.

Admin Comercial/Operacional da Plataforma:
- Gerencia assinaturas, suporte e relacionamento com clientes.
- Não deve ter acesso desnecessário a comprovantes ou dados sensíveis.

### 4.2 Papéis por Time

Dono do Time:
- Criador ou responsável principal pelo time.
- Pode editar dados do time, configurar mensalidade, posições, permissões, metas, despesas e relatório de transparência.
- Pode promover/remover moderadores e decidir exceções financeiras.

Moderador:
- Ajuda na operação diária.
- Pode gerenciar convites, solicitações, presença, justificativas, partidas e validação operacional conforme permissões delegadas.
- Pode gerenciar finanças somente quando receber a permissão específica de Tesoureiro do Time.
- Não pode alterar plano, dono do time, regras globais ou permissões críticas sem autorização.

Jogador:
- Mantém perfil, preferências de posição, confirma presença, consulta mensalidades, envia comprovantes e participa das partidas permitidas.
- Só acessa informações detalhadas de times aceitos.

Visitante/Pré-cadastro:
- Pode acessar convite, QR Code ou link público, preencher cadastro e solicitar entrada.
- Não vê dados internos antes de aprovação.

## 5. Mapa Modular

### M01 - Identidade, Autenticação e Autorização

Objetivo: autenticar usuários e aplicar permissões globais e por time.

Requisitos funcionais:
- RF-M01-01: cadastrar usuário com nome, e-mail único, telefone e senha ou provedor social.
- RF-M01-02: permitir login por e-mail/senha e login social Google.
- RF-M01-03: permitir recuperação e alteração de senha.
- RF-M01-04: emitir sessão segura com expiração e renovação.
- RF-M01-05: manter papéis globais e papéis por time.
- RF-M01-06: permitir que um usuário tenha papéis diferentes em times diferentes.
- RF-M01-07: registrar auditoria de mudanças de papel, bloqueios e ações administrativas.

Regras de negócio:
- RN-M01-01: e-mail é identificador único de login.
- RN-M01-02: ações administrativas devem validar permissão no backend.
- RN-M01-03: validação no frontend não substitui autorização no servidor.
- RN-M01-04: GUID/UUID deve ser usado para identificadores públicos.
- RN-M01-05: Super Admin só deve intervir em time com justificativa auditada.

Critérios de aceite:
- Usuário sem papel adequado recebe acesso negado por API e interface.
- Usuário com papéis em dois times executa ações apenas no escopo correto.
- Troca de papel gera registro de auditoria.

### M02 - Usuários, Jogadores e Ciclo de Vida

Objetivo: manter dados pessoais, perfil esportivo, vínculos com times e status operacional.

Requisitos funcionais:
- RF-M02-01: manter perfil com nome, e-mail, telefone, foto e endereço opcional.
- RF-M02-02: manter preferências de posição primária, secundária e terciária.
- RF-M02-03: permitir distribuição de 6 pontos de afinidade nas combinações aprovadas.
- RF-M02-04: permitir status ativo, inativo, em hiato e isento.
- RF-M02-05: inativar vínculo com time sem apagar histórico.
- RF-M02-06: buscar times por nome, link/QR Code e proximidade geográfica.
- RF-M02-07: permitir solicitação de entrada com mensagem para administradores.

Regras de negócio:
- RN-M02-01: jogador inativo não conta no limite ativo do plano.
- RN-M02-02: jogador em hiato não gera mensalidade e não pode jogar após expirar a última mensalidade válida.
- RN-M02-03: histórico financeiro, partidas e presenças não pode ser excluído ao remover vínculo.
- RN-M02-04: dados pessoais devem obedecer princípios de finalidade, minimização e necessidade.

Critérios de aceite:
- Remover jogador de time preserva histórico e tira acesso operacional.
- Busca por geolocalização só retorna times permitidos e dentro de raio configurado.
- Jogador sem vínculo aceito não vê dados internos do time.

### M03 - Times, Convites e Administração

Objetivo: administrar times, convites, solicitações, formações, sede e regras internas.

Requisitos funcionais:
- RF-M03-01: criar time com nome, brasão/logo, descrição e campo sede.
- RF-M03-02: configurar local do campo com latitude/longitude e raio operacional.
- RF-M03-03: convidar jogadores por e-mail, link, WhatsApp e QR Code.
- RF-M03-04: listar convites enviados, recebidos e solicitações pendentes.
- RF-M03-05: aceitar ou recusar solicitação de entrada.
- RF-M03-06: configurar formações permitidas por quantidade de jogadores.
- RF-M03-07: configurar posições, áreas do campo e bonificações financeiras por posição preferencial.
- RF-M03-08: ativar/desativar relatório de transparência.
- RF-M03-09: configurar raio de busca do time por proximidade geográfica.

Regras de negócio:
- RN-M03-01: criação de time exige plano ativo com limite disponível.
- RN-M03-02: criador do time torna-se Dono do Time.
- RN-M03-03: um usuário pode participar de múltiplos times conforme plano e permissões.
- RN-M03-04: convite expirado ou usado não pode ser reutilizado.
- RN-M03-05: QR Code deve ter token curto, expiração e revogação.
- RN-M03-06: raio de busca de times deve ser configurável, não fixo em 2 km.

Critérios de aceite:
- Link/QR Code cria solicitação correta para usuário novo e existente.
- Time não pode ultrapassar limite de membros ativos do plano.
- Moderador não consegue promover outro moderador se não tiver permissão delegada.

### M04 - Planos, Assinaturas e Monetização Técnica

Objetivo: controlar acesso a recursos por plano e preparar o SaaS para receita recorrente.

Planos propostos:
- Gratuito/MVP: 1 time, até 40 jogadores ativos, jogos recorrentes limitados, sem relatórios avançados.
- Initial: 1 time, 40 jogadores, notificações e financeiro básico.
- Pro: até 3 times, 60 jogadores por time, relatórios, automações e histórico expandido.
- Smart/Clube: limites negociados, suporte prioritário, múltiplos administradores e recursos avançados.

Requisitos funcionais:
- RF-M04-01: listar planos e limites.
- RF-M04-02: atribuir, alterar, remover ou expirar assinatura.
- RF-M04-03: bloquear criação de time quando limite/plano não permitir.
- RF-M04-04: aplicar limites por time, usuário, partidas recorrentes, armazenamento e relatórios.
- RF-M04-05: manter histórico de plano para auditoria e faturamento futuro.

Regras de negócio:
- RN-M04-01: sem plano ativo, usuário não cria time.
- RN-M04-02: downgrade não apaga dados; bloqueia novas ações acima do limite.
- RN-M04-03: planos pagos devem ser separados de controle financeiro interno do time.

Critérios de aceite:
- Ao exceder limite, sistema mostra bloqueio claro e opção de upgrade.
- Troca de plano altera permissões e limites sem perda de histórico.

### M05 - Mensalidades, Pagamentos e Exceções

Objetivo: controlar cobrança mensal dos jogadores e elegibilidade financeira.

Requisitos funcionais:
- RF-M05-01: configurar valor base da mensalidade do time.
- RF-M05-02: configurar dados de pagamento: beneficiário, chave PIX, banco e QR Code.
- RF-M05-03: gerar mensalidade mensal por jogador ativo elegível.
- RF-M05-04: permitir envio de comprovante pelo jogador.
- RF-M05-05: permitir aprovação, rejeição ou quitação manual pelo Dono/Moderador autorizado.
- RF-M05-05A: criar permissão de Tesoureiro do Time para gerenciar finanças, validar pagamentos, despesas e relatórios financeiros conforme delegação do Dono do Time.
- RF-M05-06: registrar entrada no caixa quando mensalidade for quitada.
- RF-M05-07: gerar cobrança N+1 somente quando N estiver paga ou quando exceção permitir.
- RF-M05-08: suportar isenção, hiato, voto de confiança, perdão de dívida e pausa de mensalidade.
- RF-M05-09: aplicar descontos por posição preferencial ou área do campo quando configurado.

Regras de negócio:
- RN-M05-01: comprovante enviado não significa pagamento confirmado.
- RN-M05-02: quitação cria lançamento de caixa imediato e auditável.
- RN-M05-03: jogador inadimplente não pode se inscrever em partida, salvo exceção registrada.
- RN-M05-04: voto de confiança permite jogar mesmo inadimplente apenas quando estiver ativo.
- RN-M05-04A: voto de confiança dura até a próxima mensalidade; na mensalidade seguinte, o jogador deve quitar a mensalidade atual e a anterior. Se não quitar as duas, fica impedido de jogar.
- RN-M05-05: isento gera mensalidade zerada e paga para manter fluxo de geração.
- RN-M05-06: desconto é calculado pela posição preferencial do perfil, não pela posição efetivamente jogada.
- RN-M05-07: perdão de dívida deve gerar ajuste financeiro auditável, não apagar cobrança.
- RN-M05-08: duração do perdão de dívida deve ser configurável pelo Dono do Time.
- RN-M05-09: pagamentos reais online ficam fora do MVP; por enquanto, o sistema opera somente por comprovante manual e quitação administrativa.

Critérios de aceite:
- Rejeitar comprovante mantém mensalidade pendente e registra motivo.
- Jogador com mensalidade vencida é bloqueado na inscrição.
- Isento mantém elegibilidade sem afetar cálculo de inadimplência.

### M06 - Despesas, Metas, Caixa e Transparência

Objetivo: controlar saúde financeira do time e permitir transparência configurável.

Requisitos funcionais:
- RF-M06-01: cadastrar despesa fixa, avulsa ou recorrente.
- RF-M06-02: configurar periodicidade semanal, mensal, bimestral, trimestral, semestral, anual ou bianual.
- RF-M06-03: gerar próxima despesa recorrente como não paga.
- RF-M06-04: marcar despesa como paga e lançar saída de caixa.
- RF-M06-05: cadastrar metas com valor alvo, prioridade, descrição, datas e status.
- RF-M06-06: distribuir superávit automaticamente entre metas conforme prioridade.
- RF-M06-07: cobrir déficit usando meta de menor prioridade conforme regra aprovada.
- RF-M06-08: gerar relatório de transparência por 1, 6 e 12 meses.
- RF-M06-09: projetar receitas e despesas futuras separando valores realizados e previstos.
- RF-M06-10: permitir que o Dono do Time configure quais dados do relatório de transparência ficam visíveis aos jogadores.

Regras de negócio:
- RN-M06-01: caixa é derivado de lançamentos, não editado diretamente sem ajuste auditável.
- RN-M06-02: receita cobre despesas antes de alimentar metas.
- RN-M06-03: déficit coberto por meta precisa registrar origem, destino e justificativa.
- RN-M06-04: relatório deve distinguir realizado, projetado e pendente.
- RN-M06-05: relatório de transparência só aparece se Dono do Time ativar.
- RN-M06-06: jogadores só podem ver os dados financeiros que o Dono do Time liberar explicitamente.

Critérios de aceite:
- Pagar despesa reduz caixa e atualiza relatório.
- Distribuição de superávit respeita prioridades.
- Jogador visualiza apenas transparência permitida, sem comprovantes ou dados pessoais de outros jogadores.

### M07 - Partidas, Inscrição e Presença

Objetivo: gerenciar jogos únicos e recorrentes, vagas, presença e histórico.

Requisitos funcionais:
- RF-M07-01: criar partida com data, horário, local, recorrência, periodicidade e limites.
- RF-M07-02: configurar número máximo de jogadores, times e jogadores por time.
- RF-M07-03: permitir inscrição e cancelamento pelo jogador elegível.
- RF-M07-04: bloquear inscrição quando mensalidade estiver pendente ou status impedir participação.
- RF-M07-05: confirmar presença por geolocalização em raio configurável por partida.
- RF-M07-06: permitir presença por QR Code no local.
- RF-M07-07: permitir presença manual por Dono/Moderador com justificativa.
- RF-M07-08: registrar faltas, atrasos e justificativas.
- RF-M07-09: ao encerrar partida recorrente, gerar próxima partida por job.
- RF-M07-10: enviar lembretes 24h e 2h antes.

Regras de negócio:
- RN-M07-01: presença geográfica é controle operacional, não prova antifraude absoluta.
- RN-M07-02: QR Code deve expirar e estar vinculado à partida.
- RN-M07-03: confirmação fecha no horário definido pelo time.
- RN-M07-04: justificativa removida da contagem punitiva não deve apagar histórico.
- RN-M07-05: partida recorrente só gera próxima se a atual foi encerrada ou cancelada corretamente.
- RN-M07-06: raio de presença não é fixo em 100 metros; deve ser configurável por partida.

Critérios de aceite:
- Jogador inadimplente não consegue se inscrever via tela nem API.
- Check-in fora do raio é negado, mas fallback por QR/manual pode ser usado conforme permissão.
- Ausência justificada deixa trilha de auditoria.

### M08 - Matchmaking e Balanceamento de Times

Objetivo: montar times justos, respeitando posições, preferências, histórico e escassez.

Requisitos funcionais:
- RF-M08-01: executar matchmaking após fechamento da confirmação de presença.
- RF-M08-02: calcular quantidade de times e vagas por posição.
- RF-M08-03: alocar por hierarquia: primária, secundária, terciária e improvisado.
- RF-M08-04: ordenar disputas por score de prioridade.
- RF-M08-05: aplicar proteção por escassez antes de penalizar especialistas.
- RF-M08-06: marcar jogador improvisado quando alocado fora das preferências.
- RF-M08-07: atualizar pontuação dinâmica após a partida.
- RF-M08-08: armazenar histórico de posição jogada nas últimas partidas.
- RF-M08-09: permitir reexecução por administrador com motivo auditado.
- RF-M08-10: exibir explicação resumida da escalação para reduzir conflitos.

Regras de negócio:
- RN-M08-01: score por posição varia de 1.0 a 6.0.
- RN-M08-02: se oferta primária <= demanda, jogador essencial não sofre penalidade na posição.
- RN-M08-03: penalidade só ocorre quando há concorrência real pela posição.
- RN-M08-04: improvisado ganha prioridade futura nas posições preferidas conforme regra de rotação.
- RN-M08-05: algoritmo precisa ser determinístico ou registrar semente quando usar aleatoriedade.
- RN-M08-06: nenhum time pode ser gerado sem validar vagas, presença e elegibilidade.
- RN-M08-07: em caso de empate, o algoritmo pode usar sorteio, desde que registre semente pública para auditoria.

Critérios de aceite:
- Especialista escasso não perde prioridade injustamente.
- Jogadores não selecionados para posição preferida ganham prioridade futura.
- Administrador consegue explicar por que um jogador foi improvisado.

### M09 - Notificações e Comunicação

Objetivo: centralizar alertas, convites, cobranças, lembretes e mensagens operacionais.

Requisitos funcionais:
- RF-M09-01: manter caixa in-app com status lida/não lida.
- RF-M09-02: enviar notificações por tipo: pagamento, partida, convite, sistema, transparência.
- RF-M09-03: suportar e-mail, push e in-app conforme configuração.
- RF-M09-04: disparar notificações de solicitação de entrada para Dono/Moderador.
- RF-M09-05: disparar lembretes automáticos de partida.
- RF-M09-06: notificar mensalistas em atraso.
- RF-M09-07: permitir opt-out de comunicações não essenciais.

Regras de negócio:
- RN-M09-01: notificações essenciais de segurança, pagamento e operação podem não permitir opt-out total.
- RN-M09-02: envio em massa deve ser assíncrono.
- RN-M09-03: falha no envio externo não pode travar a regra de negócio principal.
- RN-M09-04: caixa in-app é fonte persistente mesmo quando e-mail/push falhar.

Critérios de aceite:
- Mensagem lida atualiza contador em todas as sessões suportadas.
- Cobrança em massa não bloqueia resposta da API.
- Falha de provedor externo fica registrada para retentativa.

### M10 - Administração, Auditoria e Suporte

Objetivo: permitir operação da plataforma com segurança, rastreabilidade e suporte.

Requisitos funcionais:
- RF-M10-01: listar usuários, times, assinaturas e eventos auditáveis.
- RF-M10-02: bloquear/desbloquear usuários por violação.
- RF-M10-03: registrar logs de ações críticas.
- RF-M10-04: permitir suporte sem expor segredos ou comprovantes desnecessariamente.
- RF-M10-05: monitorar erros, performance e filas.

Regras de negócio:
- RN-M10-01: suporte deve operar por menor privilégio.
- RN-M10-02: ações de Super Admin exigem motivo.
- RN-M10-03: logs não devem armazenar senha, tokens, comprovantes sensíveis ou dados além do necessário.

Critérios de aceite:
- Toda ação administrativa crítica possui autor, data, escopo, motivo e resultado.
- Super Admin não acessa dados financeiros sensíveis sem trilha.

### M11 - Experiência PWA, Design e Acessibilidade

Objetivo: entregar interface rápida, mobile-first, acessível e coerente com futebol society.

Requisitos funcionais:
- RF-M11-01: PWA responsiva para celular e desktop.
- RF-M11-02: telas principais: dashboard do time, financeiro, partidas, presença, jogadores, convites, matchmaking e relatórios.
- RF-M11-03: estados obrigatórios: carregando, vazio, erro, sucesso, permissão negada e offline.
- RF-M11-04: identidade visual baseada em Pitch Green, Deep Stadium, Goal White, Warning Yellow, Penalty Red e Sky Blue.
- RF-M11-05: componentes com contraste adequado, labels, teclado e feedback claro.

Regras de UX:
- UX-M11-01: tarefas frequentes devem exigir poucos cliques: confirmar presença, pagar mensalidade, validar comprovante e gerar times.
- UX-M11-02: mensagens financeiras devem evitar constrangimento público.
- UX-M11-03: fluxo de convite deve funcionar bem em WhatsApp e QR Code.
- UX-M11-04: relatórios devem separar dados operacionais de dados sensíveis.

Critérios de aceite:
- Jogador confirma presença pelo celular sem depender de desktop.
- Moderador valida pagamento e presença em fluxo curto.
- Interface não expõe ações sem permissão.

## 6. Matriz de Permissões Inicial

| Ação | Super Admin | Dono do Time | Moderador | Jogador |
| --- | --- | --- | --- | --- |
| Gerenciar parâmetros globais | Sim | Não | Não | Não |
| Criar time | Conforme plano | Sim, se plano permitir | Não | Não |
| Editar time | Intervenção auditada | Sim | Parcial, se delegado | Não |
| Promover moderador | Intervenção auditada | Sim | Não | Não |
| Convidar jogador | Não usual | Sim | Sim | Não |
| Aceitar solicitação | Não usual | Sim | Sim | Não |
| Configurar mensalidade | Não usual | Sim | Somente Tesoureiro | Não |
| Enviar comprovante | Não | Não | Como jogador | Sim |
| Quitar/rejeitar mensalidade | Intervenção auditada | Sim | Somente Tesoureiro | Não |
| Criar partida | Não usual | Sim | Sim | Não |
| Confirmar presença própria | Não | Como jogador | Como jogador | Sim |
| Confirmar presença de terceiros | Intervenção auditada | Sim | Sim | Não |
| Gerar matchmaking | Não usual | Sim | Sim | Não |
| Ver relatório de transparência | Não usual | Sim | Sim | Apenas dados liberados |

## 7. Principais Problemas Encontrados

### 7.1 Estruturais

- Os documentos misturam proposta comercial, requisitos, arquitetura, prompts de código e identidade visual sem separação clara.
- Há duplicidade e conflito de papéis: Admin, Master, Owner, Dono do Time, Admin/Dev e Super Admin.
- O escopo é grande demais para um primeiro release sem priorização por MVP.
- Alguns requisitos descrevem solução antes de fechar regra de negócio, especialmente em autenticação, notificações e matchmaking.
- O documento resumido fala em limite inicial de 1 time e 40 jogadores, enquanto o documento bruto já descreve SaaS multi-time.

Solução: separar papéis globais e por time, modularizar o produto, definir MVP, evolução e regras de decisão.

### 7.2 Arquitetura

- Há conflito entre .NET 8 e .NET 10. Como o projeto é novo em 2026, a escolha recomendada é .NET 10, desde que o time valide disponibilidade de SDK, hospedagem e bibliotecas.
- ASP.NET Core Identity, Firebase Auth e Supabase Auth aparecem como alternativas, mas não podem ser usadas simultaneamente sem desenho de identidade claro.
- SignalR em instância única no Cloud Run é gargalo de escala. Funciona como experimento, mas não como arquitetura SaaS madura.
- Notificações em massa e jobs recorrentes precisam de fila ou tabela de outbox; envio síncrono por loop é risco.
- Upload de comprovantes aumenta custo e risco de privacidade; deve ter limite, retenção e armazenamento controlado.
- Matchmaking exige auditabilidade e testes de propriedades, não apenas implementação direta.

Solução: usar arquitetura modular, auth externo no MVP com autorização própria por time, fila/outbox, livro-caixa imutável, observabilidade e plano de migração de real-time.

### 7.3 Usabilidade

- Expor tags financeiras publicamente geraria risco de constrangimento; a decisão do Dono do Negócio é restringir a administradores autorizados e ao próprio jogador.
- Fluxo de geolocalização pode falhar por GPS, permissão negada ou aparelho ruim; precisa fallback.
- Validar pagamento, presença e convite devem ser fluxos rápidos no celular.
- Relatório financeiro pode confundir realizado e projetado se não separar claramente.
- Matchmaking precisa de explicação amigável para reduzir discussão entre jogadores.

Solução: UX mobile-first, mensagens claras, fallback por QR/manual, privacidade financeira e explicações resumidas do algoritmo.

### 7.4 Segurança e LGPD

- Geolocalização do cliente pode ser burlada; deve ser tratada como controle operacional, não como prova forte.
- Comprovantes podem conter dados pessoais e bancários.
- Tags como "Dívida Perdoada" e ranking de mensalista são dados sensíveis no contexto social do grupo.
- Permissões por time são críticas para evitar vazamento entre times.
- Logs não podem registrar senha, token, comprovante ou dados bancários completos.
- Convites por link/QR Code precisam expiração, revogação e escopo.

Solução: aplicar OWASP ASVS como referência de segurança, LGPD por minimização/finalidade, RBAC por time no backend, auditoria e controle de retenção.

## 8. Stack e Soluções Gratuitas Recomendadas

### 8.1 MVP de Custo Quase Zero

| Camada | Solução recomendada | Motivo | Atenção |
| --- | --- | --- | --- |
| Frontend | React + Vite + TypeScript PWA no Firebase Hosting | hospedagem estática grátis com SSL e domínio customizado | limite de hosting e transferência; monitorar uso |
| Backend | .NET 10 Web API no Google Cloud Run | paga por uso, free tier mensal e escala automática | precisa billing; cold start; custos se tráfego crescer |
| Banco | Supabase PostgreSQL + PostGIS | Postgres gerenciado e camada gratuita | free plan tem limite de banco e pausa por inatividade |
| Auth | Supabase Auth ou Firebase Auth | reduz custo e complexidade de identidade | RBAC por time deve ficar no backend |
| Arquivos | Supabase Storage ou Firebase Storage | simples para comprovantes e logos | limitar tamanho, retenção e acesso |
| Jobs | Cloud Scheduler + endpoint seguro ou Supabase cron/pg_cron quando disponível | baixo custo para mensalidades e recorrências | idempotência obrigatória |
| Fila/outbox | tabela `OutboxMessages` no Postgres no MVP | evita custo de broker no início | migrar para Pub/Sub/RabbitMQ quando volume crescer |
| Observabilidade | OpenTelemetry + logs Cloud Run + Sentry free | rastreabilidade básica | evitar dados sensíveis |
| CI/CD | GitHub Actions free para repositório elegível | build/test/deploy automatizado | proteger secrets |
| Segurança | OWASP ZAP Baseline, Semgrep community, Gitleaks | ferramentas gratuitas | não substitui revisão humana |
| Testes | xUnit, FluentAssertions, Testcontainers, Playwright | cobertura backend e E2E | Testcontainers pode exigir Docker local |

### 8.2 Gatilhos de Escala

- Banco acima de 70% do limite gratuito: revisar retenção, índices e migrar para Pro/Neon/Railway/self-host.
- Uploads de comprovantes crescendo: aplicar compressão, expiração e storage pago previsível.
- Mais de 3 times pagantes ou mais de 150 jogadores ativos: sair da dependência de free tier como base do negócio.
- Real-time com múltiplas instâncias: usar Azure SignalR, Redis backplane ou trocar para SSE/polling por caso.
- Notificações em massa recorrentes: migrar outbox para broker gerenciado.
- Matchmaking lento ou contestado: criar benchmark, explicar decisão e registrar auditoria por partida.

## 9. Requisitos Não Funcionais

Performance:
- APIs de leitura simples devem responder em até 500 ms no p95 em ambiente de produção inicial.
- Operações pesadas devem responder com aceite da tarefa e processamento assíncrono.
- Listas devem ser paginadas.

Disponibilidade:
- MVP deve aceitar cold start, mas fluxos críticos de partida precisam degradação segura.
- Para jogos marcados, presença deve funcionar com fallback manual.

Segurança:
- Autorização no backend em todas as operações protegidas.
- Proteção contra IDOR entre times.
- Senhas e tokens nunca em logs.
- Convites com expiração e revogação.
- Testes de acesso negado por perfil.

Privacidade:
- Dados pessoais mínimos.
- Comprovantes com acesso restrito.
- Retenção configurável para comprovantes.
- Tags financeiras visíveis somente para administradores autorizados e para o próprio jogador.

Auditabilidade:
- Livro-caixa imutável.
- Auditoria de permissões, pagamentos, presença manual, matchmaking reexecutado e intervenção de Super Admin.

Escalabilidade:
- Domínios desacoplados.
- Outbox para eventos.
- Índices em consultas de time, jogador, mensalidade, partida e presença.
- Limites por plano.

Acessibilidade:
- Contraste adequado.
- Navegação por teclado.
- Labels em inputs.
- Mensagens de erro claras.
- Layout mobile-first.

## 10. Dados Principais

Entidades recomendadas:
- User
- Team
- TeamMember
- TeamRole
- Invitation
- JoinRequest
- SubscriptionPlan
- SubscriptionAssignment
- PlayerProfile
- PlayerPositionPreference
- Match
- MatchRegistration
- MatchAttendance
- MatchmakingRun
- MatchmakingAssignment
- MonthlyFee
- PaymentProof
- Expense
- Goal
- CashLedgerEntry
- Notification
- NotificationRecipientStatus
- AuditLog
- OutboxMessage

Constraints essenciais:
- e-mail único por usuário.
- um vínculo ativo por usuário/time.
- papel por time único por usuário/papel.
- mensalidade única por jogador/time/mês.
- partida recorrente com chave de idempotência para evitar duplicidade.
- lançamento de caixa referenciando origem.
- convite com token único e expiração.

Índices iniciais:
- TeamMember(teamId, userId, status)
- MonthlyFee(teamId, playerId, period, status)
- Match(teamId, startsAt, status)
- MatchRegistration(matchId, playerId)
- CashLedgerEntry(teamId, occurredAt)
- NotificationRecipientStatus(userId, isRead, createdAt)
- AuditLog(scopeType, scopeId, createdAt)

## 11. Backlog Priorizado

### Release 0 - Fundação e PoC

- Definir repositórios, arquitetura, CI, ambientes e modelo de dados inicial.
- Implementar auth e autorização por time.
- Criar time, usuário, vínculo e papéis.
- Criar livro-caixa básico e mensalidade manual.
- Provar algoritmo de matchmaking com testes de propriedades e casos de escassez.

### Release 1 - MVP Operacional com Prioridade Financeira

- PWA com dashboard do time.
- Cadastro de jogadores e convites.
- Mensalidades com comprovante, aprovação/rejeição, voto de confiança, tesoureiro e caixa.
- Despesas simples e relatório básico com dados liberáveis pelo Dono do Time.
- Partidas, inscrição e presença manual/QR em segundo bloco do MVP.
- Notificações in-app e e-mail básico.

### Release 2 - Futebol e Transparência

- Matchmaking completo.
- Geolocalização com fallback.
- Partidas recorrentes e jobs.
- Metas, superávit, déficit e relatório de transparência.
- Histórico de faltas, atrasos e justificativas.

### Release 3 - SaaS e Monetização

- Planos e limites automáticos.
- Pagamento de assinatura.
- Admin da plataforma.
- Métricas de uso, churn e receita.
- Relatórios avançados e exportações.

## 12. Plano de Rentabilidade

Modelo recomendado: freemium com limite real de valor.

Preço sugerido inicial no Brasil:
- Gratuito: 1 time, até 25 ou 40 jogadores, recursos básicos e limite de histórico.
- Initial: R$ 19,90 a R$ 29,90 por mês por time.
- Pro: R$ 49,90 a R$ 79,90 por mês, até 3 times ou mais jogadores.
- Smart: R$ 149,90+ por organização, com suporte e customizações leves.

Fontes de receita:
- Assinatura por time.
- Recursos premium: relatórios avançados, histórico expandido, automações, múltiplos administradores.
- Cobrança por armazenamento extra de comprovantes/logos.
- Parcerias com arenas, quadras e escolinhas.
- White label para organizadores com vários times.

Estratégia de aquisição:
- Começar com times reais do círculo próximo para validação.
- Oferecer migração gratuita de planilha/WhatsApp para os primeiros grupos.
- Criar landing page com simulador de economia de tempo.
- Vender para donos de campo como ferramenta para fidelizar grupos.

Métricas de negócio:
- Times ativos semanais.
- Jogadores ativos por time.
- Mensalidades geradas e quitadas.
- Partidas criadas e presenças confirmadas.
- Tempo médio para validar pagamento.
- Churn de times.
- Conversão gratuito para pago.

Gatilho para cobrar:
- Quando o time usar financeiro + partidas por 4 semanas e tiver pelo menos 20 jogadores ativos, o produto já gerou valor suficiente para oferta de plano pago.

## 13. Gargalos e Mitigações

| Gargalo | Risco | Mitigação |
| --- | --- | --- |
| Free tier do banco | read-only, pausa, limite de dados | monitorar uso, limitar comprovantes, migrar ao atingir 70% |
| SignalR em instância única | não escala e pode perder conexão | MVP com in-app/SSE; escala com Redis/Azure SignalR |
| Upload de comprovantes | custo e LGPD | limite de tamanho, retenção, acesso restrito |
| Geolocalização | spoofing e falha de permissão | tratar como controle operacional, usar QR/manual |
| Matchmaking | conflito social por escalação | auditoria, explicação, testes e possibilidade de reexecução justificada |
| Financeiro | inconsistência de caixa | livro-caixa imutável, ajustes auditáveis |
| Papéis por time | vazamento entre times | autorização por escopo no backend e testes de IDOR |
| Jobs recorrentes | duplicidade de mensalidade/partida | idempotência por chave e logs |

## 14. Decisões Respondidas pelo Dono do Negócio

- Tags financeiras: visíveis apenas para administradores autorizados e para o próprio jogador.
- Voto de confiança: permite jogar mesmo inadimplente apenas quando ativo; dura até a próxima mensalidade. Na mensalidade seguinte, o jogador deve quitar a mensalidade atual e a anterior; se não quitar as duas, não joga.
- Perdão de dívida: duração configurável pelo Dono do Time.
- Tesoureiro do Time: criar permissão específica para gerenciar finanças. O Dono do Time pode atribuir essa permissão a moderador ou usuário autorizado.
- Raio de busca de times: configurável.
- Raio de presença: configurável por partida.
- Pagamentos reais: fora do MVP; por enquanto, apenas comprovante manual e quitação administrativa.
- Relatório de transparência: jogadores veem apenas os dados que o Dono do Time liberar.
- Matchmaking em empate: sorteio permitido, com registro de semente pública.
- Prioridade do MVP: financeiro antes de partidas/matchmaking.

## 15. Critérios de Aceite Gerais

- Todo módulo protegido deve validar permissão no backend.
- Nenhuma lista potencialmente grande pode ser entregue sem paginação.
- Nenhuma ação financeira pode alterar caixa sem lançamento auditável.
- Nenhum comprovante deve ser público.
- Nenhuma tag financeira sensível deve aparecer para outros jogadores; visibilidade permitida apenas para administradores autorizados e para o próprio jogador.
- Jogador inadimplente deve ser bloqueado na inscrição, salvo exceção registrada.
- Jogador em voto de confiança deve ser bloqueado se não quitar a mensalidade atual e a anterior na próxima cobrança.
- Matchmaking deve passar nos casos de escassez, concorrência, improviso e empate.
- Sorteio por empate no matchmaking deve registrar semente pública.
- Presença deve aceitar fallback quando geolocalização falhar.
- Todo job recorrente deve ser idempotente.
- Toda tela deve ter estados de loading, vazio, erro, sucesso e permissão negada.

## 16. Estratégia de Implementação

Fase 1: validar domínio e arquitetura
- Criar diagrama de domínio.
- Definir contratos de API por módulo.
- Provar auth e autorização por time.
- Provar livro-caixa e mensalidade.
- Provar matchmaking em biblioteca isolada.

Fase 2: construir MVP
- Implementar cadastro, time, vínculos e papéis.
- Implementar financeiro básico como prioridade do MVP.
- Implementar partidas e presença manual/QR após o bloco financeiro essencial.
- Implementar notificações in-app.
- Implementar PWA mobile-first.

Fase 3: endurecer para produção
- Adicionar testes E2E.
- Adicionar observabilidade.
- Adicionar segurança automatizada.
- Revisar LGPD e retenção.
- Configurar deploy e backups.

Fase 4: monetizar e escalar
- Implementar planos, limites e pagamentos.
- Ativar métricas de produto.
- Ajustar custos.
- Expandir relatórios e automações.

## 17. Fontes Oficiais Consultadas

- Google Cloud Run pricing: https://cloud.google.com/run/pricing
- Firebase pricing and Hosting quotas: https://firebase.google.com/pricing
- Supabase pricing: https://supabase.com/docs/pricing
- Supabase database size behavior: https://supabase.com/docs/guides/platform/database-size
- ASP.NET Core Minimal APIs .NET 10: https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis?view=aspnetcore-10.0
- ASP.NET Core SignalR scale: https://learn.microsoft.com/aspnet/core/signalr/scale?view=aspnetcore-10.0
- OWASP ASVS: https://owasp.org/www-project-application-security-verification-standard/
- LGPD - Lei 13.709/2018: https://www.planalto.gov.br/ccivil_03/_ato2015-2018/2018/lei/l13709.htm
