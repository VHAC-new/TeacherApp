# S3 — smoke test (presigned)

Pré-requisitos: variáveis de ambiente e `Media:S3` conforme [s3-environment-checklist.md](s3-environment-checklist.md); migration `UploadCompleted` aplicada na BD; API em `http://localhost:5092` (ou a URL que usar).

## 1. Token admin

`POST /api/v1/auth/login` com o admin. Copie `accessToken`.

No Swagger: **Authorize** → Bearer `{token}`.

## 2. Iniciar upload

`POST /api/v1/admin/media/upload-url`

Corpo JSON exemplo:

```json
{
  "fileName": "test.mp3",
  "contentType": "audio/mpeg"
}
```

Guarde `mediaId`, `uploadUrl` e `expiresAtUtc`.

## 3. PUT para o S3

O cliente tem de enviar o **mesmo** `Content-Type` que usou no passo 2 (ex. `audio/mpeg`).

### PowerShell (ficheiro pequeno de teste)

```powershell
$bytes = [byte[]](@(0xFF,0xFB,0x90,0x00) + (New-Object byte[] 1024))  # corpo mínimo de exemplo; substitua por bytes reais de MP3 para testes reais
$hdr = @{ "Content-Type" = "audio/mpeg" }
Invoke-WebRequest -Uri $uploadUrl -Method PUT -Headers $hdr -Body $bytes
```

Se o PUT falhar com **403**, verifique política IAM, região e se o URL não expirou.

## 4. Completar na API

`POST /api/v1/admin/media/{mediaId}/complete`

Substitua `{mediaId}` pelo valor devolvido no passo 2. A resposta deve incluir `uploadCompleted: true` e `sizeBytes` > 0.

## 5. URL de reprodução

Com o **mesmo** token (ou outro utilizador autenticado):

`GET /api/v1/media/{mediaId}/playback-url`

Abra o campo `url` no browser ou num cliente de áudio. O URL expira em `expiresAtUtc`.

## Erros frequentes

| Sintoma | Causa provável |
|---------|----------------|
| 400 no `upload-url` com S3 | Bucket vazio na config ou região em falta. |
| 403 no PUT ao S3 | Política IAM ou ARN do bucket incorreto; URL expirado. |
| 404 no `complete` | PUT ainda não visível no bucket; Content-Type do PUT diferente do da assinatura. |
| CORS no browser | Configurar CORS no bucket ([s3-iam-cors.md](s3-iam-cors.md)). |
