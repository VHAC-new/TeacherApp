# Admin — Blazor Server (hosting)

## Modelo

O painel **TeacherApp.Admin** usa **Blazor Server**: componentes executam no servidor e a UI é atualizada via **SignalR** (circuito entre browser e servidor).

---

## Implicações

* **Latência**: cada interação pode implicar ida e volta ao servidor; UI deve evitar trabalho desnecessário por evento
* **Estado de circuito**: estado do servidor por sessão; documentar na implementação política de timeout/reconnect se necessário
* **API**: o admin chama a **TeacherApp.Api** por HTTP (tipicamente `HttpClient` com JWT), como um cliente mais; a regra de negócio continua na API

---

## CORS

A API deve permitir as origens necessárias ao **admin web** (URL do host Blazor) e ao **app mobile**, conforme [operacional-cors.md](../backend/operacional-cors.md). Blazor Server não substitui a API: são origens e clientes distintos.

---

## Autenticação

JWT emitido pela API; o projeto admin deve anexar o token nas chamadas à API (padrão da equipa: handler delegating, header `Authorization`, etc.). Detalhes de roles em [seguranca.md](../backend/seguranca.md).

---

## UI

Componentes e **tema claro/escuro** do painel: biblioteca **MudBlazor** e diretrizes em [admin.md](admin.md) (secções *UI: MudBlazor* e *Suporte para tema Claro/Escuro*).

---

## Ver também

* [admin.md](admin.md) — funcionalidades, MudBlazor, tema e estrutura de pastas do painel
