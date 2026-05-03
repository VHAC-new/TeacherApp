# Storage de áudio e mídia

## Estratégia

* Ficheiros **não** ficam na base de dados; a tabela `MediaFile` guarda **metadados** e a **chave do objeto** (`StoragePath`).
* Em **produção** recomenda-se **Amazon S3** com bucket **privado**; a API gera **URLs pré-assinadas** para upload (PUT) e reprodução (GET).
* Sem S3 configurado, a API usa **disco local** (`Media:StoragePath` ou pasta `media-files` por defeito) — útil para desenvolvimento local.

## Configuração (API)

Defina em `appsettings` / variáveis de ambiente (ver [`appsettings.json`](../../TeacherApp.Api/appsettings.json) na secção `Media:S3`):

| Chave | Descrição |
|-------|-------------|
| `Media:S3:Bucket` | Nome do bucket. Se **vazio**, o modo local (multipart) está ativo. |
| `Media:S3:Region` | Região AWS (ex. `eu-west-1`). Obrigatória se `Bucket` estiver preenchido. |
| `Media:S3:KeyPrefix` | Prefixo das chaves (ex. `media`). |
| `Media:S3:UploadUrlExpirationMinutes` | Validade da URL de PUT (predefinição 15). |
| `Media:S3:GetUrlExpirationMinutes` | Validade da URL de GET para reprodução (predefinição 60). |

Credenciais: `AWS_ACCESS_KEY_ID` e `AWS_SECRET_ACCESS_KEY` em desenvolvimento, ou **IAM role** em produção. Não commitar segredos.

Guia de IAM, CORS e acesso privado: **[s3-iam-cors.md](s3-iam-cors.md)**.

Checklist de variáveis de ambiente: **[s3-environment-checklist.md](s3-environment-checklist.md)**.  
Smoke test presigned: **[s3-smoke-test.md](s3-smoke-test.md)**.

## Fluxo com S3 (presigned)

1. **Admin** — `POST /api/v1/admin/media/upload-url` com corpo `InitMediaUploadRequest` (`fileName`, `contentType`).  
   Resposta: `InitMediaUploadResponse` (`mediaId`, `uploadUrl`, `objectKey`, `expiresAtUtc`).
2. **Cliente** — `PUT` do ficheiro binário para `uploadUrl` (headers alinhados ao `Content-Type` usado na assinatura).
3. **Admin** — `POST /api/v1/admin/media/{mediaId}/complete`. A API faz `GetObjectMetadata` no S3, atualiza tamanho e marca o registo como concluído.
4. **Reprodução** — `GET /api/v1/media/{id}/playback-url` (JWT). Resposta: URL GET pré-assinada temporária.  
   O endpoint `GET /api/v1/media/{id}` (stream) **não** serve ficheiros quando o modo S3 está ativo; use sempre `playback-url` com o URL devolvido.

## Fluxo local (sem `Media:S3:Bucket`)

* `POST /api/v1/admin/media` — multipart, grava em disco.
* `GET /api/v1/media/{id}` — stream do ficheiro local.
* `GET /api/v1/media/{id}/playback-url` — devolve `400` indicando o uso do stream direto.

## Clientes (MAUI / Blazor)

* **MAUI**: após login, chamar `upload-url` → `HttpClient.PutAsync(uploadUrl, StreamContent)` → `complete` → para ouvir, `playback-url` e passar `Url` ao leitor de áudio (ex. `MediaSource` com URI).
* **Blazor (browser)**: o mesmo fluxo; configure **CORS no bucket S3** para a origem do admin (ver [s3-iam-cors.md](s3-iam-cors.md)), caso o `PUT` vá direto ao hostname do S3.

## Benefícios do S3

* Base de dados leve
* Melhor organização das chaves por prefixo
* Escalabilidade e custo de egresso previsíveis com políticas de lifecycle
