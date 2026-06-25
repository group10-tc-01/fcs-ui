# Guia Técnico — fcs-ui

## Stack

| Camada | Tecnologia | Versão |
|---|---|---|
| Linguagem | C# | 13.0 |
| Runtime | .NET | 10.0 |
| UI (MAUI) | .NET MAUI Blazor Hybrid | 10.0 |
| UI (Web) | Blazor WebAssembly | 10.0 |
| Componentes | MudBlazor | 7.16.0 |
| HTTP | Refit | 7.2.1 |
| Resiliência | Polly | 8.5.2 |
| Cache local | SQLite (Microsoft.Data.Sqlite) | 8.0.11 |
| MVVM | CommunityToolkit.Mvvm | 8.4.0 |
| CQRS | MediatR | 12.4.1 |
| Validação | FluentValidation | 11.11.0 |
| Result Pattern | FluentResults | 3.16.0 |
| Testes | xUnit + FluentAssertions + Moq | — |
| Testes (carga) | NBomber | 5.8.1 |
| Testes (benchmark) | BenchmarkDotNet | 0.14.0 |
| Telemetria | OpenTelemetry + Serilog + Seq | — |

## Estrutura de projetos (10)

```
Fcs.UI.slnx
├── src/
│   ├── Fcs.UI.Domain/                  net10.0 — Entidades, Value Objects, regras
│   ├── Fcs.UI.Application/             net10.0 — Use Cases, CQRS handlers
│   ├── Fcs.UI.Infrastructure/          net10.0 — Refit, Polly, OTel, DI
│   ├── Fcs.UI.Cache/                   net10.0 — SQLite local, event store, sync
│   ├── Fcs.UI.Shared/                  net10.0 — RCL .razor compartilhados
│   ├── Fcs.UI.Maui/                    net10.0-android/ios/windows — App MAUI
│   └── Fcs.UI.Web/                     net10.0 — Blazor Wasm standalone
├── tests/
│   ├── Fcs.UI.UnitTests/               net10.0 (69 testes)
│   ├── Fcs.UI.IntegrationTests/        net10.0 (13 testes)
│   └── Fcs.UI.Benchmarks/              net10.0 — BenchmarkDotNet
├── docs/                               Guias técnico e de uso
├── .github/workflows/                  CI/CD
├── docker-compose.test.yml             Dependências para testes
└── Directory.Packages.props            Versões centralizadas
```

## Pré-requisitos

- .NET 10.0 SDK
- MAUI workload: `dotnet workload install maui`
- Android SDK (para build Android)
- Windows App SDK (para build Windows)
- Xcode (para build iOS, somente macOS)
- Docker Desktop (para testes de integração com docker-compose)

## Build

```bash
# Restaurar dependências
dotnet restore Fcs.UI.slnx

# Build completo (exceto iOS no Windows)
dotnet build Fcs.UI.slnx

# Build apenas MAUI para Android
dotnet build src/Fcs.UI.Maui -f net10.0-android

# Build apenas MAUI para Windows
dotnet build src/Fcs.UI.Maui -f net10.0-windows10.0.19041.0

# Build Web
dotnet build src/Fcs.UI.Web

# Publish Android APK
dotnet publish src/Fcs.UI.Maui -f net10.0-android -c Release
```

## Testes

```bash
# Todos os testes
dotnet test Fcs.UI.slnx

# Apenas unitários
dotnet test tests/Fcs.UI.UnitTests

# Apenas integração (requer docker-compose.test.yml)
docker compose -f docker-compose.test.yml up -d
dotnet test tests/Fcs.UI.IntegrationTests
docker compose -f docker-compose.test.yml down

# Benchmarks
dotnet run -c Release --project tests/Fcs.UI.Benchmarks
```

## Arquitetura

### Clean Architecture (DDD)

```
Domain ← Application ← Infrastructure ← UI
   ↑                        ↑
   └── Cache ───────────────┘
```

- **Domain**: entidades, value objects, agregados, interfaces de repositório
- **Application**: use cases (MediatR commands/queries), handlers, interfaces de serviço
- **Infrastructure**: implementações Refit (API), Polly (resiliência), OTel + Serilog (telemetria), DI registration
- **Cache**: SQLite local, event store (IEventStore), sync service (3 estratégias de conflito)
- **Shared**: componentes .razor reutilizados entre MAUI e Web
- **Maui**: app shell, navegação, serviços platform-specific (token storage, connectivity, biometria, foreground service, deep links)
- **Web**: Blazor Wasm standalone, serviços web-specific (localStorage token, connectivity)

### Fluxo de dados (doação)

```
DonatePage → SubmitDonationCommand → SubmitDonationHandler
  ├── IEventStore.AppendAsync (SQLite local)
  ├── IDonationApi.SubmitAsync (Refit → POST /donations)
  └── ISyncService.SyncAsync (conflito remoto vs local)
```

### Fluxo de autenticação

```
LoginPage → AuthService.LoginAsync → IAuthApi.LoginAsync (Refit)
  → Token armazenado via ITokenStorage (SecureStorage no MAUI, localStorage no Web)
  → AuthGuard verifica token antes de qualquer rota protegida
```

### Telemetria

- OpenTelemetry: traces + metrics exportados via OTLP (Coletor OTel → Datadog/Prometheus)
- Serilog: logs estruturados para console + Seq
- ActivitySource: `Fcs.UI.Maui` — spans para operações HTTP, cache, sincronização

## CI/CD

### GitHub Actions (`.github/workflows/maui-ci.yml`)

| Job | Runner | Descrição |
|---|---|---|
| branch-name | ubuntu-latest | Valida nome de branch (PR) |
| secret-scan | ubuntu-latest | Gitleaks em todo o repo |
| dependency-scan | ubuntu-latest | Verifica pacotes vulneráveis |
| build-and-test | windows-latest | Build slnx + unit + integration tests |
| build-android | windows-latest | Publish APK |
| build-windows | windows-latest | Publish MSIX |
| build-ios | macos-latest | Publish IPA |

### Docker Compose para testes

`docker-compose.test.yml` sobe SQL Server + Kafka + WireMock (identity, donations, campaign).

## Troubleshooting

### Erro AMM0000 (android:exported)

Ocorre no build Android se o `AndroidManifest.xml` não tiver `android:exported="true"` no `<activity>` com intent filters. Corrigir adicionando o atributo no elemento.

### MAUI workload

Se o workload MAUI não estiver instalado: `dotnet workload install maui`. Para Android-only: `dotnet workload install maui-android`.

### MudBlazor warnings (MUD0001, MUD0002)

Avisos de análise sobre parâmetros `Command` e `Visible` — funcionam em runtime. Suprimidos via `<NoWarn>` no csproj.

### APT2265

Erro de compilação Android causado por caminho com acentos. O diretório atual (`Fase-5-Agilidade-Seguranca-IA`) é ASCII puro e não causa o problema.
