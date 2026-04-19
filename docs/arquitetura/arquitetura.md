# 📌 Arquitetura do Projeto - App de Inglês

## 🎯 Visão geral

Sistema composto por:

* App mobile (aluno) → .NET MAUI
* Painel admin (professor) → Blazor Web
* API → ASP.NET Core
* Banco → PostgreSQL
* Storage → S3

---

## 🧱 Estrutura da solution

```text
TeacherApp.sln
 ├─ TeacherApp.App
 ├─ TeacherApp.Admin
 ├─ TeacherApp.Api
 └─ TeacherApp.Tests
```

---

## 🔄 Fluxo do sistema

Aluno:
App MAUI → API → Banco / Storage

Professor:
Blazor → API → Banco / Storage

---

## 🧠 Decisões importantes

* API centraliza toda regra de negócio
* App não acessa banco diretamente
* Admin separado do app
* Áudios no S3 (não no banco)
* EF Core + Migrations
* JWT + Roles

---

## 📚 Documentação

* dominio.md
* api.md
* banco.md
* admin.md
* app-mobile.md
* seguranca.md
* storage.md
* boas-praticas.md
