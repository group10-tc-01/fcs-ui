# Guia de Uso — fcs-ui

## Sobre

**fcs-ui** é a interface de usuário da plataforma Conexão Solidária, disponível como app mobile (Android/iOS) via MAUI Blazor Hybrid e como aplicação web via Blazor WebAssembly.

## Plataformas

| Plataforma | Tecnologia | Instalação |
|---|---|---|
| Android | MAUI Blazor Hybrid | APK (Google Play ou sideload) |
| iOS | MAUI Blazor Hybrid | IPA (App Store ou TestFlight) |
| Windows | MAUI Blazor Hybrid | MSIX (Store ou sideload) |
| Web | Blazor WebAssembly | Navegador (URL pública) |

## Funcionalidades

### Autenticação

- Login com email e senha via Keycloak (proxy fcs-identity)
- Token JWT armazenado localmente (SecureStorage no mobile, localStorage no web)
- Logout limpa o token e redireciona para a tela de login

### Dashboard

- Resumo de campanhas ativas
- Indicadores de doações recebidas
- Acesso rápido às principais operações

### Doação

- Selecionar campanha
- Definir valor (validação: > 0)
- Confirmar doação → registrada localmente (SQLite) + enviada ao backend
- Estratégia de conflito (LastWriteWins / RemoteWins / MergeManual) se dados offline conflitarem

### Histórico

- Lista de doações realizadas
- Origem dos dados: cache local SQLite com sincronização remota
- Deep link: `fcs://history` abre diretamente esta tela

### Perfil

- Dados do usuário logado
- Preferências (tema, notificações)
- Biometria (impressão digital / Face ID no mobile)

## Navegação

### Rotas

| Rota | Componente | Descrição |
|---|---|---|
| `/` ou `/login` | Login | Autenticação |
| `/dashboard` | Dashboard | Home pós-login |
| `/donate` | Donate | Realizar doação |
| `/history` | History | Histórico de doações |
| `/profile` | Profile | Perfil do usuário |

### Deep Links (Android)

| URI | Destino |
|---|---|
| `fcs://donate` | Página de doação |
| `fcs://history` | Histórico |

Atalhos de app (home screen): donate e history (quando suportado pelo launcher).

## Sincronização offline

O app mantém um cache SQLite local com:

- **Event Store**: eventos de domínio persistidos localmente antes do envio remoto
- **Sync Service**: 3 estratégias de conflito:
  - `LastWriteWins`: vence o dado mais recente
  - `RemoteWins`: prioriza servidor
  - `MergeManual`: conflito exposto ao usuário

### Foreground Service (Android)

- Sincroniza cache a cada 15 minutos
- Notificação persistente na barra de status
- Permissões: `FOREGROUND_SERVICE`, `FOREGROUND_SERVICE_DATA_SYNC`, `POST_NOTIFICATIONS`

## Uso via CLI (desenvolvimento)

```bash
# Build para Windows
dotnet build src/Fcs.UI.Maui -f net10.0-windows10.0.19041.0

# Build para Android
dotnet build src/Fcs.UI.Maui -f net10.0-android

# Executar testes
dotnet test Fcs.UI.slnx
```

## Configuração

### appsettings.json

```json
{
  "ApiEndpoints": {
    "Identity": "http://localhost:5001",
    "Donations": "http://localhost:5002",
    "Campaign": "http://localhost:5003"
  },
  "Cache": {
    "DatabasePath": "fcs-cache.db",
    "SyncIntervalMinutes": 15
  }
}
```

### Variáveis de ambiente

| Variável | Descrição | Padrão |
|---|---|---|
| `OTEL_EXPORTER_OTLP_ENDPOINT` | Endpoint do OTel Collector | `http://localhost:4317` |
| `SERILOG__SEQ__URL` | Endpoint do Seq | — |
