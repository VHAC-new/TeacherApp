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

## Banco (tabelas)

* Nome no singular
* PascalCase

Ex:

* `User`
* `Lesson`
