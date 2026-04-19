# 📌 Banco de Dados

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

## Boas práticas

* Não acessar DbContext no controller
* Usar Services/Repositories
* Separar entidades de DTOs
