# Fluxo de dados

## Aluno

```text
App MAUI → API → PostgreSQL / S3
```

O app mobile **não** acede ao banco nem ao S3 diretamente; apenas à API (e URLs devolvidas pela API para media).

---

## Professor (admin)

```text
Blazor Admin → API → PostgreSQL / S3
```

O painel admin segue o mesmo princípio: toda a orquestração e regras na API.

---

## Resumo

```text
View (MAUI ou Blazor) → serviços HTTP no cliente → API → dados (BD + storage)
```
