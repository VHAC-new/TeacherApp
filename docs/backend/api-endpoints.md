# API — endpoints (referência)

Os exemplos abaixo usam o prefixo **[api-versionamento.md](api-versionamento.md)** `/api/v1/`.

---

## Auth

* `POST /api/v1/auth/login`
* `GET /api/v1/auth/me`

---

## Módulos e conteúdo (aluno / rotas gerais)

* `GET /api/v1/modules`
* `GET /api/v1/modules/{id}/lessons`
* `POST /api/v1/exercises/{id}/submit`

---

## Admin

* `GET/POST/PUT/DELETE` conforme implementação em `/api/v1/admin/modules`
* `/api/v1/admin/lessons`
* `/api/v1/admin/exercises`
* `/api/v1/admin/media`

(Detalhe de verbos e corpos: documentar junto ao OpenAPI em desenvolvimento — ver [operacional-swagger.md](operacional-swagger.md).)
