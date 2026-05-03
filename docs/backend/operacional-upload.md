# Operacional — upload de ficheiros

## Limites

* **Tamanho máximo** sugerido: **10 MB** por pedido (ajustar em configuração da API e validação explícita).
* **Tipo**: aceitar apenas **áudio** (lista de content-types permitidos, ex.: `audio/mpeg`, `audio/wav`; alinhar com o domínio e com [storage.md](storage.md)).

---

## Onde se aplica

* Rotas `/api/v1/admin/media`: **multipart** (disco local) ou **`POST .../upload-url`** + **`POST .../{id}/complete`** (S3 presigned), conforme [storage.md](storage.md).

---

## Segurança

* Validação de extensão e MIME **no servidor** (não confiar apenas no cliente).
* Política de auth/roles em [seguranca.md](seguranca.md).
* Armazenamento em S3 conforme [storage.md](storage.md).
