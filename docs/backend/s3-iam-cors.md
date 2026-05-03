# S3 — IAM, CORS e acesso privado

Use este guia depois de criar o bucket. **Não commite access keys** no repositório.

## Block Public Access (recomendado)

No bucket S3, mantenha **Block all public access** ativado. O conteúdo fica acessível apenas via:

- URLs pré-assinadas geradas pela API, ou
- Identidades AWS com política explícita no bucket.

## Política IAM mínima (API)

Anexe esta política ao utilizador IAM ou à role assumida pela API (substitua `BUCKET_NAME` e, se usar prefixo fixo, ajuste o `Resource`).

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Sid": "TeacherAppS3Media",
      "Effect": "Allow",
      "Action": [
        "s3:PutObject",
        "s3:GetObject",
        "s3:DeleteObject"
      ],
      "Resource": "arn:aws:s3:::BUCKET_NAME/*"
    }
  ]
}
```

Se o bucket usar **SSE-KMS** com CMK própria, adicione permissões `kms:Decrypt` e `kms:GenerateDataKey` no CMK usado pelo bucket.

**Metadata após upload:** a API usa `GetObjectMetadata` (equivalente IAM a **`s3:GetObject`** no objeto). Não é obrigatório declarar `s3:HeadObject` na política se já tiver `s3:GetObject` no mesmo `Resource` `arn:aws:s3:::BUCKET/*`.

## CORS no bucket (admin web / Blazor)

| Cenário | Precisa de CORS no bucket? |
|---------|----------------------------|
| **MAUI** (HTTP nativo) a fazer `PUT` ao URL pré-assinado do S3 | Normalmente **não**. |
| **Blazor / browser** a fazer `PUT` para `*.amazonaws.com` | **Sim** — configure CORS. |
| Só a API faz `PutObject` (upload multipart no servidor) | CORS no bucket para PUT do **browser** não se aplica ao mesmo fluxo; o fluxo presigned envia o browser/app direto ao S3. |

Se o browser enviar **PUT** direto ao hostname do S3, configure CORS no bucket. Exemplo (ajuste `AllowedOrigins` aos domínios reais; use `http://localhost:PORT` só em desenvolvimento):

```json
[
  {
    "AllowedHeaders": ["*"],
    "AllowedMethods": ["GET", "PUT", "HEAD"],
    "AllowedOrigins": ["https://admin.example.com", "http://localhost:5000"],
    "ExposeHeaders": ["ETag", "x-amz-request-id"],
    "MaxAgeSeconds": 3000
  }
]
```

Clientes **MAUI** (nativos) normalmente não exigem CORS no S3; o CORS continua útil para o admin Blazor ou testes no browser.

## Credenciais em desenvolvimento

Defina variáveis de ambiente (ou perfil `~/.aws/credentials`):

- `AWS_ACCESS_KEY_ID`
- `AWS_SECRET_ACCESS_KEY`
- `AWS_REGION` (opcional se `Media:S3:Region` estiver no appsettings)

## Produção

Prefira **IAM role** associada ao serviço (ECS task role, EC2 instance profile, Azure/AWS federated identity, etc.) em vez de access keys de longa duração.
