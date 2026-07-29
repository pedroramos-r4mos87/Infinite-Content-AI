# Segurança e Gestão de Segredos

## Objetivo

Definir como o Infinite Content AI protegerá usuários, organizações, dados, credenciais, integrações, arquivos, execuções e operações sensíveis.

A segurança deverá fazer parte da arquitetura desde o início.

Ela não será tratada apenas como uma camada adicionada depois da implementação.

O sistema deverá adotar controles de segurança proporcionais ao risco, evitando tanto negligência quanto complexidade desnecessária.

---

# Princípios de Segurança

A arquitetura seguirá os seguintes princípios:

- Menor privilégio.
    
- Negação por padrão.
    
- Defesa em profundidade.
    
- Validação em todas as bordas.
    
- Segregação de responsabilidades.
    
- Minimização de dados.
    
- Proteção de segredos.
    
- Rastreabilidade.
    
- Rotação de credenciais.
    
- Segurança por configuração.
    
- Falha segura.
    
- Zero Trust entre componentes.
    
- Proteção contra abuso.
    
- Auditoria de operações críticas.
    

---

# Responsabilidade Compartilhada

A segurança não pertence a apenas um projeto da solution.

Cada camada possui responsabilidades específicas.

## Domain

Responsável por:

- Regras de autorização de negócio.
    
- Estados permitidos.
    
- Invariantes.
    
- Proteção de operações críticas.
    
- Regras de aprovação.
    
- Limites de uso do produto.
    

## Application

Responsável por:

- Verificar permissões.
    
- Aplicar políticas.
    
- Validar contexto da organização.
    
- Proteger casos de uso.
    
- Evitar acesso cruzado entre organizações.
    
- Registrar operações auditáveis.
    

## API

Responsável por:

- Autenticação.
    
- Autorização de entrada.
    
- Validação de tokens.
    
- Rate limiting.
    
- Proteção de endpoints.
    
- Validação de requests.
    
- Headers de segurança.
    
- Respostas seguras.
    

## Data

Responsável por:

- Isolamento dos dados.
    
- Constraints.
    
- Proteção de dados sensíveis.
    
- Criptografia em repouso quando aplicável.
    
- Auditoria.
    
- Controle de concorrência.
    
- Retenção e exclusão.
    

## Infrastructure

Responsável por:

- Integrações seguras.
    
- Gestão de credenciais.
    
- TLS.
    
- Assinaturas.
    
- OAuth.
    
- Rotação de tokens.
    
- Proteção de chamadas externas.
    
- Validação de certificados.
    
- Cofres de segredos.
    

## Worker

Responsável por:

- Validar mensagens.
    
- Respeitar identidade e organização.
    
- Evitar processamento duplicado.
    
- Proteger jobs sensíveis.
    
- Não registrar segredos.
    
- Aplicar limites de execução.
    

---

# Modelo de Ameaças

O sistema deverá considerar ameaças como:

- Roubo de conta.
    
- Escalada de privilégio.
    
- Vazamento entre organizações.
    
- Exposição de API keys.
    
- Roubo de tokens OAuth.
    
- Prompt injection.
    
- Uso indevido de IA.
    
- Publicação não autorizada.
    
- Webhooks falsificados.
    
- Reprocessamento de mensagens.
    
- Upload de arquivos maliciosos.
    
- Abuso de endpoints caros.
    
- Excesso de consumo financeiro.
    
- Vazamento em logs.
    
- Vazamento de prompts.
    
- Acesso indevido a arquivos.
    
- Ataques de força bruta.
    
- Replay de requisições.
    
- SSRF.
    
- Injeção de comandos.
    
- SQL injection.
    
- Mass assignment.
    
- Dependências comprometidas.
    
- Manipulação de pipelines.
    
- Alteração indevida de aprovações.
    
- Publicações duplicadas.
    
- Exfiltração por providers externos.
    

---

# Identidades do Sistema

O sistema poderá trabalhar com diferentes tipos de identidade:

- Usuário humano.
    
- Organização.
    
- Serviço interno.
    
- Worker.
    
- Integração externa.
    
- n8n.
    
- Webhook de provider.
    
- Plataforma de publicação.
    
- Serviço Python.
    
- Job administrativo.
    

Cada identidade deverá possuir permissões explícitas.

---

# Autenticação

Autenticação confirma quem está realizando uma operação.

O sistema poderá utilizar:

- OpenID Connect.
    
- OAuth 2.0.
    
- JWT.
    
- Cookies seguros.
    
- API keys para integrações controladas.
    
- Credenciais de serviço.
    
- Assinaturas de webhook.
    
- Tokens de curta duração.
    

A estratégia exata deverá ser registrada em ADR.

---

# Autenticação de Usuários

Para usuários humanos, a recomendação inicial é utilizar um provedor de identidade compatível com:

```text
OpenID Connect
    +
OAuth 2.0
```

Possíveis alternativas:

- ASP.NET Core Identity.
    
- Auth0.
    
- Keycloak.
    
- Microsoft Entra ID.
    
- Amazon Cognito.
    
- Clerk.
    
- Outro provedor compatível.
    

A decisão dependerá de:

- Custo.
    
- Complexidade.
    
- Controle.
    
- Multi-tenancy.
    
- Login social.
    
- MFA.
    
- Recuperação de conta.
    
- Escalabilidade.
    
- Portabilidade.
    

---

# Tokens de Acesso

Tokens de acesso deverão:

- Possuir curta duração.
    
- Ser validados quanto à assinatura.
    
- Ser validados quanto ao emissor.
    
- Ser validados quanto à audiência.
    
- Ser validados quanto à expiração.
    
- Utilizar algoritmos seguros.
    
- Possuir escopos mínimos.
    
- Não conter dados sensíveis desnecessários.
    

---

# Refresh Tokens

Refresh tokens deverão receber proteção adicional.

Eles deverão:

- Ser armazenados com segurança.
    
- Possuir rotação.
    
- Ser revogáveis.
    
- Ser vinculados à sessão.
    
- Possuir expiração.
    
- Ser invalidados após uso suspeito.
    
- Ser criptografados em repouso quando persistidos.
    

Um refresh token reutilizado após rotação poderá indicar roubo de sessão.

---

# Cookies

Caso cookies sejam utilizados, deverão possuir:

```text
HttpOnly
Secure
SameSite
```

A configuração dependerá do fluxo de autenticação.

Cookies de autenticação não deverão ser acessíveis por JavaScript.

Proteções contra CSRF deverão ser aplicadas quando necessário.

---

# Autenticação Multifator

MFA deverá ser considerada para:

- Administradores.
    
- Operações financeiras.
    
- Gestão de credenciais.
    
- Conexão de contas externas.
    
- Publicações automáticas.
    
- Alteração de políticas críticas.
    
- Exclusão de organização.
    

No MVP, poderá ser inicialmente opcional, mas a arquitetura não deverá impedir sua adoção.

---

# Autorização

Autorização define o que uma identidade pode fazer.

O sistema deverá utilizar autorização baseada em:

- Papel.
    
- Permissão.
    
- Organização.
    
- Recurso.
    
- Estado do recurso.
    
- Propriedade.
    
- Política de negócio.
    

Não será suficiente verificar apenas se o usuário está autenticado.

---

# RBAC

O sistema poderá começar com Role-Based Access Control.

Papéis possíveis:

```text
Owner
Administrator
Editor
Reviewer
Publisher
Analyst
Viewer
```

---

# Owner

Pode:

- Gerenciar a organização.
    
- Gerenciar faturamento.
    
- Gerenciar membros.
    
- Excluir a organização.
    
- Configurar integrações.
    
- Definir administradores.
    

---

# Administrator

Pode:

- Gerenciar projetos.
    
- Configurar providers.
    
- Gerenciar pipelines.
    
- Gerenciar membros, conforme política.
    
- Visualizar custos.
    
- Configurar automações.
    

---

# Editor

Pode:

- Criar projetos.
    
- Criar conteúdo.
    
- Alterar roteiros.
    
- Executar pipelines.
    
- Gerenciar artefatos editoriais.
    

---

# Reviewer

Pode:

- Revisar conteúdos.
    
- Aprovar ou rejeitar artefatos.
    
- Adicionar observações.
    
- Solicitar alterações.
    

---

# Publisher

Pode:

- Aprovar publicação.
    
- Agendar conteúdo.
    
- Publicar.
    
- Gerenciar conexões de plataformas, se permitido.
    

---

# Analyst

Pode:

- Visualizar métricas.
    
- Gerar relatórios.
    
- Consultar custos.
    
- Analisar desempenho.
    

---

# Viewer

Pode:

- Visualizar projetos.
    
- Visualizar execuções.
    
- Visualizar conteúdos autorizados.
    
- Não pode realizar alterações.
    

---

# Permissões Granulares

Papéis representam agrupamentos.

As verificações reais deverão utilizar permissões.

Exemplos:

```text
projects.create
projects.read
projects.update
projects.delete

pipelines.execute
pipelines.cancel
pipelines.retry

content.review
content.approve

publications.create
publications.schedule
publications.publish
publications.delete

providers.configure
providers.view_costs

members.manage
organization.delete
```

---

# Autorização Baseada em Política

O ASP.NET Core poderá utilizar policies.

Exemplo conceitual:

```csharp
services.AddAuthorization(options =>
{
    options.AddPolicy(
        "CanPublishContent",
        policy => policy.RequireClaim(
            "permission",
            "publications.publish"));
});
```

Endpoints poderão exigir políticas específicas.

---

# Autorização de Recurso

Permissões globais não são suficientes.

Também deverá ser verificado se o usuário pode acessar o recurso específico.

Exemplo:

```text
Usuário possui projects.read
    +
Projeto pertence à organização atual
    +
Usuário participa dessa organização
```

Todos os critérios devem ser verdadeiros.

---

# Multi-Tenancy

O Infinite Content AI deverá proteger os dados entre organizações.

A abordagem inicial será:

```text
Shared Database
    +
Shared Schema
    +
OrganizationId
```

Toda entidade pertencente a um cliente deverá possuir `OrganizationId`.

---

# Contexto da Organização

Cada requisição deverá resolver um contexto de organização.

Exemplo conceitual:

```csharp
public interface ICurrentOrganization
{
    Guid OrganizationId { get; }

    bool IsAvailable { get; }
}
```

O contexto poderá ser obtido a partir de:

- Claim do token.
    
- Header validado.
    
- Subdomínio.
    
- Rota.
    
- Sessão ativa.
    

O valor nunca deverá ser aceito cegamente do request.

---

# Proteção contra Vazamento entre Organizações

Toda consulta deverá considerar a organização atual.

Exemplo incorreto:

```csharp
await repository.GetByIdAsync(projectId);
```

Exemplo preferível:

```csharp
await repository.GetByIdAsync(
    organizationId,
    projectId,
    cancellationToken);
```

Ou o isolamento poderá ser protegido por um contexto consistente e filtros globais cuidadosamente testados.

---

# Regra de Propriedade

Uma entidade somente poderá ser acessada quando:

```text
entity.OrganizationId == currentOrganization.Id
```

Exceções administrativas deverão ser explícitas, auditadas e restritas.

---

# Bypass Administrativo

Operações de suporte ou administração global deverão:

- Exigir papel específico.
    
- Exigir autenticação reforçada.
    
- Registrar o operador.
    
- Registrar o motivo.
    
- Registrar os recursos acessados.
    
- Possuir duração limitada.
    
- Ser revisáveis.
    

Não deverá existir um bypass silencioso.

---

# Segurança na Application

Todo caso de uso sensível deverá verificar autorização.

Não depender apenas do endpoint.

Motivos:

- O caso de uso pode ser chamado pelo Worker.
    
- Pode ser chamado por outro endpoint.
    
- Pode ser chamado por mensagem.
    
- Pode ser chamado por automação.
    
- Pode ser reutilizado futuramente.
    

A Application deve proteger suas próprias operações.

---

# Segurança em Workers

Mensagens assíncronas deverão transportar apenas o contexto necessário.

Exemplo:

```csharp
public sealed record StartPipelineCommand(
    Guid MessageId,
    Guid OrganizationId,
    Guid PipelineExecutionId,
    Guid RequestedBy,
    string CorrelationId);
```

O Worker deverá:

- Validar o contexto.
    
- Carregar a execução correta.
    
- Confirmar a organização.
    
- Verificar o estado atual.
    
- Aplicar idempotência.
    
- Registrar o solicitante.
    

---

# Identidade de Serviço

Workers e serviços internos deverão possuir identidade própria.

Evitar credenciais compartilhadas entre:

- API.
    
- Worker.
    
- Serviço Python.
    
- n8n.
    
- Job de migrations.
    
- Ferramentas administrativas.
    

Cada componente deverá possuir:

- Credencial separada.
    
- Permissão mínima.
    
- Rotação independente.
    
- Auditoria própria.
    

---

# Menor Privilégio

Exemplos:

## API

Pode:

- Ler e gravar dados necessários.
    
- Publicar mensagens.
    
- Ler segredos necessários.
    

Não precisa:

- Administrar banco.
    
- Excluir filas.
    
- Gerenciar usuários do cluster.
    

## Worker

Pode:

- Consumir filas específicas.
    
- Persistir estados de execução.
    
- Acessar providers necessários.
    

Não precisa:

- Gerenciar configurações globais.
    
- Alterar usuários.
    

## Migration Job

Pode:

- Alterar schema.
    

Não precisa:

- Publicar mensagens.
    
- Chamar providers de IA.
    
- Publicar conteúdos.
    

---

# Gestão de Segredos

Segredos são valores que concedem acesso a sistemas ou dados.

Exemplos:

- API keys.
    
- Client secrets.
    
- Senhas.
    
- Connection strings.
    
- Refresh tokens.
    
- Access tokens.
    
- Chaves privadas.
    
- Webhook secrets.
    
- Credenciais de broker.
    
- Credenciais de storage.
    
- Chaves de criptografia.
    
- Tokens de plataformas.
    

---

# Regra Fundamental para Segredos

Segredos nunca deverão ser:

- Versionados no Git.
    
- Incluídos em imagens Docker.
    
- Gravados em logs.
    
- Exibidos em mensagens de erro.
    
- Enviados ao frontend sem necessidade.
    
- Armazenados em texto puro.
    
- Compartilhados por mensagens.
    
- Inseridos diretamente no código.
    
- Copiados para documentação.
    
- Armazenados em prompts.
    

---

# Armazenamento de Segredos

Segredos deverão ser armazenados em mecanismos como:

- Variáveis de ambiente.
    
- Azure Key Vault.
    
- AWS Secrets Manager.
    
- Google Secret Manager.
    
- HashiCorp Vault.
    
- Docker Secrets.
    
- Kubernetes Secrets com proteção adequada.
    
- Secret store do ambiente de desenvolvimento.
    

A escolha deverá ser registrada em ADR.

---

# Ambiente de Desenvolvimento

No desenvolvimento local, poderão ser utilizados:

- .NET User Secrets.
    
- Variáveis de ambiente.
    
- Arquivo local não versionado.
    
- Cofre de desenvolvimento.
    

Exemplo:

```text
dotnet user-secrets
```

Arquivos como `.env` deverão estar no `.gitignore`.

Um arquivo `.env.example` poderá existir apenas com nomes de variáveis e valores fictícios.

---

# Estrutura de Configuração

Exemplo:

```json
{
  "Providers": {
    "OpenAI": {
      "ApiKey": "",
      "DefaultModel": "",
      "Enabled": true
    }
  }
}
```

O arquivo versionado deverá conter apenas a estrutura.

O valor real deverá vir do ambiente seguro.

---

# Validação na Inicialização

Configurações críticas deverão ser validadas no startup.

Exemplo conceitual:

```csharp
services
    .AddOptions<OpenAiOptions>()
    .BindConfiguration(OpenAiOptions.SectionName)
    .ValidateDataAnnotations()
    .Validate(options =>
        !options.Enabled ||
        !string.IsNullOrWhiteSpace(options.ApiKey))
    .ValidateOnStart();
```

A aplicação deverá falhar rapidamente quando uma configuração obrigatória estiver ausente.

---

# Rotação de Segredos

Todo segredo deverá possuir estratégia de rotação.

A rotação poderá ocorrer:

- Periodicamente.
    
- Após incidente.
    
- Após saída de colaborador.
    
- Após exposição suspeita.
    
- Após alteração de ambiente.
    
- Antes da expiração.
    
- Quando exigido pelo provider.
    

---

# Rotação sem Downtime

Quando possível, a aplicação deverá suportar:

1. Adicionar nova credencial.
    
2. Atualizar serviços.
    
3. Validar nova credencial.
    
4. Revogar a antiga.
    
5. Confirmar ausência de falhas.
    

Sistemas que suportam múltiplas chaves temporariamente facilitam a transição.

---

# Inventário de Segredos

O projeto deverá manter um inventário técnico sem armazenar os valores.

Exemplo:

|Segredo|Responsável|Ambiente|Rotação|
|---|---|---|---|
|OpenAI API Key|Plataforma|Produção|90 dias|
|Database Password|Infraestrutura|Produção|90 dias|
|YouTube OAuth Client Secret|Integrações|Produção|Sob necessidade|
|Webhook Signing Secret|Plataforma|Produção|180 dias|

O inventário deverá registrar:

- Nome.
    
- Finalidade.
    
- Responsável.
    
- Sistema.
    
- Ambientes.
    
- Data de criação.
    
- Última rotação.
    
- Próxima rotação.
    
- Dependências.
    

---

# Segredos por Organização

Algumas credenciais pertencerão a clientes.

Exemplos:

- Conta do YouTube.
    
- Conta do TikTok.
    
- Conta do Instagram.
    
- Webhook externo.
    
- API key própria do cliente.
    

Esses segredos deverão estar vinculados ao `OrganizationId`.

---

# Armazenamento de Credenciais de Clientes

A aplicação poderá armazenar:

- Referência ao segredo no cofre.
    
- Metadados da conexão.
    
- Provider.
    
- Escopos.
    
- Data de expiração.
    
- Status.
    
- Última sincronização.
    

Evitar armazenar o segredo diretamente na tabela principal.

Exemplo:

```text
external_connections
├── id
├── organization_id
├── provider
├── secret_reference
├── status
├── scopes
├── expires_at
└── created_at
```

---

# Criptografia

A segurança deverá considerar:

- Criptografia em trânsito.
    
- Criptografia em repouso.
    
- Criptografia em nível de aplicação.
    
- Gestão de chaves.
    

---

# Criptografia em Trânsito

Toda comunicação externa deverá utilizar TLS.

Exemplos:

- Frontend para API.
    
- API para banco.
    
- API para broker.
    
- API para storage.
    
- Worker para providers.
    
- Webhooks.
    
- Serviços internos.
    

HTTP sem TLS somente poderá existir em desenvolvimento local controlado.

---

# Criptografia em Repouso

Deverá ser habilitada para:

- Banco.
    
- Backups.
    
- Storage.
    
- Volumes.
    
- Logs sensíveis.
    
- Cofres de segredos.
    

A criptografia oferecida pelo provedor de infraestrutura deverá ser utilizada sempre que disponível.

---

# Criptografia em Nível de Aplicação

Alguns campos poderão exigir criptografia antes da persistência.

Exemplos:

- Refresh tokens.
    
- Credenciais de publicação.
    
- Segredos de webhooks.
    
- Tokens de integrações.
    
- Chaves privadas.
    

A aplicação deverá armazenar apenas o conteúdo cifrado.

---

# Gestão de Chaves de Criptografia

Chaves de criptografia não deverão ficar no mesmo local dos dados cifrados.

A gestão deverá considerar:

- Key Vault.
    
- KMS.
    
- Rotação.
    
- Versionamento.
    
- Controle de acesso.
    
- Auditoria.
    
- Recuperação.
    
- Revogação.
    

O registro deverá indicar qual versão da chave cifrou o dado.

---

# Hash versus Criptografia

## Hash

Utilizado quando o valor não precisa ser recuperado.

Exemplos:

- Senhas.
    
- Tokens de verificação.
    
- Chaves de idempotência sensíveis.
    
- Alguns API tokens internos.
    

## Criptografia

Utilizada quando o valor precisa ser recuperado.

Exemplos:

- Refresh token.
    
- Credencial OAuth.
    
- Chave de integração.
    

Senhas nunca deverão ser criptografadas de forma reversível.

---

# Senhas

Caso a aplicação gerencie senhas diretamente, deverá utilizar:

- Algoritmo moderno.
    
- Salt individual.
    
- Parâmetros de custo adequados.
    
- Biblioteca consolidada.
    
- Proteção contra força bruta.
    

Preferir o mecanismo do provedor de identidade.

Nunca implementar algoritmo próprio.

---

# OAuth com Plataformas Externas

Integrações com YouTube, TikTok, Instagram e outras plataformas poderão utilizar OAuth 2.0.

Fluxo:

```text
Usuário inicia conexão
    ↓
Aplicação gera state
    ↓
Usuário autoriza na plataforma
    ↓
Plataforma retorna authorization code
    ↓
Application valida state
    ↓
Infrastructure troca code por tokens
    ↓
Tokens são protegidos
    ↓
Conexão é registrada
```

---

# Parâmetro State

O `state` deverá:

- Ser imprevisível.
    
- Possuir expiração.
    
- Ser vinculado ao usuário.
    
- Ser vinculado à organização.
    
- Ser validado no callback.
    
- Ser utilizado uma única vez.
    

Ele protege contra ataques de CSRF no fluxo OAuth.

---

# PKCE

PKCE deverá ser utilizado quando suportado e adequado.

Especialmente para clientes públicos ou fluxos onde o client secret não pode ser protegido.

---

# Escopos OAuth

Solicitar apenas os escopos necessários.

Evitar permissões amplas como:

```text
acesso total à conta
```

quando a aplicação necessita apenas de:

```text
publicar vídeos
```

Escopos deverão ser documentados.

---

# Renovação de Tokens

O sistema deverá:

- Detectar expiração.
    
- Renovar tokens com segurança.
    
- Atualizar o valor protegido.
    
- Registrar falhas.
    
- Marcar conexão como inválida.
    
- Solicitar reconexão quando necessário.
    

---

# Revogação

Quando uma conexão for removida:

1. Tentar revogar os tokens no provider.
    
2. Remover ou inutilizar as credenciais locais.
    
3. Marcar a conexão como revogada.
    
4. Cancelar operações futuras.
    
5. Registrar auditoria.
    

---

# API Keys Internas

API keys poderão ser utilizadas para integrações simples.

Cada API key deverá:

- Possuir identificador.
    
- Possuir hash persistido.
    
- Ser exibida apenas uma vez.
    
- Possuir escopos.
    
- Possuir organização.
    
- Possuir data de expiração.
    
- Ser revogável.
    
- Possuir última utilização.
    
- Permitir rotação.
    

---

# Estrutura de API Key

Exemplo:

```text
icai_live_abcd1234.secret
```

A parte pública poderá identificar a chave.

A parte secreta deverá ser validada por hash.

O valor completo não deverá ser armazenado.

---

# Assinatura de Webhooks

Webhooks deverão possuir assinatura criptográfica.

Exemplo:

```text
HMAC-SHA256
```

Fluxo:

```text
Payload
    +
Timestamp
    +
Secret
    ↓
Signature
```

O receptor recalcula e compara a assinatura.

---

# Regras para Webhooks Recebidos

Todo webhook deverá:

- Validar assinatura.
    
- Validar timestamp.
    
- Validar Content-Type.
    
- Limitar tamanho do payload.
    
- Validar schema.
    
- Aplicar idempotência.
    
- Evitar replay.
    
- Registrar o provider.
    
- Responder rapidamente.
    
- Processar tarefas longas em background.
    

---

# Comparação de Assinaturas

A comparação deverá ser realizada em tempo constante quando possível.

Evitar comparações simples que possam expor diferenças de tempo.

---

# Proteção contra Replay

Um webhook deverá possuir:

- Timestamp.
    
- MessageId.
    
- Janela máxima de validade.
    
- Registro de processamento.
    

Exemplo:

```text
Webhook com mais de cinco minutos
    ↓
Rejeitado
```

O tempo exato deverá ser configurável.

---

# Webhooks de Saída

Webhooks enviados pela aplicação deverão:

- Utilizar HTTPS.
    
- Ser assinados.
    
- Possuir timestamp.
    
- Possuir MessageId.
    
- Possuir versão.
    
- Possuir timeout.
    
- Possuir retentativas.
    
- Evitar dados sensíveis desnecessários.
    
- Registrar entregas.
    

---

# Segurança de Mensageria

Mensagens internas deverão utilizar:

- Autenticação no broker.
    
- TLS.
    
- Filas protegidas.
    
- Credenciais separadas.
    
- Permissões por fila ou tópico.
    
- Payload mínimo.
    
- Versionamento.
    
- Idempotência.
    
- Dead Letter Queue.
    

---

# Confiança em Mensagens Internas

Uma mensagem interna não deverá ser considerada segura apenas por estar no broker.

O consumidor deverá validar:

- Tipo.
    
- Versão.
    
- Campos obrigatórios.
    
- Organização.
    
- Identificador.
    
- Estado do recurso.
    
- Origem quando disponível.
    
- Assinatura, em cenários de maior risco.
    

---

# Dados Sensíveis em Mensagens

Evitar transportar em mensagens:

- Access tokens.
    
- Refresh tokens.
    
- Senhas.
    
- API keys.
    
- Prompts sensíveis completos.
    
- Arquivos grandes.
    
- Dados pessoais desnecessários.
    

Preferir referências internas seguras.

---

# Segurança de Arquivos

Uploads representam uma superfície de ataque importante.

Todo arquivo recebido deverá ser tratado como não confiável.

---

# Validação de Upload

Validar:

- Tamanho.
    
- Extensão.
    
- MIME type declarado.
    
- MIME type real.
    
- Assinatura binária.
    
- Nome.
    
- Quantidade.
    
- Dimensões.
    
- Duração.
    
- Tipo permitido.
    
- Organização proprietária.
    

---

# Tipos Permitidos

O sistema deverá utilizar allowlist.

Exemplo:

```text
image/png
image/jpeg
audio/mpeg
audio/wav
video/mp4
text/vtt
application/json
```

Não confiar apenas na extensão.

---

# Nome de Arquivo

O nome original não deverá ser utilizado diretamente como chave de storage.

Preferir identificadores gerados pelo sistema.

Exemplo:

```text
organization/{organizationId}/artifacts/{artifactId}
```

O nome original poderá ser armazenado apenas como metadado sanitizado.

---

# Antivírus e Análise

Arquivos fornecidos por usuários poderão passar por:

- Antivírus.
    
- Malware scanning.
    
- Validação de conteúdo.
    
- Sandbox.
    
- Reprocessamento seguro.
    
- Quarentena.
    

A necessidade dependerá do risco e da exposição do produto.

---

# Quarentena

Fluxo possível:

```text
Upload
    ↓
Storage temporário
    ↓
Status Quarantined
    ↓
Validação
    ↓
Approved ou Rejected
```

Arquivos em quarentena não deverão ser utilizados por pipelines.

---

# URLs Pré-Assinadas

URLs temporárias de upload e download deverão:

- Possuir curta duração.
    
- Restringir método.
    
- Restringir arquivo.
    
- Restringir tamanho quando possível.
    
- Ser vinculadas à organização.
    
- Não conceder acesso ao bucket inteiro.
    

---

# Acesso a Arquivos

A aplicação não deverá expor caminhos internos do storage.

Fluxo preferencial:

```text
Usuário solicita arquivo
    ↓
Application autoriza
    ↓
Infrastructure gera URL temporária
    ↓
Usuário acessa o arquivo
```

A autorização deve ocorrer antes da geração da URL.

---

# SSRF

Integrações que aceitam URLs externas deverão proteger contra Server-Side Request Forgery.

O sistema deverá impedir acesso a:

- `localhost`.
    
- Endereços privados.
    
- Metadata services.
    
- Redes internas.
    
- Protocolos não permitidos.
    
- Portas arbitrárias.
    
- Arquivos locais.
    

---

# Download de Conteúdo Externo

Quando o sistema baixar uma URL:

- Validar protocolo.
    
- Resolver DNS com segurança.
    
- Limitar redirecionamentos.
    
- Revalidar destino após redirecionamento.
    
- Limitar tamanho.
    
- Definir timeout.
    
- Validar conteúdo.
    
- Bloquear IPs internos.
    
- Utilizar allowlist quando possível.
    

---

# Injeção de SQL

O EF Core deverá utilizar queries parametrizadas.

SQL manual deverá sempre utilizar parâmetros.

Nunca concatenar entrada do usuário em SQL.

Exemplo proibido:

```csharp
var sql = $"SELECT * FROM users WHERE name = '{name}'";
```

---

# Injeção de Comandos

Processamento com FFmpeg, Python ou ferramentas de sistema deverá evitar interpolação direta de parâmetros em comandos.

Preferir:

- APIs de processo com argumentos separados.
    
- Allowlist.
    
- Identificadores internos.
    
- Caminhos controlados.
    
- Escapamento apropriado.
    
- Container isolado.
    

---

# Execução de Processos

Serviços de mídia deverão:

- Executar com usuário sem privilégios.
    
- Possuir limite de CPU.
    
- Possuir limite de memória.
    
- Possuir timeout.
    
- Possuir diretório temporário isolado.
    
- Não possuir acesso amplo à rede.
    
- Não possuir acesso a segredos desnecessários.
    
- Limpar arquivos temporários.
    

---

# Prompt Injection

Conteúdo externo poderá tentar manipular agentes.

Exemplos:

- Página dizendo para ignorar instruções.
    
- Documento contendo comandos maliciosos.
    
- Texto tentando extrair segredos.
    
- Conteúdo pedindo acesso a ferramentas.
    
- Instrução escondida em metadados.
    

---

# Regra Fundamental para IA

Conteúdo recuperado de fontes externas é dado.

Ele não é instrução confiável.

Exemplo:

```text
System Instructions
    >
Application Rules
    >
Agent Instructions
    >
User Input
    >
Retrieved External Content
```

Conteúdo externo deverá possuir o menor nível de confiança.

---

# Proteções contra Prompt Injection

- Separar instruções de conteúdo.
    
- Delimitar fontes externas.
    
- Não inserir segredos no contexto.
    
- Restringir ferramentas disponíveis.
    
- Validar tool calls.
    
- Utilizar allowlist de ações.
    
- Solicitar aprovação para operações críticas.
    
- Limitar URLs.
    
- Validar saídas.
    
- Registrar fontes.
    
- Remover conteúdo irrelevante.
    
- Aplicar moderação.
    
- Utilizar modelos de permissão explícitos.
    

---

# Agentes com Ferramentas

Um agente não deverá possuir acesso irrestrito a todas as ferramentas.

Exemplo:

## Research Agent

Pode:

- Consultar busca.
    
- Ler fontes.
    
- Armazenar referências.
    

Não pode:

- Publicar.
    
- Excluir projetos.
    
- Ler segredos.
    
- Alterar faturamento.
    

## Publishing Agent

Pode:

- Publicar artefato aprovado.
    
- Consultar status.
    
- Atualizar publicação.
    

Não pode:

- Alterar roteiro.
    
- Ler credenciais em texto puro.
    
- Modificar organização.
    

---

# Tool Allowlist

Cada agente deverá possuir uma lista explícita de capacidades.

Exemplo conceitual:

```text
ScriptAgent
├── GenerateText
├── ReadResearchArtifact
└── SaveScriptArtifact
```

Qualquer ação fora da lista deverá ser bloqueada.

---

# Aprovação Humana para Ações Críticas

Ações de alto impacto poderão exigir aprovação.

Exemplos:

- Publicar conteúdo.
    
- Excluir publicação.
    
- Gastar acima de limite.
    
- Alterar provider principal.
    
- Compartilhar dados externamente.
    
- Conectar conta.
    
- Excluir projeto.
    
- Alterar permissões.
    
- Executar conteúdo sensível.
    

---

# Controle Financeiro

Endpoints e pipelines capazes de gerar custo deverão possuir proteção.

Controles:

- Limite por execução.
    
- Limite por projeto.
    
- Limite por organização.
    
- Limite diário.
    
- Limite mensal.
    
- Aprovação acima de valor.
    
- Rate limiting.
    
- Modelos permitidos.
    
- Quantidade de tentativas.
    
- Concorrência máxima.
    

---

# Abuso de Recursos

Um usuário não deverá conseguir iniciar milhares de jobs caros sem controle.

Estratégias:

- Quotas.
    
- Rate limits.
    
- Limites de concorrência.
    
- Filas por prioridade.
    
- Orçamento.
    
- Bloqueio temporário.
    
- Alertas.
    
- Aprovação manual.
    

---

# Rate Limiting

O sistema poderá aplicar limites por:

- IP.
    
- Usuário.
    
- Organização.
    
- API key.
    
- Endpoint.
    
- Tipo de operação.
    
- Provider.
    
- Custo estimado.
    

Endpoints caros deverão possuir limites mais restritivos.

---

# Exemplos de Categorias

```text
Authentication
StandardApi
ExpensiveAiOperation
FileUpload
Webhook
Publishing
Administrative
```

Cada categoria poderá possuir política própria.

---

# Proteção contra Força Bruta

Aplicável a:

- Login.
    
- Recuperação de conta.
    
- MFA.
    
- Validação de códigos.
    
- API keys.
    
- Convites.
    
- OAuth state.
    

Controles:

- Rate limiting.
    
- Backoff.
    
- Bloqueio temporário.
    
- Captcha quando necessário.
    
- Alertas.
    
- Detecção de anomalias.
    

---

# Validação de Entrada

Toda entrada deverá ser validada antes de chegar às regras centrais.

Validar:

- Tipo.
    
- Tamanho.
    
- Formato.
    
- Faixa.
    
- Obrigatoriedade.
    
- Enum.
    
- Identificador.
    
- URL.
    
- Data.
    
- Conteúdo permitido.
    
- Relacionamento.
    
- Organização.
    

---

# Mass Assignment

Requests não deverão ser mapeados automaticamente para entidades de domínio.

Exemplo de risco:

```text
Request contém IsAdministrator = true
    ↓
Mapper copia todos os campos
```

Preferir comandos específicos com apenas os campos permitidos.

---

# Limites de Payload

Definir limites para:

- JSON.
    
- Multipart.
    
- Webhooks.
    
- Uploads.
    
- Prompts.
    
- Respostas.
    
- Arquivos.
    
- Quantidade de itens.
    

Payloads ilimitados podem causar consumo excessivo de memória e negação de serviço.

---

# Segurança de Respostas

Respostas da API não deverão expor:

- Stack traces.
    
- Connection strings.
    
- Nomes internos.
    
- Segredos.
    
- Tokens.
    
- SQL.
    
- Paths internos.
    
- Detalhes de infraestrutura.
    
- Payloads de providers.
    
- Informações de outras organizações.
    

---

# Problem Details

Erros poderão utilizar `ProblemDetails`.

Exemplo:

```json
{
  "type": "https://errors.infinitecontent.ai/forbidden",
  "title": "Operation not allowed",
  "status": 403,
  "code": "publication_not_allowed",
  "traceId": "..."
}
```

Mensagens externas devem ser seguras.

Detalhes técnicos deverão ficar apenas nos logs internos.

---

# Códigos de Erro

Códigos deverão ser:

- Estáveis.
    
- Documentados.
    
- Sem informações sensíveis.
    
- Independentes de mensagens de providers.
    

Exemplos:

```text
authentication_required
permission_denied
organization_mismatch
invalid_webhook_signature
provider_credential_invalid
execution_cost_limit_exceeded
content_not_approved
```

---

# Logs de Segurança

Eventos importantes deverão ser registrados.

Exemplos:

- Login bem-sucedido.
    
- Login falhou.
    
- MFA alterado.
    
- Permissão alterada.
    
- Membro adicionado.
    
- Membro removido.
    
- Credencial criada.
    
- Credencial revogada.
    
- Provider conectado.
    
- Publicação executada.
    
- Conteúdo excluído.
    
- Organização excluída.
    
- Acesso administrativo.
    
- Webhook inválido.
    
- Limite financeiro excedido.
    

---

# Auditoria

Uma trilha de auditoria deverá registrar:

- Quem.
    
- O quê.
    
- Quando.
    
- Organização.
    
- Recurso.
    
- Ação.
    
- Resultado.
    
- IP, quando apropriado.
    
- User agent, quando apropriado.
    
- Correlation ID.
    
- Estado anterior.
    
- Estado posterior.
    
- Motivo.
    

---

# Audit Log

Estrutura possível:

```text
audit_logs
├── id
├── organization_id
├── actor_type
├── actor_id
├── action
├── resource_type
├── resource_id
├── result
├── metadata
├── ip_address
├── user_agent
├── correlation_id
└── occurred_at
```

---

# Imutabilidade da Auditoria

Logs de auditoria não deverão ser editáveis por usuários comuns.

Operações de exclusão deverão preservar o histórico conforme requisitos legais.

A retenção deverá ser definida por política.

---

# Dados Pessoais

O sistema deverá coletar apenas os dados necessários.

Princípios:

- Minimização.
    
- Finalidade.
    
- Transparência.
    
- Segurança.
    
- Retenção limitada.
    
- Controle de acesso.
    
- Exclusão quando aplicável.
    
- Portabilidade quando necessária.
    

---

# Classificação de Dados

Os dados poderão ser classificados como:

## Públicos

Exemplos:

- Conteúdo publicado.
    
- Página pública.
    

## Internos

Exemplos:

- Métricas técnicas.
    
- Configurações não sensíveis.
    

## Confidenciais

Exemplos:

- Roteiros não publicados.
    
- Estratégias.
    
- Dados de projetos.
    
- Prompts proprietários.
    

## Restritos

Exemplos:

- Tokens.
    
- API keys.
    
- Credenciais.
    
- Dados financeiros.
    
- Informações pessoais sensíveis.
    

A classificação deverá influenciar armazenamento, acesso e logging.

---

# Retenção

Dados sensíveis não deverão permanecer armazenados indefinidamente.

Definir retenção para:

- Tokens revogados.
    
- Logs de acesso.
    
- Payloads de webhook.
    
- Prompts.
    
- Respostas de IA.
    
- Arquivos temporários.
    
- Logs de auditoria.
    
- Dados de contas removidas.
    
- Backups.
    

---

# Exclusão de Organização

A exclusão de organização deverá ser uma operação controlada.

Poderá exigir:

- Confirmação reforçada.
    
- MFA.
    
- Período de espera.
    
- Cancelamento de execuções.
    
- Revogação de tokens.
    
- Remoção de integrações.
    
- Exclusão de arquivos.
    
- Anonimização.
    
- Auditoria.
    
- Notificação.
    

---

# Backups

Backups também contêm dados sensíveis.

Deverão possuir:

- Criptografia.
    
- Controle de acesso.
    
- Retenção.
    
- Auditoria.
    
- Testes de restauração.
    
- Processo de descarte.
    
- Proteção contra alteração.
    

---

# Segurança de Cache

Redis e caches deverão:

- Exigir autenticação.
    
- Utilizar TLS quando possível.
    
- Não ficar expostos publicamente.
    
- Possuir rede restrita.
    
- Evitar armazenar segredos em texto puro.
    
- Utilizar TTL.
    
- Não ser fonte de verdade.
    
- Separar ambientes.
    

---

# Segurança do Banco

O PostgreSQL deverá:

- Utilizar TLS.
    
- Restringir acesso por rede.
    
- Utilizar credenciais separadas.
    
- Aplicar menor privilégio.
    
- Manter backups criptografados.
    
- Não ficar publicamente acessível.
    
- Registrar falhas de acesso.
    
- Rotacionar credenciais.
    
- Aplicar atualizações de segurança.
    

---

# Segurança do Storage

O storage deverá:

- Permanecer privado por padrão.
    
- Bloquear acesso público global.
    
- Utilizar URLs temporárias.
    
- Registrar acessos.
    
- Utilizar criptografia.
    
- Aplicar políticas por prefixo.
    
- Separar ambientes.
    
- Configurar lifecycle.
    
- Validar uploads.
    

---

# Segurança do n8n

O n8n deverá ser tratado como sistema externo privilegiado.

Controles:

- Autenticação.
    
- HTTPS.
    
- Credenciais próprias.
    
- Webhooks assinados.
    
- Acesso de rede restrito.
    
- Segredos protegidos.
    
- Workflows versionados.
    
- Logs controlados.
    
- Permissões mínimas.
    

O n8n não deverá receber acesso direto ao banco de produção.

---

# Segurança do Serviço Python

O serviço Python deverá:

- Possuir identidade própria.
    
- Receber apenas dados necessários.
    
- Executar em container isolado.
    
- Utilizar usuário não root.
    
- Possuir filesystem restrito.
    
- Possuir limites de recurso.
    
- Possuir timeout.
    
- Não acessar segredos desnecessários.
    
- Não expor endpoints publicamente sem necessidade.
    
- Validar requests.
    
- Autenticar chamadas.
    

---

# Segurança de Containers

Imagens deverão:

- Utilizar base mínima.
    
- Evitar execução como root.
    
- Não conter segredos.
    
- Possuir versões fixadas.
    
- Ser verificadas por scanner.
    
- Remover ferramentas desnecessárias.
    
- Utilizar filesystem read-only quando possível.
    
- Separar build e runtime.
    
- Possuir health checks.
    

---

# Dependências

Dependências externas representam risco.

O projeto deverá:

- Fixar versões.
    
- Atualizar regularmente.
    
- Executar análise de vulnerabilidades.
    
- Remover pacotes não utilizados.
    
- Revisar pacotes críticos.
    
- Utilizar lock files.
    
- Acompanhar alertas de segurança.
    
- Evitar bibliotecas abandonadas.
    

---

# Supply Chain

A pipeline de CI/CD deverá considerar:

- Dependabot ou equivalente.
    
- Scanning de imagens.
    
- Scanning de secrets.
    
- Verificação de licenças.
    
- SBOM.
    
- Assinatura de artefatos.
    
- Proteção de branches.
    
- Revisão obrigatória.
    
- Controle de ambientes.
    

---

# Secret Scanning

O repositório deverá possuir análise automática para detectar:

- API keys.
    
- Tokens.
    
- Senhas.
    
- Connection strings.
    
- Chaves privadas.
    
- Segredos de webhook.
    

Caso um segredo seja versionado, ele deverá ser considerado comprometido e rotacionado.

Remover apenas do histórico não é suficiente.

---

# CI/CD

A pipeline não deverá imprimir segredos.

Segredos deverão ser injetados apenas nas etapas necessárias.

Jobs de pull request de código não confiável não deverão receber segredos de produção.

---

# Ambientes

Cada ambiente deverá possuir:

- Banco separado.
    
- Storage separado.
    
- Credenciais separadas.
    
- Providers configurados separadamente.
    
- URLs separadas.
    
- Chaves de assinatura separadas.
    
- Broker separado ou isolado.
    
- OAuth callbacks específicos.
    

Nunca reutilizar segredos de produção no desenvolvimento.

---

# Produção

O ambiente de produção deverá possuir controles mais rígidos:

- MFA administrativo.
    
- Logs de auditoria.
    
- Acesso restrito.
    
- Rotação.
    
- Monitoramento.
    
- Alertas.
    
- Backups.
    
- Aprovação de deploy.
    
- Mudanças rastreáveis.
    
- Segredos em cofre.
    
- Rede privada quando possível.
    

---

# Feature Flags e Segurança

Feature flags sensíveis deverão ser protegidas.

Exemplos:

```text
EnableAutomaticPublishing
EnableHighCostModels
EnableExternalWebhooks
EnableAdminImpersonation
```

Alterações deverão exigir permissão e gerar auditoria.

---

# Segurança por Estado

Algumas operações dependem do estado atual.

Exemplo:

```text
Publication.Status == Approved
```

Somente nesse estado a publicação poderá ocorrer.

Autorização e validação de estado devem trabalhar juntas.

Um usuário autorizado não poderá ignorar invariantes do domínio.

---

# Publicação Segura

Antes de publicar, o sistema deverá validar:

- Organização.
    
- Conta conectada.
    
- Permissão.
    
- Artefato aprovado.
    
- Versão aprovada.
    
- Plataforma.
    
- Política de conteúdo.
    
- Idempotency key.
    
- Limite financeiro.
    
- Agendamento.
    
- Estado do pipeline.
    

---

# Operações Irreversíveis

Operações irreversíveis deverão possuir proteção adicional.

Exemplos:

- Excluir organização.
    
- Revogar credencial principal.
    
- Publicar conteúdo público.
    
- Excluir publicação externa.
    
- Apagar artefato aprovado.
    
- Alterar proprietário.
    
- Remover todos os membros.
    

Possíveis controles:

- Confirmação.
    
- Reautenticação.
    
- MFA.
    
- Delay.
    
- Aprovação dupla.
    
- Auditoria.
    
- Notificação.
    

---

# Respostas de Providers

Respostas de providers externos deverão ser tratadas como não confiáveis.

Validar:

- Schema.
    
- Tipo.
    
- Tamanho.
    
- URLs.
    
- MIME type.
    
- Conteúdo.
    
- Metadados.
    
- Identificadores.
    
- Status.
    
- Arquivos.
    
- Instruções inesperadas.
    

---

# Dados Enviados para IA

Antes de enviar dados a um provider, avaliar:

- O dado é necessário?
    
- Contém informação pessoal?
    
- Contém segredo?
    
- Contém material confidencial?
    
- O provider pode armazenar o conteúdo?
    
- A região é permitida?
    
- A política do cliente permite?
    
- Existe opção de anonimização?
    

---

# Redação de Dados

Dados sensíveis poderão ser removidos ou mascarados.

Exemplos:

```text
email@example.com
```

poderá virar:

```text
[EMAIL]
```

Outros exemplos:

- Tokens.
    
- Telefones.
    
- Endereços.
    
- Identificadores pessoais.
    
- Credenciais.
    
- Dados financeiros.
    

---

# Políticas por Provider

Cada provider deverá possuir documentação sobre:

- Dados enviados.
    
- Finalidade.
    
- Retenção.
    
- Região.
    
- Segurança.
    
- Custos.
    
- Capacidades.
    
- Riscos.
    
- Política de uso.
    

A organização poderá bloquear providers específicos.

---

# Consentimento e Configuração

Uma organização poderá escolher:

- Quais providers podem ser utilizados.
    
- Se dados podem sair da região.
    
- Se prompts podem ser armazenados.
    
- Se respostas podem ser usadas para avaliação.
    
- Se publicação automática é permitida.
    
- Se aprovação humana é obrigatória.
    
- Limite de custo.
    

---

# Observabilidade de Segurança

Métricas importantes:

- Falhas de autenticação.
    
- Falhas de autorização.
    
- Webhooks inválidos.
    
- API keys revogadas.
    
- Tokens expirados.
    
- Tentativas de acesso cruzado.
    
- Rate limits atingidos.
    
- Uploads rejeitados.
    
- Jobs bloqueados.
    
- Operações administrativas.
    
- Falhas de assinatura.
    
- Credenciais inválidas.
    
- Eventos suspeitos.
    

---

# Alertas

Alertas deverão ser criados para eventos como:

- Muitas falhas de login.
    
- Aumento repentino de custo.
    
- Uso anormal de provider.
    
- Falhas repetidas de webhook.
    
- Tentativa de acesso a outra organização.
    
- Publicações em volume incomum.
    
- Secret exposto.
    
- Token OAuth revogado.
    
- Alteração de Owner.
    
- Exclusão em massa.
    
- Fila com mensagens suspeitas.
    
- Falhas de integridade.
    

---

# Resposta a Incidentes

O projeto deverá possuir processo para incidentes.

Etapas:

1. Detectar.
    
2. Conter.
    
3. Investigar.
    
4. Revogar credenciais.
    
5. Corrigir.
    
6. Restaurar.
    
7. Notificar responsáveis.
    
8. Registrar evidências.
    
9. Revisar causa.
    
10. Prevenir recorrência.
    

---

# Evidências

Durante um incidente, preservar:

- Logs.
    
- Audit logs.
    
- Correlation IDs.
    
- Eventos.
    
- Acessos.
    
- Mudanças.
    
- Versões.
    
- Mensagens.
    
- IPs.
    
- Horários.
    

Evitar modificar dados necessários para investigação.

---

# Revogação de Emergência

O sistema deverá permitir:

- Desativar provider.
    
- Revogar API key.
    
- Desconectar plataforma.
    
- Suspender organização.
    
- Desabilitar publicação automática.
    
- Parar pipelines.
    
- Bloquear usuário.
    
- Invalidar sessões.
    
- Rotacionar webhook secret.
    

---

# Testes de Segurança

## Testes Unitários

Validar:

- Policies.
    
- Regras de permissão.
    
- Isolamento por organização.
    
- Estados permitidos.
    
- Validação de assinatura.
    
- Redação de dados.
    
- Limites financeiros.
    
- Expiração de tokens.
    
- Proteção de operações críticas.
    

---

## Testes de Integração

Validar:

- Autenticação.
    
- Autorização.
    
- Multi-tenancy.
    
- Webhooks.
    
- API keys.
    
- URLs temporárias.
    
- Uploads.
    
- Headers.
    
- Rate limiting.
    
- Revogação.
    
- Criptografia.
    
- Auditoria.
    

---

## Testes Negativos

Testar explicitamente:

- Usuário sem permissão.
    
- Usuário de outra organização.
    
- Token expirado.
    
- Token com audiência inválida.
    
- Webhook sem assinatura.
    
- Webhook repetido.
    
- Arquivo inválido.
    
- Payload muito grande.
    
- URL interna.
    
- Chave revogada.
    
- Artefato não aprovado.
    
- Custo excedido.
    

---

# Análise Automatizada

A CI poderá executar:

- SAST.
    
- Dependency scanning.
    
- Secret scanning.
    
- Container scanning.
    
- IaC scanning.
    
- Testes de segurança.
    
- Análise de licenças.
    

---

# Pentest

Antes de uma exposição pública relevante, considerar testes focados em:

- Autenticação.
    
- Autorização.
    
- Multi-tenancy.
    
- Uploads.
    
- Webhooks.
    
- OAuth.
    
- SSRF.
    
- APIs.
    
- Publicação.
    
- Manipulação de pipelines.
    
- Consumo financeiro.
    

---

# Checklist para Novo Endpoint

Todo endpoint deverá responder:

- Exige autenticação?
    
- Qual permissão é necessária?
    
- Qual organização está sendo acessada?
    
- O recurso pertence à organização?
    
- A entrada está validada?
    
- Existe limite de tamanho?
    
- Existe rate limit?
    
- A operação gera custo?
    
- A operação precisa de auditoria?
    
- A resposta expõe dados sensíveis?
    
- A operação é idempotente?
    
- Existe risco de replay?
    
- Existe risco de abuso?
    

---

# Checklist para Nova Integração

Toda nova integração deverá responder:

- Qual credencial utiliza?
    
- Onde o segredo será armazenado?
    
- Qual a política de rotação?
    
- Quais dados serão enviados?
    
- O provider retém dados?
    
- A comunicação utiliza TLS?
    
- Existe timeout?
    
- Existe rate limit?
    
- Existe validação da resposta?
    
- Existe fallback?
    
- Existe auditoria?
    
- Existe idempotência?
    
- Existe risco de SSRF?
    
- Existe webhook?
    
- Como a assinatura será validada?
    

---

# Checklist para Novo Agente

Todo novo agente deverá responder:

- Quais dados pode acessar?
    
- Quais ferramentas pode utilizar?
    
- Quais ações pode executar?
    
- Pode gerar custo?
    
- Pode publicar?
    
- Pode acessar conteúdo externo?
    
- Pode receber prompt injection?
    
- A saída é validada?
    
- Existe aprovação humana?
    
- Quais informações são enviadas ao provider?
    
- Existem segredos no contexto?
    
- Existe limite de execução?
    

---

# Checklist para Novo Pipeline

Todo pipeline deverá responder:

- Quem pode executar?
    
- Qual organização é proprietária?
    
- Qual custo máximo?
    
- Quais etapas são críticas?
    
- Quais etapas exigem aprovação?
    
- Quais ações são irreversíveis?
    
- Como o cancelamento funciona?
    
- Como a retomada é protegida?
    
- Como duplicidade é evitada?
    
- Quais credenciais são necessárias?
    
- Quais dados saem do sistema?
    
- Quais eventos são auditados?
    

---

# Regras Arquiteturais

- Segurança deve existir em todas as camadas.
    
- Autenticação não substitui autorização.
    
- Autorização deve considerar a organização.
    
- Casos de uso sensíveis devem se proteger.
    
- Nenhum segredo deve ser versionado.
    
- Nenhum segredo deve aparecer em logs.
    
- Segredos devem possuir rotação.
    
- Credenciais devem possuir menor privilégio.
    
- Componentes devem possuir identidades separadas.
    
- Tokens OAuth devem ser protegidos em repouso.
    
- Webhooks devem ser assinados.
    
- Webhooks devem ser idempotentes.
    
- Uploads devem ser validados.
    
- Arquivos devem ser privados por padrão.
    
- URLs temporárias devem possuir curta duração.
    
- Conteúdo externo deve ser tratado como não confiável.
    
- Agentes devem possuir ferramentas limitadas.
    
- Ações críticas devem permitir aprovação humana.
    
- Operações caras devem possuir limites.
    
- Todas as bordas devem validar entrada.
    
- Respostas externas devem ser validadas.
    
- Erros não devem expor detalhes internos.
    
- Eventos críticos devem gerar auditoria.
    
- Dados entre organizações devem permanecer isolados.
    
- Dados enviados à IA devem ser minimizados.
    
- Produção deve utilizar cofres de segredos.
    
- Ambientes não devem compartilhar credenciais.
    
- Dependências devem ser monitoradas.
    
- Incidentes devem possuir procedimento de resposta.
    

---

# Decisões Pendentes

As seguintes decisões deverão ser registradas em ADRs:

- Provedor de identidade.
    
- Estratégia de autenticação do frontend.
    
- Uso de cookies ou bearer tokens.
    
- Estratégia de MFA.
    
- Modelo inicial de papéis e permissões.
    
- Estratégia de multi-tenancy.
    
- Provedor de Secret Manager.
    
- Estratégia de criptografia de tokens.
    
- Rotação de chaves.
    
- Política de retenção de logs.
    
- Política de armazenamento de prompts.
    
- Estratégia de assinatura de webhooks.
    
- Política de upload e antivírus.
    
- Estratégia de auditoria.
    
- Regras de publicação automática.
    
- Política de providers permitidos.
    
- Estratégia de rate limiting.
    
- Política de exclusão de organizações.
    
- Estratégia de resposta a incidentes.
    

---

# Exemplo Completo

```text
Usuário solicita publicação
    ↓
API valida access token
    ↓
Application resolve organização
    ↓
Policy valida publications.publish
    ↓
Application carrega a execução
    ↓
Confirma que a execução pertence à organização
    ↓
Confirma que o artefato está aprovado
    ↓
Confirma que o limite financeiro não foi excedido
    ↓
Obtém referência protegida da conexão externa
    ↓
Infrastructure acessa o segredo no cofre
    ↓
Publishing Provider publica o conteúdo
    ↓
Resultado é validado
    ↓
Estado e custo são persistidos
    ↓
Evento é salvo na Outbox
    ↓
Audit log registra a operação
```

Fluxo de webhook:

```text
Provider envia webhook
    ↓
API valida HTTPS
    ↓
Valida timestamp
    ↓
Valida assinatura
    ↓
Valida MessageId
    ↓
Valida schema
    ↓
Registra na Inbox
    ↓
Publica comando interno
    ↓
Worker processa
    ↓
Application confirma organização e estado
    ↓
Atualiza a execução
    ↓
Registra auditoria
```

---

# Objetivo Final

Criar uma arquitetura segura, auditável e preparada para operar com múltiplas organizações, providers, plataformas e agentes.

O Infinite Content AI deverá proteger usuários e dados sem comprometer a evolução do produto.

Toda nova funcionalidade deverá nascer com autenticação, autorização, isolamento, validação, observabilidade e gestão de segredos consideradas desde o início.