# 📌 Git Workflow

## Branches principais

* `main` → produção
* `develop` → desenvolvimento

---

## Novas features

Sempre criar:

```bash
feature/nome-da-feature
```

Ex:

* `feature/login`
* `feature/lesson-crud`

---

## Correções

```bash
fix/nome-do-bug
```

---

## Commits

Padrão:

```bash
tipo: descrição
```

Ex:

* `feat: adiciona endpoint de login`
* `fix: corrige validação de resposta`

---

## Regra importante

Nunca commitar direto na `main`.
