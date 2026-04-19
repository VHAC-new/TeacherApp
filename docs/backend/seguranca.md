# 📌 Segurança

## Autenticação

* JWT

---

## Roles

* Student
* Teacher
* Admin

---

## Proteção

```csharp
[Authorize(Roles = "Teacher,Admin")]
```

---

## Regras

* senha com hash
* validação de input
* upload com restrições
