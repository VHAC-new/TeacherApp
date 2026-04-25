# TeacherApp — Primeiros passos (guia de execução)

Este documento consolida **a ordem de execução** e **o mapa da documentação** que guia o início do projeto. Ele não substitui os documentos existentes; ele apenas organiza o “como começar”.

## 1) Arquitetura e stack (o “shape” do projeto)

- Referência principal: [arquitetura/arquitetura.md](arquitetura/arquitetura.md)
- Solution esperada:

```text
TeacherApp.sln
 ├─ TeacherApp.App      (.NET MAUI)
 ├─ TeacherApp.Admin    (Blazor Server)
 ├─ TeacherApp.Api      (ASP.NET Core Web API)
 └─ TeacherApp.Tests    (testes)
```

- Decisões-chave:
  - **API centraliza** regra de negócio.
  - **PostgreSQL + EF Core + Migrations** para persistência.
  - **JWT + Roles** (`Student`, `Teacher`, `Admin`) para autenticação/autorização.
  - **Áudios no S3** (não no banco). O banco guarda apenas metadados e `file_key`.

## 2) Ordem recomendada de implementação (incremental)

### 2.1 API primeiro (base do sistema)

- Guia: [backend/api.md](backend/api.md)
- Estrutura interna (camadas/pastas): `Controllers/`, `Application/`, `Domain/`, `Infrastructure/`, `Contracts/`, `Common/`
- Controllers previstos:
  - Públicos: `AuthController`, `ModulesController`, `LessonsController`, `ExercisesController`, `ProgressController`, `AudioController`
  - Admin: `/api/admin/*` (módulos, lições, exercícios, mídia)

### 2.2 Banco + EF Core (persistência e migrations)

- Guia: [backend/banco.md](backend/banco.md)
- Tech: PostgreSQL + EF Core
- Estrutura esperada:

```text
Data
 ├─ AppDbContext
 ├─ Mappings
 └─ Migrations
```

- Princípios:
  - Controllers **não** acessam `DbContext` diretamente.
  - Usar **Services/Repositories**.
  - Separar **entidades** de **DTOs**.

### 2.3 Segurança (JWT + Roles)

- Guia: [backend/seguranca.md](backend/seguranca.md)
- Roles: `Student`, `Teacher`, `Admin`
- Uso:

```csharp
[Authorize(Roles = "Teacher,Admin")]
```

### 2.4 Storage de áudio (S3)

- Guia: [backend/storage.md](backend/storage.md)
- Estratégia:
  - Áudio no S3
  - Banco guarda `file_key` (ex.: `audios/module-1/lesson-10.mp3`)
  - API gera URL segura e retorna para app/admin

### 2.5 Domínio e regras (antes de “feature complete”)

- Guia: [dominio/dominio.md](dominio/dominio.md)
- Entidades: `Users`, `Modules`, `Lessons`, `Exercises`, `MediaFiles`, progresso/tentativas
- Regras a respeitar desde cedo:
  - Lição pode ser **rascunho**
  - Preferir **exclusão lógica**
  - `OrderIndex` define ordenação
  - Áudio não pode ser removido se estiver em uso
  - Validação de resposta ignora case e espaços extras

### 2.6 Frontends (depois da base da API)

- Admin (Blazor): [frontend/admin.md](frontend/admin.md)
- App (MAUI, MVVM): [frontend/app-mobile.md](frontend/app-mobile.md)

### 2.7 Testes

- Guia: [fundamentos/testes.md](fundamentos/testes.md)
- Foco: **Services**, validações e regras de negócio (testar comportamento).
- Padrão de nome:

```text
Metodo_Condicao_ResultadoEsperado
```

## 3) Padrões obrigatórios (para manter consistência)

- Boas práticas: [fundamentos/boas-praticas.md](fundamentos/boas-praticas.md)
- Nomenclatura: [fundamentos/padrao-nomenclatura.md](fundamentos/padrao-nomenclatura.md)
- Git workflow: [fundamentos/git-workflow.md](fundamentos/git-workflow.md)

## 4) Checklist mínimo do “primeiro PR” (estrutura)

- Solution `TeacherApp.sln` criada com os 4 projetos.
- Pastas base criadas (principalmente no `TeacherApp.Api`).
- Dependências mínimas da API adicionadas (EF Core + PostgreSQL + JWT + S3).
- Build/restore ok.

