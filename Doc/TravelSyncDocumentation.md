# TravelSync Documentation

## Objetivo do projeto
O TravelSync e um projeto de estudo para praticar arquitetura de microservicos com Vertical Slice usando .NET 10, SQL Server, EF Core, MediatR, FluentValidation e Carter.

A proposta funcional do sistema e permitir que um usuario:
- Crie conta com email, senha e CPF (CPF como chave forte de negocio).
- Crie viagens com nome, periodo e destinos.
- Convide amigos por email para participar da viagem.
- Controle acesso para que apenas owner e convidados aceitos visualizem os dados da viagem.

## Objetivos de arquitetura
- Aplicar Vertical Slice por feature dentro de cada microservico.
- Manter baixo acoplamento entre servicos.
- Separar responsabilidades por dominio (Identity, Trip, Invitation, Location, Notification).
- Evoluir o sistema com mensageria, resiliencia e observabilidade.

## Objetivos tecnicos de aprendizado
- Praticar desenho de APIs com Carter + MediatR.
- Praticar validacao de comandos com FluentValidation.
- Praticar persistencia com EF Core e SQL Server por servico.
- Praticar autorizacao por recurso (owner/convidado aceito).
- Praticar integracoes externas (Google Maps, clima, notificacoes).
- Praticar extensao para app Flutter consumindo backend distribuido.

## Escopo macro
- Backend em microservicos.
- Worker de notificacoes assincrono.
- Gateway para consolidar entrada.
- Frontend Flutter em sprint dedicada pos-backend.
- Roadmap de expansoes para transformar o projeto em estudo completo (financeiro, checklist, votacao, timeline, offline, PDF, clima e recomendacoes).

## Criterio de sucesso do estudo
O projeto sera considerado bem-sucedido quando:
- O fluxo principal (cadastro -> login -> criar viagem -> convidar -> aceitar -> acessar viagem) estiver funcional ponta a ponta.
- O padrao Vertical Slice estiver consistente nos servicos.
- Existirem testes minimos, logs e documentacao de API para as features entregues.
