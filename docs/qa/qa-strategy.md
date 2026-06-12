# Estratégia de QA — TeacherApp

## Objetivo

Este documento define a estratégia de QA do TeacherApp.

O objetivo é garantir que a API, o painel Admin e o app mobile sejam estáveis, testáveis, seguros e manteníveis.

## Repositórios

O projeto está dividido em **dois repositórios**:

| Repositório | Projetos | Responsabilidade |
|-------------|----------|------------------|
| **TeacherApp** | `TeacherApp.Admin`, `TeacherApp.App` | Painel admin (Blazor Server) e app do aluno (MAUI) |
| **TeacherAppApi** | `TeacherApp.Api`, `TeacherApp.Contracts`, `TeacherApp.Tests` | API, contratos partilhados e testes automatizados |

* `TeacherApp.sln` contém apenas Admin e App.
* A API e os testes ficam em `C:\git\ProjetoDiego\TeacherAppApi` (ver `docs/backend/caminho-projeto-api.md`).
* Alterações em `TeacherApp.Contracts` impactam API, Admin e App — nunca duplicar DTOs (ver `docs/arquitetura/contracts.md`).

## Áreas da aplicação

### API (`TeacherApp.Api`)

Responsabilidades principais:

* Autenticação JWT
* Autorização por roles (`Admin`, `Student`)
* Gestão de utilizadores e alunos (CPF, perfil, ativo/inativo)
* Módulos, lições, exercícios, exercícios finais
* Media/áudio (S3 quando configurado)
* Progresso do aluno e tentativas de exercício
* PostgreSQL + EF Core migrations
* Email (SMTP/SES) no cadastro de aluno

### Painel Admin (`TeacherApp.Admin`)

* Blazor **Server** com **MudBlazor**
* Dashboard
* CRUD módulos, lições, exercícios, exercícios finais
* Upload de media
* Gestão de alunos (lista, detalhes, criar, ativar/desativar acesso)
* Visualização de desempenho e histórico de respostas
* Tema claro/escuro

### App Mobile (`TeacherApp.App`)

* MAUI XAML + MVVM (Feature Folders)
* Login do aluno (role `Student`)
* Home, lista de lições, módulo
* Player de lição com áudio
* Exercícios e página de resultados (score)
* Exercícios finais do módulo
* Perfil, troca de senha, logout, tema claro/escuro
* Padrões de memória: `ICleanup`, cancelamento de tasks, dispose de áudio — ver `maui-boas-praticas-memoria.md`

## Pirâmide de testes

Prioridade alinhada ao estado atual do projeto:

| Nível | Estado atual | Onde |
|-------|--------------|------|
| 1. Testes de integração (API) | **Implementado** | `TeacherAppApi/TeacherApp.Tests` |
| 2. Testes unitários (regras de negócio) | Parcial | Helpers e serviços isolados na API |
| 3. Testes de ViewModel (MAUI) | Não implementado | Manual |
| 4. Testes de componentes (Blazor) | Não implementado | Manual |
| 5. Testes exploratórios (mobile) | Manual | Dispositivo físico recomendado |

Convenção de nomes: `Metodo_Condicao_ResultadoEsperado` — ver `docs/fundamentos/testes.md`.

## Portão de qualidade mínimo

Antes de considerar uma feature concluída:

* Build com sucesso no(s) repositório(s) afetado(s)
* Testes de integração relevantes passam (`dotnet test` em TeacherAppApi)
* Novas regras de negócio na API têm testes de integração
* Novos endpoints validam input inválido e roles
* Fluxo principal testado manualmente (Admin e/ou MAUI)
* Contratos atualizados em `TeacherApp.Contracts` quando necessário
* Nenhum bug Critical ou High em aberto

### CI atual

* **TeacherApp:** `.github/workflows/ci.yml` faz build do `TeacherApp.Admin` apenas.
* **TeacherAppApi:** sem workflow CI no repositório (testes executados localmente).
* Build MAUI (Android/iOS) não está no CI — validar localmente antes de release.

## Casos de teste — API

Todo endpoint novo deve cobrir:

* Request válido (sucesso)
* Request inválido (400)
* Campos obrigatórios em falta
* Não autenticado (401)
* Role incorreta (403) — ex.: `Student` em rota `Admin`
* Não encontrado (404)
* Persistência na base de dados
* Contrato de resposta alinhado com `TeacherApp.Contracts`

Exemplos de testes existentes: `AdminSetStudentActiveIntegrationTests`, `ExerciseSubmitIntegrationTests`, `CatalogIntegrationTests`.

## Casos de teste — MAUI

Toda tela nova deve ser validada manualmente para:

* Carregamento inicial (`LoadCommand`, `OnAppearing`)
* Dados carregados com sucesso
* Estado vazio
* Erro de API / timeout / offline
* Navegação para frente e voltar (`Shell`)
* Proteção contra duplo clique (`IsBusy`)
* Risco de memory leak (`ICleanup`, eventos, áudio, `CancellationTokenSource`)
* Comportamento dos commands do ViewModel
* Navegação repetida ida/volta (stress de memória)

## Casos de teste — Blazor Admin

Toda página nova deve ser validada manualmente para:

* Render inicial e `[Authorize]`
* Estado de loading (`MudProgressLinear`)
* Estado vazio
* Validação de formulário
* Save com sucesso e com falha
* Confirmação antes de ações destrutivas (`MudDialog`)
* Utilizador não autorizado redirecionado
* Toggle/switch reverte estado se API falhar
* Layout responsivo e legibilidade em tema claro/escuro

## Fluxos críticos (smoke / regressão)

### Mobile (aluno)

1. Login → Home → Módulo → Lição (áudio) → Exercícios → Results
2. Exercícios finais do módulo
3. Perfil → alterar senha → logout
4. Aluno inativo não consegue login

### Admin

1. Login (apenas `Admin`) → Dashboard
2. CRUD módulos / lições / exercícios
3. Upload de media
4. Alunos: listar → detalhes → ativar/desativar acesso
5. Exercícios finais

### API

1. Auth JWT (login, change password)
2. Roles e rotas admin vs aluno
3. Contratos e migrations
4. Media S3 (quando configurado)

## Testes de regressão

Obrigatórios quando alterar:

* Login e JWT
* Roles e permissões (`Admin` / `Student`)
* Flag `IsActive` do aluno
* Fluxo lição → exercício → resultados
* Correção de exercícios e progresso
* Exercícios finais
* Contratos em `TeacherApp.Contracts`
* Migrations EF Core
* Upload e playback de media
* Navegação Shell (MAUI)
* Cadastro de aluno (CPF, email único)

## Ambientes

| Ambiente | API | Admin |
|----------|-----|-------|
| Local | `http://localhost:5092` | `appsettings.json` → `Api:BaseUrl` |
| Produção | VPS | Ver `docs/vps/connection.md` |

Após deploy na VPS, confirmar que a API inclui os endpoints esperados (ex.: Swagger em `/swagger`).

## Formato de bug report

```md
# Bug Report

## Title

## Severity
Critical / High / Medium / Low

## Environment
Local / Development / Staging / Production

## Area
API / Admin / Mobile / Database / Infrastructure / Contracts

## Repository
TeacherApp / TeacherAppApi / Both

## Steps to Reproduce

## Expected Result

## Actual Result

## Evidence
Screenshot, log, stack trace, response body, or video.

## Suspected Cause

## Suggested Fix

## Regression Test Needed
Yes / No
```

## Definition of Done

Uma tarefa está concluída quando:

* Código implementado no repositório correto
* Code review feito
* Build passa (Admin/App e/ou API)
* Testes de integração relevantes passam (API)
* Fluxo principal validado manualmente
* Documentação atualizada quando necessário
* Contratos sincronizados se houve alteração de API
* Nenhum bug Critical/High em aberto

## Documentação relacionada

* `.cursor/rules/qa-agent.mdc` — regras do agente QA no Cursor
* `.cursor/skills/qa-agent/SKILL.md` — skill de QA
* `docs/backend/seguranca.md` — roles e JWT
* `docs/fundamentos/testes.md` — convenção de nomes
* `maui-boas-praticas-memoria.md` — memória no MAUI
* `docs/arquitetura/contracts.md` — contratos partilhados
