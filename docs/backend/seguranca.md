# 📌 Segurança

## Autenticação

* JWT

---

## Roles (atual)

* **Admin** — acesso ao painel Blazor Admin e a `GET/POST /api/v1/admin/*`.
* **Student** — acesso às rotas de aluno da API e ao app mobile (`TeacherApp.App`); **sem** acesso a `/api/v1/admin/*` (403).

O cadastro de aluno no painel cria `User` com role `Student` e perfil `Student` para **login no mobile**, não para entrar no Admin.

---

## Proteção

```csharp
[Authorize(Roles = "Admin")]
```

Rotas admin na API usam apenas **Admin**. O login do painel Admin rejeita tokens cujo JWT não inclua a role `Admin` (ver `Login.razor`).

---

## Regras

* senha com hash
* validação de input
* upload com restrições (tamanho, tipo) — ver [operacional-upload.md](operacional-upload.md)
* criação de aluno: sem password no body; a API gera hash provisório; envio de senha por email (Amazon SES) é evolução futura
