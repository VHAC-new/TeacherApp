# 📌 API

## Estrutura

```text
Controllers
Application
Domain
Infrastructure
Contracts
Common
```

---

## Controllers principais

* AuthController
* ModulesController
* LessonsController
* ExercisesController
* ProgressController
* AudioController

---

## Endpoints exemplo

### Auth

POST /api/auth/login
GET /api/auth/me

### Modules

GET /api/modules

### Lessons

GET /api/modules/{id}/lessons

### Exercises

POST /api/exercises/{id}/submit

---

## Admin endpoints

/api/admin/modules
/api/admin/lessons
/api/admin/exercises
/api/admin/media
