# 📌 Padrão de Services

## 🎯 Objetivo

Separar responsabilidades corretamente.

---

## Controller

* Recebe request
* Chama service
* Retorna response

---

## Service

* Contém regra de negócio
* Valida dados
* Orquestra operações

---

## Repository

* Acesso ao banco
* Sem regra de negócio

---

## Fluxo padrão

Controller → Service → Repository → DB

---

## Exemplo

```csharp
public class LessonService : ILessonService
{
    public async Task CreateLesson(CreateLessonRequest request)
    {
        // regra de negócio aqui
    }
}
```
