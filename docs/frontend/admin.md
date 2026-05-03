# 📌 Painel Admin (Blazor)

Hosting e modelo **Blazor Server**: [admin-hosting.md](admin-hosting.md).

---

## UI: MudBlazor

O painel **TeacherApp.Admin** utiliza **[MudBlazor](https://mudblazor.com/)** como biblioteca de componentes (pacotes `MudBlazor` e serviços MudBlazor registados em `Program.cs`, por exemplo `AddMudServices()`).

* Preferir componentes Mud (`MudTable`, `MudForm`, `MudDialog`, `MudTextField`, etc.) a markup Bootstrap solto, para consistência e acessibilidade.
* Temas visuais (paleta, tipografia) devem passar por **`MudTheme`** / **`MudThemeProvider`** na raiz da app quando a equipa integrar o pacote no layout principal.

---

## Suporte para tema Claro/Escuro

**Requisito:** o admin deve oferecer **tema claro e tema escuro**, com boa legibilidade em formulários, tabelas, modais e navegação.

**Diretrizes de implementação (MudBlazor):**

* Definir duas variantes de **`MudTheme`** (claro e escuro) com contraste adequado para texto, superfícies e estados (erro, aviso, sucesso).
* Envolver a árvore de componentes com **`MudThemeProvider`** e alternar `IsDarkMode` (ou equivalente) conforme a escolha do utilizador ou a **preferência do sistema** (`prefers-color-scheme`), quando suportado no browser.
* **Persistência opcional** da escolha (ex.: `localStorage` ou cookie leve) para manter o tema entre sessões no mesmo browser.
* Evitar cores “hardcoded” fora do tema; alinhar componentes custom ao tokens/paleta Mud.

---

## Responsabilidades

* CRUD módulos
* CRUD lições
* CRUD exercícios
* Upload de áudio
* Publicação de conteúdo

---

## Estrutura

```text
Pages
Components
Services
Models
State
```

---

## Fluxo

Login → Dashboard → Modules → Lessons → Exercises → Upload
