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

## Estado do repositório

Até existir código (`.sln` / `.csproj` no repositório), esta estrutura é a **especificação alvo**; ao criar a solution, validar que coincide com esta lista.
