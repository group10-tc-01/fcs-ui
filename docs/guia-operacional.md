# Guia Operacional — Conexão Solidária (fcs-ui)

Guia completo para executar o fcs-ui localmente com todas as dependências e para realizar o deploy em produção (AKS).

---

## Índice

- [Parte 1 — Execução Local](#parte-1--execução-local)
  - [1.1 Pré-requisitos](#11-pré-requisitos)
  - [1.2 Subir infraestrutura (backends)](#12-subir-infraestrutura-backends)
  - [1.3 Credenciais padrão](#13-credenciais-padrão)
  - [1.4 Executar fcs-ui Web (Blazor WASM)](#14-executar-fcs-ui-web-blazor-wasm)
  - [1.5 Executar fcs-ui Maui](#15-executar-fcs-ui-maui)
  - [1.6 Executar backends individualmente](#16-executar-backends-individualmente)
  - [1.7 Testes](#17-testes)
  - [1.8 Troubleshooting](#18-troubleshooting)
  - [1.9 Referência: docker-compose.unificado.yml](#19-referência-docker-composeunificadoyml)
- [Parte 2 — Produção](#parte-2--produção)
  - [2.1 Containerização do fcs-ui Web](#21-containerização-do-fcs-ui-web)
  - [2.2 Container Registry](#22-container-registry)
  - [2.3 Kubernetes (AKS)](#23-kubernetes-aks)
  - [2.4 CI/CD](#24-cicd)
  - [2.5 Databases](#25-databases)
  - [2.6 Keycloak em produção](#26-keycloak-em-produção)
  - [2.7 Observabilidade](#27-observabilidade)
  - [2.8 Secrets e variáveis de ambiente](#28-secrets-e-variáveis-de-ambiente)

---

## Parte 1 — Execução Local

### 1.1 Pré-requisitos

| Ferramenta | Versão | Obrigatório |
|---|---|---|
| Docker Desktop | 4.x+ | Sim (backends) |
| .NET SDK | 10.0.301+ | Sim (fcs-ui) |
| MAUI workload | 10.0.x | Apenas para build Android/iOS |
| Android SDK | 34+ | Apenas para build Android |
| Node.js | 22+ | Opcional (fcs-mobile) |

Verificar SDK:

```powershell
dotnet --list-sdks
# Deve mostrar 10.0.301 ou superior
dotnet workload list
# Deve listar maui (se for buildar MAUI)
```

### 1.2 Subir infraestrutura (backends)

O compose unificado sobe todos os serviços necessários:

```powershell
# Da raiz do workspace
cd projetos
docker compose -f docker-compose.unificado.yml up -d
```

Aguardar ~2-3 minutos para inicialização completa.

**Serviços iniciados:**

| Serviço | Container | Porta host | Porta container | Dependency |
|---|---|---|---|---|
| SQL Server | fcs-sqlserver | 1433 | 1433 | — |
| MongoDB | fcs-mongodb | 27017 | 27017 | — |
| Zookeeper | fcs-zookeeper | 2181 | 2181 | — |
| Kafka | fcs-kafka | 9092 / 29092 | 9092 / 29092 | zookeeper |
| Seq | fcs-seq | 5341 | 5341 | — |
| Keycloak DB Init | fcs-keycloak-db-init | — | — | sqlserver |
| Keycloak | fcs-keycloak | 8081 | 8080 | sqlserver |
| fcs-identity | fcs-identity | 5001 | 8080 | sqlserver, kafka, keycloak |
| fcs-campaign | fcs-campaign | 5002 | 8080 | sqlserver, kafka |
| fcs-donations | fcs-donations | 5003 | 8080 | sqlserver, mongodb, kafka |

**Healthcheck:**

```powershell
curl http://localhost:5001/health
curl http://localhost:5002/health
curl http://localhost:5003/health
# Seq: http://localhost:5341
# Keycloak: http://localhost:8081
```

**Derrubar tudo:**

```powershell
docker compose -f docker-compose.unificado.yml down -v
```

### 1.3 Credenciais padrão

| Papel | Usuário | Senha |
|---|---|---|
| Keycloak admin | admin | admin |
| GestorONG (seed automático) | gestor@ong.test | Gestor123! |
| Doador | (registrar na UI) | (definir no registro) |

O realm `conexao-solidaria` é importado automaticamente do arquivo `fcs-identity/keycloak/conexao-solidaria-realm.json`.

### 1.4 Executar fcs-ui Web (Blazor WASM)

```powershell
cd projetos/fcs-ui
dotnet run --project src/Fcs.UI.Web --no-launch-profile
```

Acessar: http://localhost:5000

O Web project consulta as APIs nos endereços configurados em `src/Fcs.UI.Web/Program.cs`:
- Identity: `http://localhost:5001`
- Campaign: `http://localhost:5002`
- Donations: `http://localhost:5003`

### 1.5 Executar fcs-ui Maui

**Windows:**

```powershell
cd projetos/fcs-ui
dotnet build -f net10.0-windows10.0.19041.0 src/Fcs.UI.Maui
# Executar o binário gerado em:
# src/Fcs.UI.Maui/bin/Debug/net10.0-windows10.0.19041.0/Fcs.UI.Maui.exe
```

Ou abrir a solution no Visual Studio 2022+ e executar o projeto `Fcs.UI.Maui`.

**Android (requer MAUI workload + Android SDK):**

```powershell
dotnet build -f net10.0-android src/Fcs.UI.Maui
# O APK será gerado em:
# src/Fcs.UI.Maui/bin/Debug/net10.0-android/*.apk
```

**iOS (requer macOS + Xcode):**

```powershell
dotnet build -f net10.0-ios src/Fcs.UI.Maui
```

### 1.6 Executar backends individualmente

Cada backend possui seu próprio `docker-compose.yml` para desenvolvimento isolado:

```powershell
# Apenas fcs-identity
cd projetos/fcs-identity
docker compose up -d

# Apenas fcs-campaign
cd projetos/fcs-campaign
docker compose up -d

# Apenas fcs-donations
cd projetos/fcs-donations
docker compose up -d
```

Isso é útil para debug de um serviço específico sem subir todos os backends.

### 1.7 Testes

```powershell
cd projetos/fcs-ui
dotnet test Fcs.UI.slnx
```

**Resultado esperado:** 82 testes passando (69 unit + 13 integration).

Para testes com cobertura:

```powershell
dotnet test Fcs.UI.slnx --collect "XPlat Code Coverage"
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coverage-report" -reporttypes:Html
```

### 1.8 Troubleshooting

#### hostpolicy.dll / DevServer não inicia

**Sintoma:** `dotnet run --project src/Fcs.UI.Web` falha com `A fatal error was encountered. The library 'hostpolicy.dll'...`

**Causa:** `<SelfContained>false</SelfContained>` ou versão incorreta dos pacotes WASM.

**Solução:**
- Remover `<SelfContained>false</SelfContained>` do csproj
- Usar `Microsoft.AspNetCore.Components.WebAssembly` e `DevServer` na mesma versão do SDK (`10.0.9` para SDK `10.0.301`)
- Preferir `dotnet run --no-launch-profile`

#### MudBlazor warnings MUD0001 / MUD0002

**Sintoma:** Warnings de parâmetros ilegais em MudButton, MudAlert.

**Causa:** Analisador MudBlazor 7.x identifica parâmetros por padrão antigo (`Command` em MudButton, `Visible` em MudAlert).

**Solução:** Esperado e suprimido no csproj. Não afeta runtime.

#### WASM0001 — SQLitePCL varargs

**Sintoma:** `warning WASM0001: Found a native function (sqlite3_config) with varargs...`

**Causa:** SQLitePCLRaw nativo expõe funções com varargs não suportadas em WASM. Transitivo via Fcs.UI.Cache.

**Solução:** Esperado e inofensivo. O cache SQLite não é usado no Web project.

#### NuGet vulnerability NU1903

**Sintoma:** `warning NU1903: System.Linq.Dynamic.Core 1.3.12 has a known high severity vulnerability`

**Causa:** Transiente do WireMock.Net (testes de integração).

**Solução:** Apenas em testes. Ignorar até WireMock atualizar a dependência.

#### Porta ocupada

Se a porta 5000 já estiver em uso:

```powershell
dotnet run --project src/Fcs.UI.Web --no-launch-profile --urls http://localhost:5005
```

#### docker compose falha

Se o Docker Desktop não estiver rodando:

```powershell
# Verificar status
docker info

# Iniciar Docker Desktop (se necessário)
& "C:\Program Files\Docker\Docker\Docker Desktop.exe"
```

### 1.9 Referência: docker-compose.unificado.yml

Localização: `projetos/docker-compose.unificado.yml`

**Rede:** padrão (bridge) — todos os containers se comunicam pelo nome do serviço.

**Volumes:** nenhum volume persistente definido (dados são perdidos ao derrubar). Para persistência em dev, adicionar volumes nomeados.

**Keycloak realm:** montado como volume read-only de `./fcs-identity/keycloak:/opt/keycloak/data/import:ro`.

**Build das APIs:** cada serviço constrói do seu próprio Dockerfile no contexto do repositório.

---

## Parte 2 — Produção

### 2.1 Containerização do fcs-ui Web

O fcs-ui Web (Blazor WASM) é uma SPA de arquivos estáticos. O Dockerfile usa multi-stage build:

1. **Stage build:** SDK .NET 10 publica o projeto
2. **Stage final:** nginx:alpine serve os arquivos em `/usr/share/nginx/html`

**Dockerfile:** `src/Fcs.UI.Web/Dockerfile`

**nginx.conf:** `src/Fcs.UI.Web/nginx.conf`

O nginx está configurado com fallback para `index.html` (SPA routing) e suporte a MIME type `application/wasm`.

**Build da imagem:**

```powershell
cd projetos/fcs-ui
docker build -f src/Fcs.UI.Web/Dockerfile -t fcs-ui:latest .
```

### 2.2 Container Registry

As imagens devem ser armazenadas em um registry privado. A arquitetura alvo é Azure Container Registry (ACR).

```powershell
# Login
az acr login --name <acr-name>

# Tag e push
docker tag fcs-ui:latest <acr-name>.azurecr.io/fcs-ui:latest
docker push <acr-name>.azurecr.io/fcs-ui:latest
```

### 2.3 Kubernetes (AKS)

Os manifests K8s estão em `k8s/`:

| Arquivo | Recurso | Propósito |
|---|---|---|
| `configmap.yml` | ConfigMap | appsettings.Production.json com URLs das APIs |
| `deployment.yml` | Deployment | 2 réplicas, liveness/readiness probes, monta ConfigMap |
| `service.yml` | Service | ClusterIP na porta 80 |
| `ingress.yml` | Ingress | TLS, host app.conexaosolidaria.com |

**Aplicar:**

```powershell
kubectl create namespace fcs-ui
kubectl apply -f k8s/ -n fcs-ui
```

**Variáveis no deployment.yml:**

O deployment usa placeholders `__ACR_NAME__` e `__IMAGE_TAG__` que são substituídos pelo pipeline CI/CD.

**ConfigMap de produção:**

```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: fcs-ui-config
data:
  appsettings.Production.json: |
    {
      "Api": {
        "Identity": "https://identity.conexaosolidaria.com",
        "Campaign": "https://campaign.conexaosolidaria.com",
        "Donations": "https://donations.conexaosolidaria.com"
      }
    }
```

O ConfigMap é montado como arquivo no nginx, sobrescrevendo o `appsettings.Production.json` padrão.

### 2.4 CI/CD

**Workflow:** `.github/workflows/fcs-ui-cd.yml`

**Trigger:**
- Push na branch `main`
- Tag `v*` (ex: `v1.0.0`)
- Manual (workflow_dispatch)

**Jobs:**

| Job | Descrição |
|---|---|
| `build-scan-push` | Build imagem → push ACR → Trivy scan (CRITICAL/HIGH) |
| `deploy-aks` | (condicional: main ou tag) kubectl apply → rollout status → healthcheck |

**Secrets necessários no GitHub:**

| Secret | Descrição |
|---|---|
| `ACR_NAME` | Nome do ACR (sem azurecr.io) |
| `ACR_USERNAME` | ACR admin username |
| `ACR_PASSWORD` | ACR admin password |
| `AKS_KUBECONFIG` | Kubeconfig do cluster AKS (base64) |

**Pipeline reutilizável:**

Para alinhamento com os demais serviços, o pipeline pode ser migrado para usar `fcs-pipelines/.github/workflows/dotnet-service-delivery.yml` com:

```yaml
jobs:
  deploy:
    uses: <org>/fcs-pipelines/.github/workflows/dotnet-service-delivery.yml@main
    with:
      service_name: fcs-ui
      image_name: fcs-ui
      dockerfile_path: src/Fcs.UI.Web/Dockerfile
      docker_context: .
      azure_container_registry_name: ${{ vars.ACR_NAME }}
      aks_resource_group: ${{ vars.AKS_RESOURCE_GROUP }}
      aks_cluster_name: ${{ vars.AKS_CLUSTER_NAME }}
      k8s_namespace: fcs-ui
      k8s_deployment_name: fcs-ui
      k8s_container_name: fcs-ui
      k8s_manifests_path: k8s
      healthcheck_url: https://app.conexaosolidaria.com
      deploy_to_aks: true
    secrets:
      AZURE_CONTAINER_REGISTRY_USERNAME: ${{ secrets.ACR_USERNAME }}
      AZURE_CONTAINER_REGISTRY_PASSWORD: ${{ secrets.ACR_PASSWORD }}
      AKS_KUBECONFIG: ${{ secrets.AKS_KUBECONFIG }}
```

### 2.5 Databases

Cada serviço usa seu próprio banco de dados:

| Serviço | Banco | Provider | Produção |
|---|---|---|---|
| fcs-identity | KeycloakDb | SQL Server via Keycloak | Azure SQL Managed Instance |
| fcs-campaign | CampaignsDb | SQL Server | Azure SQL Database |
| fcs-donations | fcs_donations | SQL Server | Azure SQL Database |
| fcs-donations | fcs_donations_mongo | MongoDB | MongoDB Atlas |
| fcs-audit-logs | fcs_audit | MongoDB | MongoDB Atlas |

**EF Core Migrations:** as migrations rodam automaticamente na startup (via `app.UseMigration()` ou similar). Em produção, recomenda-se executar como job separado no deploy.

### 2.6 Keycloak em produção

**Recomendações:**

1. **Database:** SQL Server gerenciado (Azure SQL) em vez de H2 embarcado
2. **SSL:** habilitar HTTPS no Keycloak
3. **Realm:** exportar realm configurado e versionar no repositório (`fcs-identity/keycloak/`)
4. **Usuários:** não usar seed em produção — criar via API admin ou integração com provedor de identidade
5. **Alta disponibilidade:** Keycloak em cluster com cache distribuído (JDBC_PING)

### 2.7 Observabilidade

**Stack:**

| Camada | Tecnologia | Porto |
|---|---|---|
| Métricas | Prometheus | 9090 |
| Dashboards | Grafana | 3000 |
| Tracing | OpenTelemetry Collector | 4317 (gRPC) |
| Logs | Seq ou Datadog | 5341 (Seq) |
| Agente | Datadog Agent (opcional) | — |

**Configuração existente:**
- `projetos/fcs-infra/docker/observability/docker-compose.yml` — stack de observabilidade local
- `projetos/fcs-infra/docker/observability/otel-collector/config.yml` — pipeline OTLP → Datadog

**Para produção no AKS:**

```yaml
# Exemplo de ConfigMap para OTel
apiVersion: v1
kind: ConfigMap
metadata:
  name: otel-collector-config
data:
  config.yml: |
    receivers:
      otlp:
        protocols:
          grpc:
            endpoint: 0.0.0.0:4317
    exporters:
      datadog:
        api:
          key: ${DD_API_KEY}
    service:
      pipelines:
        traces: [otlp, datadog]
        metrics: [otlp, datadog]
```

### 2.8 Secrets e variáveis de ambiente

**Nunca hardcodar secrets.** Usar Azure Key Vault + Secret Store CSI driver no AKS.

Os manifests do deployment usam placeholders para substituição no pipeline:

| Placeholder | Substituído por | Origem |
|---|---|---|
| `__ACR_NAME__` | Nome do ACR | Secrets do GitHub |
| `__IMAGE_TAG__` | SHA curto do commit | Gerado no pipeline |
| `__AKS_KEYVAULT_CLIENT_ID__` | Managed Identity do AKS | Secrets do GitHub (se usar fcs-pipelines) |

**Para adicionar um novo secret:**

1. Adicionar ao Key Vault
2. Criar SecretProviderClass no cluster
3. Referenciar no deployment como volume mount
