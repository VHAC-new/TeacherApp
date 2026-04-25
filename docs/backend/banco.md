# 📌 Banco de Dados

## Conexão via json criando um appsettings

## Tecnologia

* PostgreSQL
* Entity Framework Core

---

## Estratégia

* ORM com EF Core
* Versionamento com Migrations

---

## Comandos

Criar migration:
dotnet ef migrations add Nome

Atualizar banco:
dotnet ef database update

---

## Estrutura

```text
Data
 ├─ AppDbContext
 ├─ Mappings
 └─ Migrations
```

---

## Nomenclatura (PostgreSQL)

* Tabelas e colunas em **snake_case** (convenção habitual em PostgreSQL).
* Em C#, as entidades EF podem usar PascalCase; o mapeamento para colunas `snake_case` faz-se em **Fluent API** ou convenções do modelo.

Exemplos de nomes de tabela: `user`, `lesson`, `media_file`.

---

## Boas práticas

* Não acessar DbContext no controller
* Usar Services/Repositories
* Separar entidades de DTOs (DTOs partilhados em `TeacherApp.Contracts` — ver [contracts.md](../arquitetura/contracts.md))
