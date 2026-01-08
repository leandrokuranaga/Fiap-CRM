# 🕹️ FIAP CRM - Sistema de Customer Relationship Management

Sistema de CRM desenvolvido como projeto acadêmico da FIAP, implementando uma arquitetura robusta para gerenciamento de usuários e biblioteca de jogos com integração a serviços externos.

## 📋 Sobre o Projeto

Este projeto implementa um sistema CRM completo utilizando **Clean Architecture** e **Domain-Driven Design (DDD)**, desenvolvido em **.NET 8** com foco em escalabilidade, manutenibilidade e boas práticas de desenvolvimento.

### 🎯 Funcionalidades Principais

- **Gerenciamento de Usuários**: Criação, atualização, consulta e exclusão de usuários
- **Sistema de Autenticação**: Login com JWT tokens para autorização
- **Biblioteca de Jogos**: Gestão da biblioteca pessoal de jogos dos usuários
- **Integração Externa**: Comunicação com APIs externas (FCG - Games API)
- **Processamento de Eventos**: Sistema de mensageria para comunicação entre microsserviços

## 🏗️ Arquitetura

### Camadas da Aplicação
- **API Layer** (`Fiap.CRM.Api`): Controladores REST e configurações da API
- **Application Layer** (`Fiap.CRM.Application`): Casos de uso, validações e DTOs
- **Domain Layer** (`Fiap.CRM.Domain`): Entidades de negócio e regras de domínio
- **Infrastructure Layer**: Implementações de persistência e serviços externos

### 🛠️ Tecnologias Utilizadas
- **.NET 8** - Framework principal
- **ASP.NET Core Web API** - API REST
- **Entity Framework Core** - ORM para acesso a dados
- **MySQL** - Banco de dados principal
- **Redis** - Cache distribuído
- **KurrentDB** - Banco de eventos (outbox pattern)
- **JWT** - Autenticação e autorização
- **Swagger/OpenAPI** - Documentação da API
- **Docker** - Containerização
- **AWS ECS** - Deploy em produção
- **Prometheus** - Métricas e monitoramento
- **Serilog** - Logging estruturado

## 📡 Endpoints da API

### Autenticação
- `POST /api/v1/auth/login` - Autenticação de usuários

### Usuários
- `POST /api/v1/users/create` - Criar usuário regular (envia eventos para crm-queue e lambda-queue)
- `POST /api/v1/users/create-admin` - Criar usuário administrador (envia eventos para crm-queue e lambda-queue)
- `GET /api/v1/users` - Listar todos os usuários (apenas admin)
- `GET /api/v1/users/{id}` - Buscar usuário por ID (apenas admin)
- `PATCH /api/v1/users/{id}` - Atualizar usuário (apenas admin)
- `DELETE /api/v1/users/{id}` - Deletar usuário (apenas admin)
- `GET /api/v1/users/users-games` - Buscar biblioteca de jogos do usuário autenticado

## 🔄 Integrações e Mensageria

### Sistema de Outbox Pattern
- **MySQL**: Armazena dados principais da aplicação
- **KurrentDB**: Persiste eventos através do padrão Outbox para garantir consistência eventual

### Handlers de Eventos
- **AddGameToUserLibraryHandler**: Processa compras bem-sucedidas vindas do serviço `fiap-payment`
- **CreatedUserHandler**: Processa eventos de criação de usuários (envia para crm-queue e lambda-queue)
- **UpdatedUserHandler**: Processa eventos de atualização de usuários
- **DeletedUserHandler**: Processa eventos de exclusão de usuários

### Integração FCG (Games API)
- **IFCGService**: Serviço para buscar dados de jogos externos
- Utilizado para enriquecer as informações da biblioteca do usuário

### Filas de Mensagens
- **crm-queue**:
  - Recebe eventos do serviço `fiap-payment` sobre compras bem-sucedidas
  - Recebe eventos de criação de usuários para persistência no outbox
  - Adiciona automaticamente jogos comprados à biblioteca do usuário
  - Persiste eventos de criação, atualização e exclusão de usuários
- **lambda-queue**:
  - Recebe eventos de criação de usuários
  - Integra com `fiap.events` para envio de email de boas-vindas aos novos usuários

## 🚀 Como Executar

### Pré-requisitos
- .NET 8 SDK
- MySQL
- Redis
- Docker (opcional)

### Docker
```bash
# Build da imagem
docker build -t fiap-crm .

# Executar container
docker run -p 8080:8080 fiap-crm
```

## 📊 Build & Tests
| CI | Status |
| --- | --- |
| Validação PR| [![Pré-validação de Release/Hotfix](https://github.com/leandrokuranaga/fiap-crm/actions/workflows/validate-release-pr.yml/badge.svg)](https://github.com/leandrokuranaga/fiap-crm/actions/workflows/validate-release-pr.yml)
| Validação PR Main e Dev | [![Validate PRs for Main and Dev](https://github.com/leandrokuranaga/fiap-crm/actions/workflows/validate-pr.yml/badge.svg)](https://github.com/leandrokuranaga/fiap-crm/actions/workflows/validate-pr.yml)
| Release | [![Release on Main (.NET 8)](https://github.com/leandrokuranaga/fiap-crm/actions/workflows/release.yml/badge.svg)](https://github.com/leandrokuranaga/fiap-crm/actions/workflows/release.yml)

## 🚀 Deployment
| CD | Status |
| --- | --- |
| Dockerhub | [![Publicar Imagem Docker no Docker Hub](https://github.com/leandrokuranaga/fiap-crm/actions/workflows/deploy-dockerhub.yml/badge.svg)](https://github.com/leandrokuranaga/fiap-crm/actions/workflows/deploy-dockerhub.yml) |
| EKS | [![Deploy to AWS EKS](https://github.com/leandrokuranaga/fiap-crm/actions/workflows/deploy-eks.yml/badge.svg)](https://github.com/leandrokuranaga/fiap-crm/actions/workflows/deploy-eks.yml) |

## 📋 Monitoramento

- **Health Checks**: `/health` - Verifica status do MySQL e Redis
- **Métricas**: `/metrics` - Endpoint para coleta do Prometheus
- **Documentação API**: `/swagger` - Interface Swagger em desenvolvimento

## 🎓 Contexto Acadêmico

Este projeto foi desenvolvido como parte do curso da FIAP, demonstrando:
- Implementação de Clean Architecture
- Padrões de design (Repository, Outbox)
- Integração entre microsserviços
- Práticas de DevOps (CI/CD, Docker, AWS)
- Testes automatizados (Unit e BDD)
- Monitoramento e observabilidade


### 🔗 Fluxo de Criação de Usuários

Os dois endpoints de criação de usuários (`/create` e `/create-admin`) implementam um fluxo completo de mensageria:

1. **Persistência Local**: Dados salvos no MySQL
2. **Outbox Pattern**: Eventos persistidos no KurrentDB via crm-queue
3. **Notificação**: Eventos enviados para lambda-queue que integra com `fiap.events` para envio automático de email de boas-vindas

Este fluxo garante **consistência eventual** e **comunicação assíncrona** entre os microsserviços do ecossistema FIAP.
