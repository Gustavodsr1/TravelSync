# TravelSync - Blueprint de Estudo (Microservicos + Vertical Slice)

## 1. Objetivo do projeto
Projeto para estudo pratico de arquitetura em microservicos com Vertical Slice Architecture usando:
- .NET 10 (preview no momento, validar SDK instalado)
- ASP.NET Core
- EF Core (SQL Server)
- MediatR
- FluentValidation
- Carter (Minimal APIs organizadas)

Dominio: planejamento de viagens entre amigos.
Fluxo principal:
1. Usuario cria conta (email, senha, CPF)
2. Usuario cria uma viagem
3. Usuario convida amigos por email
4. Convidados aceitam convite
5. Apenas dono da viagem e convidados aceitos podem acessar detalhes daquela viagem

---

## 2. Requisitos funcionais (escopo inicial)

### 2.1 Cadastro e autenticacao
- Registrar usuario com:
  - Email (unico)
  - Senha (hash)
  - CPF (chave forte de negocio, unico)
- Login com emissao de JWT

### 2.2 Gestao de viagem
- Criar viagem com:
  - Nome da trip
  - Data ida
  - Data volta
  - Lista de destinos (paises/cidades)
- Listar viagens do usuario
- Obter detalhes de uma viagem
- Atualizar e cancelar viagem

### 2.3 Convites
- Dono da viagem convida por email
- Endpoint para enviar email de confirmacao de convite
- Convidado aceita ou recusa convite
- Somente convidados aceitos visualizam a viagem

### 2.4 Destinos
- Endpoint de listagem de todos os paises
- Endpoint de listagem de todas as cidades do Brasil
- Endpoint de pesquisa de destinos para autocomplete (opcional inicial)

### 2.5 Integracao com Google Maps
- Geocoding (nome do local -> coordenadas)
- Place details (dados complementares do destino)
- Opcional fase 2: distancia estimada entre pontos

---

## 3. Arquitetura proposta (microservicos)

## 3.1 Servicos
1. Gateway (BFF/API Gateway)
- Entrada unica da aplicacao
- Roteamento, autenticacao, rate limit e agregacao simples

2. Identity API
- Registro/login
- Emissao e validacao de JWT
- Gestao de usuario (CPF como chave de negocio)

3. Trip API
- Viagens
- Membros da viagem
- Regras de permissao por viagem

4. Invitation API
- Convites
- Aceite/recusa
- Estados do convite

5. Location API
- Catalogo de paises e cidades do Brasil
- Integracao Google Maps

6. Notification Worker
- Envio assicrono de emails
- Processa eventos (convite criado, convite aceito)

7. Shared libs (apenas contratos e cross-cutting essencial)
- TravelSync.SharedKernel: primitives, Result, errors base
- TravelSync.Messaging: contratos de eventos e envelope

Observacao de estudo:
- Evitar acoplamento forte entre microservicos.
- Compartilhar somente contratos e utilitarios minimos.

---

## 4. Vertical Slice dentro de cada microservico
Cada feature organizada por fatia vertical, nao por camada tecnica global.

Exemplo de estrutura por servico (Trip API):
- Features/
  - CreateTrip/
    - Endpoint.cs (Carter)
    - Command.cs
    - Validator.cs (FluentValidation)
    - Handler.cs (MediatR)
    - Mapping.cs
  - GetTripById/
  - ListMyTrips/
  - InviteToTrip/ (ou delegar para Invitation)
- Infrastructure/
  - Persistence/
    - DbContext
    - Configurations (EF Core)
- Program.cs

Padrao de request flow:
Carter endpoint -> MediatR command/query -> Validator -> Handler -> EF Core -> resposta.

---

## 5. Banco de dados (SQL Server + EF Core)

## 5.1 Estrategia
- Um banco por microservico (recomendado para estudo de microservicos)
  - TravelSyncIdentityDb
  - TravelSyncTripDb
  - TravelSyncInvitationDb
  - TravelSyncLocationDb
- Migrations separadas por servico

## 5.2 Modelagem inicial (alto nivel)

Identity:
- Users
  - UserId (GUID tecnico)
  - Cpf (string, unico, chave forte de negocio)
  - Email (unico)
  - PasswordHash
  - FullName
  - CreatedAt

Trip:
- Trips
  - TripId
  - OwnerUserId
  - Name
  - StartDate
  - EndDate
  - Status
  - CreatedAt
- TripMembers
  - TripMemberId
  - TripId
  - UserId
  - Role (Owner, Guest)
  - MembershipStatus (Pending, Accepted, Rejected)
- TripDestinations
  - TripDestinationId
  - TripId
  - Country
  - City
  - Latitude
  - Longitude
  - VisitOrder

Invitation:
- Invitations
  - InvitationId
  - TripId
  - InviterUserId
  - InviteeEmail
  - Token
  - ExpiresAt
  - Status (Pending, Accepted, Rejected, Expired)
  - CreatedAt

Location:
- Countries
  - Code
  - Name
- BrazilCities
  - IbgeCode
  - Name
  - StateCode
  - Latitude
  - Longitude

---

## 6. Regras de permissao (importante)

Regra central:
Usuario autenticado so acessa viagem se:
- For owner da viagem, ou
- Estiver como membro com status Accepted

Casos de autorizacao:
- Criar viagem: qualquer usuario autenticado
- Editar/cancelar viagem: somente owner
- Convidar pessoas: somente owner
- Ver viagem: owner ou member accepted
- Listar membros da viagem: owner ou member accepted

Tecnica sugerida:
- Policy-based authorization no servico Trip
- Revalidar permissao no handler (defesa em profundidade)
- Nao confiar apenas no gateway

---

## 7. Contratos de API (escopo inicial)

## 7.1 Identity API
- POST /auth/register
- POST /auth/login
- GET /users/me

## 7.2 Trip API
- POST /trips
- GET /trips
- GET /trips/{tripId}
- PUT /trips/{tripId}
- DELETE /trips/{tripId}
- GET /trips/{tripId}/members

## 7.3 Invitation API
- POST /invitations
- POST /invitations/{invitationId}/send-email
- POST /invitations/{token}/accept
- POST /invitations/{token}/reject
- GET /trips/{tripId}/invitations

## 7.4 Location API
- GET /locations/countries
- GET /locations/brazil/cities
- GET /locations/search?term=
- GET /locations/geocode?query=

Observacao:
Voce pediu endpoint de listagem de destinos (todos os paises) e tambem endpoint de paises. Isso pode ser o mesmo endpoint (`/locations/countries`) para evitar duplicidade.

---

## 8. Mensageria e eventos (recomendado para estudo)
Mesmo em estudo, ja vale praticar eventos de dominio/integracao:
- InvitationCreated -> Notification Worker envia email
- InvitationAccepted -> Trip API atualiza membership
- TripCreated -> auditoria/log

Opcao simples para inicio:
- Outbox pattern + processamento interno
Opcao fase 2:
- Broker (RabbitMQ ou Azure Service Bus)

---

## 9. Observabilidade e qualidade

Minimo recomendado:
- Logging estruturado (Serilog)
- CorrelationId por request
- Health checks por servico
- Swagger/OpenAPI em todos os servicos
- Testes:
  - Unitarios para handlers e validadores
  - Integracao para fluxos principais (registro, criar viagem, convite, aceite)

---

## 10. Seguranca
- Senha com hash forte (ASP.NET Identity password hasher ou BCrypt)
- JWT com expiracao curta + refresh token (fase 2)
- CPF mascarado em logs
- Validacoes de input com FluentValidation
- Rate limit em endpoints sensiveis (login, convite)

---

## 11. Sugestao de roadmap (cronograma de implementacao)

## Sprint 0 (1 semana) - Fundacao
- Definir convencoes de projeto
- Subir solucao com microservicos vazios
- Configurar SQL Server, conexoes e migrations base
- Configurar Carter, MediatR, FluentValidation em todos
- Definir contrato de autenticacao (JWT)

Entregavel:
- Solucao compilando com servicos no ar e health checks

Prompt de implementacao:
"Implemente a Sprint 0 no monorepo TravelSync com .NET 10, criando a base de todos os microservicos (Gateway, Identity, Trip, Invitation, Location e Notification Worker), configurando Carter, MediatR e FluentValidation em cada API, SQL Server + EF Core com DbContext e migration inicial por servico, health checks e OpenAPI. Entregue estrutura compilando, padroes de pastas em Vertical Slice e README curto com comandos para subir localmente."

## Sprint 1 (1 semana) - Identity
- Register/Login
- Persistencia de usuario com CPF unico
- JWT funcional
- Testes unitarios basicos

Entregavel:
- Usuario consegue criar conta e autenticar

Prompt de implementacao:
"Implemente a Sprint 1 no servico Identity API com cadastro e login usando email, senha e CPF unico, persistencia no SQL Server via EF Core, validacoes com FluentValidation, endpoints Carter, handlers MediatR e emissao de JWT. Garanta hash de senha seguro, testes unitarios essenciais e contratos de resposta padronizados."

## Sprint 2 (1 a 2 semanas) - Trip Core
- Criacao/listagem/detalhe de viagem
- Regras de owner
- Destinos basicos na viagem
- Testes de regra de negocio

Entregavel:
- Usuario autenticado cria e consulta viagens

Prompt de implementacao:
"Implemente a Sprint 2 no servico Trip API, criando features de criar, listar e detalhar viagens com destinos, aplicando Vertical Slice completo (Endpoint, Command/Query, Validator, Handler), persistencia EF Core e regras de owner. Inclua testes de regra de negocio e documentacao OpenAPI dos endpoints."

## Sprint 3 (1 semana) - Convites e permissao fina
- Criar convite
- Aceite/recusa por token
- Vincular convidado como membro accepted
- Bloqueio de acesso para nao membros

Entregavel:
- Fluxo completo de convidar e controlar acesso

Prompt de implementacao:
"Implemente a Sprint 3 no servico Invitation API e integracao com Trip API para fluxo de convites: criar convite, aceitar/recusar por token e atualizar membro da viagem para accepted. Aplique regra de autorizacao para bloquear acesso de nao convidados e adicione testes de integracao do fluxo ponta a ponta de permissao."

## Sprint 4 (1 semana) - Location + Google Maps
- Catalogo de paises
- Cidades do Brasil
- Geocode via Google Maps
- Cache simples de destinos

Entregavel:
- Endpoints de localizacao estaveis

Prompt de implementacao:
"Implemente a Sprint 4 no servico Location API com endpoints para listar todos os paises, cidades do Brasil e geocode via Google Maps. Use cache simples para reduzir chamadas externas, validacoes de entrada e contratos de resposta consistentes. Documente configuracao da API key e limites de uso."

## Sprint 5 (1 semana) - Notification + qualidade
- Worker para envio de email
- Evento InvitationCreated
- Logs estruturados, tracing basico
- Integracao tests ponta a ponta

Entregavel:
- Convite por email funcionando de forma assincrona

Prompt de implementacao:
"Implemente a Sprint 5 adicionando mensageria de evento InvitationCreated e processamento assincrono no Notification Worker para envio de email de convite. Configure logs estruturados, correlation id, retries basicos e testes de integracao cobrindo publicacao e consumo do evento."

## Sprint 6 (opcional) - Hardening
- Idempotencia
- Outbox robusto
- Retry policies
- Observabilidade avancada (OpenTelemetry)

Prompt de implementacao:
"Implemente a Sprint 6 com foco em robustez: idempotencia para operacoes criticas, Outbox pattern confiavel, politicas de retry/circuit breaker e telemetria com OpenTelemetry (traces e metricas). Entregue dashboards e checklist de resiliencia por servico."

## Sprint 7 (pos-backend, 1 a 2 semanas) - Frontend Flutter
- Tarefa de gestao do frontend apos backend concluido
- Definir arquitetura Flutter (camadas, estado, client HTTP)
- Implementar autenticacao (login/cadastro) integrada ao Identity
- Implementar fluxo principal: criar viagem, convidar amigos, aceitar convite
- Implementar telas de destinos (paises e cidades do Brasil)
- Integrar mapa (Google Maps Flutter) para visualizar destinos
- Configurar monitoramento de erros no app (ex.: Crashlytics)

Entregavel:
- App Flutter MVP consumindo os endpoints do backend com fluxo ponta a ponta

Prompt de implementacao:
"Implemente a Sprint 7 criando o app Flutter MVP integrado ao backend TravelSync, com arquitetura em camadas, gerenciamento de estado, client HTTP autenticado por JWT, telas de login/cadastro, criacao de viagem, convite, aceite e visualizacao de destinos com Google Maps. Inclua tratamento de erro e monitoramento de crash."

## Sprint 8 (1 semana) - Orcamento da viagem
- Planejar e implementar controle de gastos previstos x realizados por viagem
- Registrar despesas por categoria (hospedagem, transporte, alimentacao, passeios)
- Exibir saldo consolidado e percentual de estouro de orcamento

Entregavel:
- Modulo financeiro basico da viagem com endpoints e tela Flutter de acompanhamento

Prompt de implementacao:
"Implemente a Sprint 8 com modulo de orcamento da viagem: cadastro de custos previstos e realizados por categoria, calculo de saldo e percentual de variacao, endpoints no backend e telas Flutter de acompanhamento financeiro. Garanta validacoes de valores e autorizacao por membro da viagem."

## Sprint 9 (1 semana) - Checklist colaborativo
- Criar checklist por viagem (documentos e preparacao)
- Permitir marcar itens como concluidos por participantes autorizados
- Incluir templates iniciais (passaporte, visto, seguro, vacinas)

Entregavel:
- Checklist colaborativo funcional por viagem

Prompt de implementacao:
"Implemente a Sprint 9 com checklist colaborativo por viagem, incluindo templates iniciais de preparacao, marcacao de itens por participantes autorizados e historico simples de alteracoes. Entregue endpoints completos e interface Flutter para uso colaborativo."

## Sprint 10 (1 semana) - Divisao de custos
- Registrar quem pagou e para quem dividir
- Calcular saldo por participante (quem deve e quem recebe)
- Gerar resumo de acerto de contas

Entregavel:
- Fechamento financeiro por participante para cada viagem

Prompt de implementacao:
"Implemente a Sprint 10 com divisao de custos entre participantes: registro de despesas, rateio por membros, calculo de quem deve/recebe e resumo final de acerto de contas. Inclua regras de arredondamento, consistencia financeira e visao no app Flutter."

## Sprint 11 (1 semana) - Votacao de destinos e datas
- Criar enquetes para destinos e periodos da viagem
- Permitir voto unico por participante convidado aceito
- Exibir resultado consolidado da votacao

Entregavel:
- Modulo de votacao com resultado auditavel

Prompt de implementacao:
"Implemente a Sprint 11 com votacao de destinos e datas por participantes aceitos, voto unico por enquete e apuracao consolidada auditavel. Entregue endpoints, validacoes de janela de votacao e telas Flutter para votar e consultar resultados."

## Sprint 12 (1 semana) - Timeline de atividades
- Criar agenda por dia da viagem com atividades
- Permitir ordenar atividades por horario/prioridade
- Notificar participantes sobre alteracoes relevantes

Entregavel:
- Roteiro diario completo da viagem

Prompt de implementacao:
"Implemente a Sprint 12 com timeline diaria de atividades da viagem, permitindo criar, editar, ordenar por horario/prioridade e notificar participantes sobre mudancas relevantes. Entregue backend + Flutter com visao de agenda por dia."

## Sprint 13 (1 semana) - Notificacoes multicanal
- Manter email e adicionar notificacoes por WhatsApp
- Definir estrategia de templates e retries por canal
- Implementar opt-in/opt-out por usuario

Entregavel:
- Convites e alertas enviados por email e WhatsApp

Prompt de implementacao:
"Implemente a Sprint 13 para notificacoes multicanal mantendo email e adicionando WhatsApp, com templates por tipo de mensagem, retries por canal e preferencias de opt-in/opt-out por usuario. Entregue fluxo de envio com rastreabilidade de status por notificacao."

## Sprint 14 (1 semana) - Modo offline no Flutter
- Cache local de viagens, destinos e checklist
- Fila de sincronizacao quando voltar conexao
- Tratar conflitos de sincronizacao com regra simples (last write wins inicial)

Entregavel:
- Experiencia minima offline-first no app

Prompt de implementacao:
"Implemente a Sprint 14 com modo offline no Flutter: cache local de viagens/destinos/checklist, fila de sincronizacao ao reconectar e estrategia inicial de resolucao de conflito (last write wins). Garanta feedback visual do estado de sync para o usuario."

## Sprint 15 (1 semana) - Exportacao PDF
- Exportar roteiro completo da viagem em PDF
- Incluir participantes, destinos, atividades e resumo financeiro
- Disponibilizar download no backend e compartilhamento no app

Entregavel:
- Geracao e compartilhamento de PDF da viagem

Prompt de implementacao:
"Implemente a Sprint 15 com exportacao de roteiro em PDF contendo participantes, destinos, timeline e resumo financeiro. Disponibilize endpoint de geracao/download no backend e compartilhamento no app Flutter com layout legivel e padronizado."

## Sprint 16 (1 semana) - Integracao com clima
- Consumir API de previsao do tempo por destino
- Exibir previsao por periodo da viagem
- Incluir alertas climaticos basicos

Entregavel:
- Painel de clima integrado ao planejamento

Prompt de implementacao:
"Implemente a Sprint 16 integrando API de clima por destino e periodo da viagem, com exibicao de previsao no backend/frontend e alertas climaticos basicos para apoiar o planejamento. Inclua cache e fallback para indisponibilidade do provedor externo."

## Sprint 17 (1 semana) - Recomendacoes turisticas
- Integrar fonte de pontos turisticos por destino
- Sugerir atracoes por perfil/interesse (fase inicial por tags)
- Permitir adicionar recomendacoes ao roteiro da viagem

Entregavel:
- Sistema de recomendacoes integrado ao planejamento

Prompt de implementacao:
"Implemente a Sprint 17 com recomendacoes turisticas por destino, usando integracao externa e classificacao inicial por tags/interesses. Permita adicionar sugestoes diretamente ao roteiro da viagem e acompanhar origem da recomendacao."

---

## 12. Expansoes oficialmente integradas
As sugestoes extras agora fazem parte do roadmap oficial nas Sprints 8 a 17:
1. Orcamento da viagem (gastos previstos x realizados)
2. Checklist colaborativo (passaporte, vistos, seguro)
3. Divisao de custos entre participantes
4. Votacao de destinos e datas
5. Timeline da viagem com atividades por dia
6. Notificacoes por WhatsApp alem de email
7. Modo offline no frontend com sync posterior
8. Exportar roteiro em PDF
9. Integracao com clima (weather API)
10. Recomendacoes de pontos turisticos por destino

---

## 13. Definicoes de estudo (DoD por feature)
Uma feature so termina quando tiver:
- Endpoint Carter
- Command/Query + Handler (MediatR)
- Validator (FluentValidation)
- Persistencia EF Core com migration
- Testes minimos
- Documentacao do endpoint no OpenAPI

---

## 14. Riscos e atencoes
- .NET 10 pode exigir SDK preview e ajustes de pacote
- Complexidade de microservicos pode crescer rapido para estudo individual
- Integracao Google Maps envolve custo e chave segura
- CPF como chave forte de negocio exige muito cuidado com LGPD

Mitigacao:
- Comecar simples, evoluir por sprint
- Manter limites claros entre servicos
- Automatizar testes cedo

---

## 15. Proximo passo recomendado (sem codar ainda)
1. Validar este blueprint
2. Congelar contratos minimos de API
3. Definir backlog da Sprint 0 em tarefas pequenas
4. So depois iniciar scaffolding e implementacao
5. Apos concluir o backend, iniciar Sprint 7 para gestao e entrega do frontend Flutter
6. Seguir Sprints 8 a 17 para implementar todas as expansoes integradas

Se quiser, no proximo passo eu transformo este blueprint em um backlog tecnico detalhado (issues por sprint, com criterio de aceite e estimativa).