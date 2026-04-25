# API — estrutura em camadas

## Objetivo

Descrever apenas a **organização de pastas/projetos** da `TeacherApp.Api`, sem listar endpoints (ver [api-endpoints.md](api-endpoints.md)) nem versionamento (ver [api-versionamento.md](api-versionamento.md)).

---

## Camadas internas (exemplo)

```text
Controllers
Application
Domain
Infrastructure
Common
```

---

## Contratos HTTP (DTOs)

Os tipos partilhados com MAUI e Blazor (**DTOs**, **Request**, **Response**, **enums**) vivem no projeto **TeacherApp.Contracts**, referenciado pela API — **não duplicar** esses tipos dentro da pasta da API.

A API mapeia entre entidades de domínio e os tipos em `TeacherApp.Contracts`. Ver [contracts.md](../arquitetura/contracts.md).

---

## Controllers

* Recebem pedidos HTTP
* Delegam em serviços da camada Application
* Sem regra de negócio pesada (ver [padrao-services.md](padrao-services.md) e [boas-praticas.md](../fundamentos/boas-praticas.md))

---

## Controllers previstos (nomes)

* `AuthController`
* `ModulesController`
* `LessonsController`
* `ExercisesController`
* `ProgressController`
* `AudioController`
