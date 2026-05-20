# Solution e projetos

## Ficheiro

`TeacherApp.sln`

---

## Projetos

```text
TeacherApp.sln
 ├─ TeacherApp.App          → MAUI (aluno)
 ├─ TeacherApp.Admin        → Blazor Server (admin)
 ├─ TeacherApp.Api          → ASP.NET Core
 ├─ TeacherApp.Contracts    → DTOs, Requests, Responses, enums partilhados
 └─ TeacherApp.Tests        → testes
```

---

## Contratos

O assembly **TeacherApp.Contracts** é referenciado pela API, pelo app MAUI e pelo Blazor. Detalhes e regras em [contracts.md](contracts.md).

---

## App MAUI — organização

O projeto **TeacherApp.App** usa **Feature Folders** (`Features/{Feature}/Views`, `ViewModels`, `Services`, …) e **Core** para serviços partilhados. Ver [app-mobile-mvvm.md](../frontend/app-mobile-mvvm.md).

## Estado do repositório

A API e os contratos vivem em repositório separado (`TeacherAppApi`); ver [caminho-projeto-api.md](../backend/caminho-projeto-api.md). A solution `TeacherApp.sln` no repositório atual contém **Admin** e **App**.
