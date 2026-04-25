# Operacional — logging

## Objetivo

**Logging estruturado** na API (e nos hosts, quando aplicável), para diagnóstico em desenvolvimento e pesquisa em produção.

---

## Diretriz

* Preferir **Serilog** (ou equivalente acordado) com sinks configuráveis por ambiente (consola em dev, ficheiro ou agregador em prod).
* Incluir propriedades úteis: `RequestId`, `UserId` (quando autenticado), duração de pedidos críticos.

---

## Níveis

* **Development**: nível mais verboso conforme necessidade
* **Production**: Information ou superior; evitar dados sensíveis (passwords, tokens completos)
