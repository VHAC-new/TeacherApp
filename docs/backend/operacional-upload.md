# Operacional — upload de ficheiros

## Limites

* **Tamanho máximo** sugerido: **10 MB** por pedido (ajustar em configuração da API e validação explícita).
* **Tipo**: aceitar apenas **áudio** (lista de content-types permitidos, ex.: `audio/mpeg`, `audio/wav`; alinhar com o domínio e com [storage.md](storage.md)).

---

## Onde se aplica

* Endpoints relacionados com **media** / **áudio** (ex.: fluxo associado a `AudioController` e rotas `/api/v1/admin/media`).

---

## Segurança

* Validação de extensão e MIME **no servidor** (não confiar apenas no cliente).
* Política de auth/roles em [seguranca.md](seguranca.md).
* Armazenamento em S3 conforme [storage.md](storage.md).
