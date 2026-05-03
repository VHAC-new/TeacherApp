# S3 — checklist de ambiente (API)

Use esta lista antes de `dotnet run` ou de publicar a API. **Não** coloque access keys no `appsettings.json` commitado.

## Variáveis obrigatórias para a API arrancar

| Variável | Descrição |
|----------|-----------|
| `TEACHERAPP_JWT_SECRET` | Obrigatório (nome alternativo configurável em `Jwt:SecretEnvVarName`). |

## Variáveis AWS (utilizador IAM ou role)

| Variável | Quando |
|----------|--------|
| `AWS_ACCESS_KEY_ID` | Utilizador IAM com access key (dev / CI). |
| `AWS_SECRET_ACCESS_KEY` | Par da chave acima. |
| `AWS_REGION` | Opcional se `Media:S3:Region` estiver definido no `appsettings`. |

Em produção com **IAM role** (ECS, EC2, etc.), as duas primeiras podem estar ausentes; o SDK usa a role.

## Configuração em ficheiro (`Media:S3`)

Ver [`TeacherApp.Api/appsettings.json`](../../TeacherApp.Api/appsettings.json):

- `Media:S3:Bucket` — nome exato do bucket (deve coincidir com o ARN na política IAM).
- `Media:S3:Region` — ex.: `sa-east-1`.

### Alinhar política IAM ao `Media:S3:Bucket`

Na consola IAM, o `Resource` da política tem de ser:

`arn:aws:s3:::{NOME_DO_BUCKET}/*`

onde `{NOME_DO_BUCKET}` é **exactamente** o valor de `Media:S3:Bucket` (sem `/*` no nome do bucket, só no fim do ARN). Se o nome no JSON da política e o nome no `appsettings` divergirem um carácter, receberá **403** nos pedidos S3.

## PowerShell (sessão atual)

Substitua os valores e execute **na mesma janela** onde vai correr `dotnet run`:

```powershell
$env:TEACHERAPP_JWT_SECRET = "sua-chave-jwt-longa-para-hs256"
$env:AWS_ACCESS_KEY_ID = "AKIA..."
$env:AWS_SECRET_ACCESS_KEY = "..."
$env:AWS_REGION = "sa-east-1"
```

## Visual Studio / User Secrets (opcional)

Para não depender só do PowerShell:

```text
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=..." --project TeacherApp.Api
```

Chaves AWS **não** devem ir para user-secrets se o repositório for partilhado sem cuidado; prefira variáveis de ambiente da máquina ou do pipeline CI.

## Perfil `launchSettings.json` (Visual Studio / `dotnet run`)

O perfil `http` / `https` define `AWS_REGION=sa-east-1` para coincidir com buckets nessa região. Se o bucket estiver noutra região, altere esse valor ou sobrescreva com variável de ambiente na máquina.

## Verificação rápida

1. `echo $env:AWS_ACCESS_KEY_ID` (PowerShell) — não deve estar vazio em dev com IAM user.
2. Arranque da API sem exceção `Missing JWT secret` / `S3:Region`.
3. No log de arranque, confirmar a linha **S3 media storage enabled** com o nome do bucket esperado e o aviso sobre `AWS_ACCESS_KEY_ID` se aplicável (`Program.cs`).
4. Fluxo completo: [s3-smoke-test.md](s3-smoke-test.md).
