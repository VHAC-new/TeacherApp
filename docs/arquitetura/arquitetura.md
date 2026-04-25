# Arquitetura — índice

Documentação da arquitetura do TeacherApp (app de inglês), **um tema por página** para evitar misturar contextos.

---

## Visão e composição

* [Visão geral — componentes](arquitetura-visao-geral.md)
* [Solution e projetos](arquitetura-solution.md)
* [Fluxo de dados](arquitetura-fluxo-dados.md)
* [Contratos partilhados (TeacherApp.Contracts)](contracts.md)

---

## Frontends (detalhe)

* [App mobile — telas e pastas](../frontend/app-mobile.md)
* [App mobile — MVVM](../frontend/app-mobile-mvvm.md)
* [Admin Blazor](../frontend/admin.md)
* [Admin — hosting Blazor Server](../frontend/admin-hosting.md)

---

## Backend e domínio

* [API — índice](../backend/api.md)
* [Domínio](../dominio/dominio.md)
* [Banco de dados](../backend/banco.md)
* [Storage](../backend/storage.md)
* [Segurança](../backend/seguranca.md)
* [Padrão de services](../backend/padrao-services.md)

---

## Fundamentos

* [Boas práticas](../fundamentos/boas-praticas.md)
* [Padrão de nomenclatura (C#)](../fundamentos/padrao-nomenclatura.md)
* [Testes](../fundamentos/testes.md)
* [Git workflow](../fundamentos/git-workflow.md)

---

## Decisões resumidas

* API centraliza regra de negócio; clientes não acedem ao banco diretamente
* Áudios em **S3**, metadados na API/BD
* **EF Core** com migrations
* **JWT** e roles (ver segurança)
* MAUI com **MVVM Toolkit**; admin em **Blazor Server**
