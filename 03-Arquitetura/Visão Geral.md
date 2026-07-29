# Visão Geral da Arquitetura

## Filosofia

O Infinite Content AI será desenvolvido seguindo os princípios da Clean Architecture, Domain-Driven Design (quando fizer sentido) e SOLID.

A principal preocupação da arquitetura é garantir que o domínio da aplicação permaneça independente de frameworks, bibliotecas e provedores externos.

Toda integração deverá ser facilmente substituível.

---

# Objetivos da Arquitetura

A arquitetura deve ser:

- Simples de entender.
    
- Fácil de evoluir.
    
- Fácil de testar.
    
- Independente de provedores.
    
- Independente do banco de dados.
    
- Independente da interface.
    
- Preparada para crescimento.
    

---

# Princípios

## Clean Architecture

A regra principal é:

> Dependências sempre apontam para dentro.

O domínio nunca conhece detalhes de infraestrutura.

---

## SOLID

Todo componente deverá possuir uma responsabilidade clara.

As dependências serão invertidas através de interfaces.

---

## Separation of Concerns

Cada camada será responsável apenas pelo seu papel.

---

## Dependency Injection

Toda implementação externa será registrada por injeção de dependência.

---

## Fail Fast

Falhas devem acontecer o mais cedo possível.

---

## Composição acima de Herança

Sempre que possível utilizaremos composição.

---

# Estrutura Geral

A aplicação será dividida em cinco grandes blocos.

## 1. API

Responsável por:

- Endpoints
    
- Autenticação
    
- Validação
    
- Swagger
    
- Health Checks
    

---

## 2. Application

Responsável por:

- Casos de uso
    
- Commands
    
- Queries
    
- Orquestração
    
- Interfaces
    

---

## 3. Domain

Responsável por:

- Entidades
    
- Regras de negócio
    
- Value Objects
    
- Eventos de domínio
    

Esta camada não poderá depender de nenhuma tecnologia externa.

---

## 4. Infrastructure

Responsável por:

- Entity Framework
    
- PostgreSQL
    
- OpenAI
    
- Gemini
    
- Anthropic
    
- YouTube
    
- Storage
    
- Mensageria
    

Toda dependência externa ficará nesta camada.

---

## 5. Worker

Responsável por:

- Processamentos longos
    
- Filas
    
- Background Jobs
    
- Processamento de vídeos
    

---

# Arquitetura em Camadas

```text
Presentation
      │
      ▼
Application
      │
      ▼
Domain
      ▲
      │
Infrastructure
```

---

# Fluxo Geral

```text
Usuário
      │
      ▼
API
      │
      ▼
Application
      │
      ▼
Domain
      │
      ▼
Infrastructure
      │
      ▼
Serviços Externos
```

---

# Agentes

A geração de conteúdo será dividida em agentes especializados.

Cada agente possuirá uma única responsabilidade.

Exemplo:

Trend Agent

↓

Research Agent

↓

Script Agent

↓

Translation Agent

↓

Voice Agent

↓

Thumbnail Agent

↓

Video Agent

↓

SEO Agent

↓

Publishing Agent

↓

Analytics Agent

---

# Providers

Nenhum agente conhecerá diretamente OpenAI, Gemini ou Anthropic.

Todos dependerão de abstrações.

Exemplo:

IAiTextProvider

↓

OpenAiProvider

GeminiProvider

AnthropicProvider

---

# Banco de Dados

O banco armazenará:

- Projetos
    
- Conteúdos
    
- Execuções
    
- Prompts
    
- Custos
    
- Publicações
    
- Métricas
    

Nunca armazenaremos lógica de negócio no banco.

---

# Automação

O n8n será utilizado para:

- Agendamento
    
- Workflows
    
- Integrações
    
- Processos automáticos
    

As regras de negócio continuarão dentro da aplicação.

---

# Escalabilidade

O sistema deverá suportar:

- Novos agentes
    
- Novos idiomas
    
- Novos provedores
    
- Novos canais
    
- Novas plataformas
    

Sem necessidade de alterar o núcleo da aplicação.

---

# Padrão de Desenvolvimento

Todo código deverá seguir:

- SOLID
    
- Clean Code
    
- Clean Architecture
    
- Program.cs mínimo
    
- Extension Methods
    
- DI
    
- Async/Await
    
- CancellationToken
    
- Result Pattern
    
- Tratamento centralizado de exceções
    
- Logs estruturados
    

---

# Objetivo Final

Construir uma plataforma escalável de automação de conteúdo, preparada para crescer sem necessidade de grandes refatorações.