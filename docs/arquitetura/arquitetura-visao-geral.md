# Visão geral — componentes do sistema

## Objetivo

Descrever **quem** compõe o TeacherApp e **com que tecnologia**, sem detalhes de pastas de código ou MVVM (isso fica em documentos dedicados).

---

## Componentes

* **App mobile (aluno)** — .NET MAUI
* **Painel admin (professor)** — Blazor Web, modelo **Blazor Server** (ver [admin-hosting.md](../frontend/admin-hosting.md))
* **API** — ASP.NET Core
* **Banco** — PostgreSQL (EF Core)
* **Storage de ficheiros** — S3 (áudios e media referenciados na API)

---

## Princípio

Toda a regra de negócio e persistência passa pela **API**. Clientes (MAUI e Blazor) consomem apenas HTTP + contratos partilhados ([contracts.md](contracts.md)).
