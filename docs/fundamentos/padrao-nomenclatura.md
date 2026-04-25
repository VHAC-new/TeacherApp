# 📌 Padrão de Nomenclatura

## 🎯 Objetivo

Garantir clareza e consistência em todo o projeto.

---

## Classes

* PascalCase
* Nome sempre substantivo

Ex:

* `UserService`
* `LessonRepository`
* `CreateLessonRequest`

---

## Métodos

* PascalCase
* Começar com verbo

Ex:

* `GetLessons()`
* `CreateLesson()`
* `ValidateAnswer()`

---

## Variáveis

* camelCase

Ex:

* `userId`
* `lessonName`

---

## DTOs

Sempre usar sufixos:

* `Request`
* `Response`

Ex:

* `CreateLessonRequest`
* `LessonResponse`

---

## Interfaces

Prefixo `I`

Ex:

* `ILessonService`
* `IUserRepository`

---

## Base de dados (PostgreSQL)

Não documentar convenções de tabelas/colunas neste ficheiro — usar apenas **[banco.md](../backend/banco.md)** (**snake_case** no PostgreSQL).
