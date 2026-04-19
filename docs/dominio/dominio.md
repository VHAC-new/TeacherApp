# 📌 Domínio e Regras de Negócio

## Entidades principais

* Users
* Modules
* Lessons
* Exercises
* ExerciseAnswers
* StudentLessonProgress
* StudentExerciseAttempts
* MediaFiles

---

## 📦 MediaFiles (IMPORTANTE)

```text
media_files:
 - id
 - file_name
 - file_key
 - content_type
 - size_bytes
 - duration_seconds
 - uploaded_by_user_id
 - created_at
 - is_active
```

---

## Regras importantes

* Lição pode existir como rascunho
* Exclusão lógica é preferível
* OrderIndex define ordenação
* Áudio não pode ser removido se estiver em uso
* Validação de resposta ignora:

  * maiúsculas/minúsculas
  * espaços extras
