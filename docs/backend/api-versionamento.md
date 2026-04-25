# API — versionamento

## Prefixo da API pública

Todas as rotas REST sob contrato estável usam o prefixo:

```text
/api/v1/
```

Exemplos:

* `GET /api/v1/modules`
* `GET /api/v1/modules/{id}/lessons`
* `POST /api/v1/exercises/{id}/submit`

Rotas de **admin** seguem o mesmo prefixo de versão, por exemplo:

```text
/api/v1/admin/modules
```

(Ajustar nomes finais dos controladores à implementação, mantendo o prefixo `/api/v1/`.)

---

## Evolução (v2)

Quando existir alteração incompatível do contrato, introduzir `/api/v2/` em paralelo, com período de transição acordado pela equipa.

---

## Health check

O endpoint `GET /health` está definido em [operacional-health.md](operacional-health.md) — **fora** do prefixo `/api/v1/`, para simplificar probes de infraestrutura.
