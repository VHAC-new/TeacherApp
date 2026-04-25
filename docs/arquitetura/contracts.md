# TeacherApp.Contracts

## Objetivo

Um único projeto com **contratos** usados pela API e pelos clientes, evitando cópias divergentes de tipos de mensagem.

---

## Conteúdo do projeto

* **DTOs** de leitura/escrita partilhados
* Tipos com sufixo **Request** e **Response**
* **Enums** partilhados entre API, MAUI e Blazor

---

## Quem referencia

* `TeacherApp.Api`
* `TeacherApp.App` (MAUI)
* `TeacherApp.Admin` (Blazor)

---

## Regra

**Nunca duplicar** o mesmo contrato (DTO / Request / Response / enum) noutro projeto. Alterações de contrato fazem-se **apenas** em `TeacherApp.Contracts`.

---

## Nomenclatura

Seguir [padrao-nomenclatura.md](../fundamentos/padrao-nomenclatura.md) para nomes em C# (PascalCase em tipos públicos, sufixos Request/Response).
