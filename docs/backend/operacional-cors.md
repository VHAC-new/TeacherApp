# Operacional — CORS

## Objetivo

A API (`TeacherApp.Api`) deve permitir chamadas **cross-origin** apenas das origens necessárias.

---

## Clientes a permitir

1. **App mobile (MAUI)** — conforme esquema de origem usado pelo stack de rede do cliente (em muitos cenários o app não usa browser CORS da mesma forma que SPA; configurar para cenários híbridos/WebView se existirem).
2. **Admin web (Blazor Server)** — URL base onde o admin é servido (ex.: `https://admin.seudominio.pt`).

---

## Diretriz

* Listar origens explicitamente em configuração por ambiente (**não** usar `AllowAnyOrigin` com credenciais).
* Alinhar com autenticação JWT e headers permitidos (`Authorization`, `Content-Type`, etc.).

---

## Ver também

* [admin-hosting.md](../frontend/admin-hosting.md) — modelo Blazor Server
