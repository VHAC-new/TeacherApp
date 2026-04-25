# Operacional — health check

## Endpoint

* `GET /health`

Resposta adequada para **liveness/readiness** (ex.: HTTP 200 com corpo mínimo JSON ou texto, conforme padrão da equipa).

---

## Âmbito

* Mantém-se **fora** do prefixo versionado `/api/v1/` para facilitar configuração em balanceadores e Kubernetes.

---

## Implementação (ASP.NET Core)

Usar `MapHealthChecks` / middleware de health checks da stack Microsoft; alinhar com logging e ambiente em [operacional-logging.md](operacional-logging.md).
