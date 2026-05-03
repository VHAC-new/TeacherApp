# API — endpoints (referência)

Os exemplos abaixo usam o prefixo **[api-versionamento.md](api-versionamento.md)** `/api/v1/`.
Todos os endpoints (exceto login) exigem JWT Bearer. Rotas `/admin/*` exigem role `Admin`.

---

## Auth

* `POST /api/v1/auth/login` — login, retorna `accessToken`
* `GET /api/v1/auth/me` — dados do utilizador autenticado

---

## Catálogo (qualquer utilizador autenticado)

* `GET /api/v1/modules` — lista módulos ordenados
* `GET /api/v1/modules/{id}/lessons` — lições de um módulo

---

## Exercícios — submissão (qualquer utilizador autenticado)

* `POST /api/v1/exercises/{id}/submit` — submete resposta a um exercício de lição
* `POST /api/v1/final-exercises/{id}/submit` — submete resposta a um exercício final de módulo

---

## Progresso (qualquer utilizador autenticado)

* `GET /api/v1/progress` — progresso geral do utilizador (todos os módulos)
* `GET /api/v1/progress/modules/{moduleId}` — progresso num módulo específico

---

## Mídia (leitura — qualquer utilizador autenticado)

* `GET /api/v1/media/{id}/playback-url` — URL temporária (presigned GET) para reprodução quando **S3** está configurado (`Media:S3:Bucket`)
* `GET /api/v1/media/{id}` — stream do ficheiro quando **não** há S3 (modo disco local); com S3 ativo, usar `playback-url`

---

## Admin

* `GET/POST/PUT/DELETE /api/v1/admin/modules`
* `GET/POST/PUT/DELETE /api/v1/admin/lessons` (`?moduleId=` no GET)
* `GET/POST/PUT/DELETE /api/v1/admin/exercises` (`?lessonId=` no GET)
* `GET/POST/PUT/DELETE /api/v1/admin/final-exercises` (`?moduleId=` no GET)
* `GET /api/v1/admin/media` — listar ficheiros (inclui `uploadCompleted`)
* `POST /api/v1/admin/media` — upload multipart (**apenas** se S3 **não** estiver configurado)
* `POST /api/v1/admin/media/upload-url` — inicia upload presigned para S3 (corpo: `fileName`, `contentType`)
* `POST /api/v1/admin/media/{id}/complete` — confirma upload no S3 após o `PUT` ao bucket
* `DELETE /api/v1/admin/media/{id}` — remove metadados e objeto (S3 ou disco)

(Detalhe de verbos e corpos: documentar junto ao OpenAPI em desenvolvimento — ver [operacional-swagger.md](operacional-swagger.md).)
