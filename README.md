# fcs-ui

Aplicação **Conexão Solidária** — interface do usuário multiplataforma.
Implementada em .NET MAUI Blazor Hybrid (Android/iOS/Windows) + Blazor
WebAssembly (Web), com componentes compartilhados via RCL `Fcs.UI.Shared`.

> Aplicação que compõe o MVP da Conexão Solidária junto a `fcs-identity`,
  `fcs-campaign`, `fcs-donations`, `fcs-donation-worker`, `fcs-audit-logs`
  e `fcs-infra`.

---

## Responsabilidades

- Login e autenticação via `fcs-identity` com suporte offline.
- Dashboard com campanhas ativas (painel de transparência).
- Realização de doações com rastreamento de status.
- Histórico de doações com status em tempo real.
- Perfil do usuário (Doador/GestorONG).

---

## Stack

| Camada | Tecnologia |
|---|---|
| Mobile | .NET MAUI Blazor Hybrid (net10.0) |
| Web | Blazor WebAssembly (net10.0) |
| Shared | RCL `Fcs.UI.Shared` (componentes .razor) |
| State | CommunityToolkit.Mvvm + Signals |
| HTTP | Refit + Polly |
| Cache | SQLite (Microsoft.Data.Sqlite) |
| Telemetry | OpenTelemetry + Serilog |

---

## Estrutura

```
src/
├── Fcs.UI.Domain/            net10.0, classlib
├── Fcs.UI.Application/       net10.0, classlib
├── Fcs.UI.Cache/             net10.0, classlib (SQLite)
├── Fcs.UI.Infrastructure/    net10.0, classlib (Refit, Polly, OTel)
├── Fcs.UI.Shared/            net10.0, RCL
├── Fcs.UI.Maui/              net10.0-android/ios/windows
└── Fcs.UI.Web/               net10.0, Blazor Wasm
tests/
├── Fcs.UI.UnitTests/         xUnit (69 tests)
├── Fcs.UI.IntegrationTests/  xUnit (13 tests)
└── Fcs.UI.Benchmarks/        BenchmarkDotNet
```

---

## Pré-requisitos

- .NET 10 SDK + MAUI workload
- Docker Desktop (para backends via `docker-compose.unificado.yml`)
- Android SDK (para build Android)
- Windows App SDK (para build Windows)

## Desenvolvimento

```bash
# Build class libraries
dotnet build src/Fcs.UI.Shared

# Web (dev server)
dotnet run --project src/Fcs.UI.Web

# MAUI (Windows)
dotnet build src/Fcs.UI.Maui -f net10.0-windows10.0.19041.0

# Testes
dotnet test tests/Fcs.UI.UnitTests
dotnet test tests/Fcs.UI.IntegrationTests
```
