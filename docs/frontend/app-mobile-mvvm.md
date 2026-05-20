# App MAUI — MVVM e CommunityToolkit

## Padrão

O app utiliza **MVVM (Model-View-ViewModel)** com **CommunityToolkit.Mvvm** e organização por **Feature Folders**.

---

## Estrutura (Feature Folders)

Cada funcionalidade fica em `Features/{NomeDaFeature}/`, com subpastas conforme necessidade:

```text
TeacherApp.App/
 ├─ Converters/              → conversores XAML partilhados
 ├─ Core/
 │   └─ Services/            → infra e serviços usados por várias features (HTTP, catálogo)
 └─ Features/
     ├─ Login/
     │   ├─ Services/
     │   ├─ ViewModels/
     │   └─ Views/
     ├─ Home/
     │   ├─ Components/       → UI reutilizável da feature (quando existir)
     │   ├─ Popups/           → popups/modais da feature (quando existir)
     │   ├─ Services/
     │   ├─ ViewModels/
     │   └─ Views/
     ├─ Module/
     │   ├─ ViewModels/
     │   └─ Views/
     ├─ Lesson/
     │   ├─ Services/
     │   ├─ ViewModels/
     │   └─ Views/
     └─ Exercise/
         ├─ Services/
         ├─ ViewModels/
         └─ Views/
```

**Regras:**

* Código usado por **uma** feature → pasta dentro dessa feature.
* Código usado por **várias** features → `Core/Services` (ou `Core/` para outros tipos no futuro).
* Namespaces seguem a pasta: `TeacherApp.App.Features.Home.ViewModels`, etc.
* Novas telas: criar pasta em `Features/{Feature}/` com `Views` e `ViewModels` (e `Services` se for exclusivo da feature).

---

## Views

* Pages e componentes XAML
* Binding com ViewModels
* Sem lógica de negócio

---

## ViewModels

* Herdam de `ObservableObject`
* `[ObservableProperty]` para propriedades reativas
* `[RelayCommand]` para ações
* Não acedem diretamente a banco ou infraestrutura

---

## Models

* Objetos alinhados aos contratos em **TeacherApp.Contracts** (ou projeções simples para UI)
* Sem lógica de negócio complexa no cliente

---

## Services

* Consumo da API (HTTP)
* Autenticação (JWT) na feature **Login**; token partilhado em **Core**
* Abstração de chamadas remotas

---

## MVVM Toolkit — recursos

* `ObservableObject` — notificação de alterações
* `[ObservableProperty]` — geração de propriedades
* `[RelayCommand]` — comandos

### Exemplo

```csharp
namespace TeacherApp.App.Features.Home.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    [ObservableProperty]
    private string title;

    [RelayCommand]
    private async Task LoadAsync()
    {
        // chamada de serviço
    }
}
```

---

## Fluxo interno

```text
View → ViewModel → Service → API
```

* ViewModel não conhece detalhes de UI além do necessário para comandos/bindings
* Services isolam acesso à API
* Nenhum acesso direto ao base de dados no app

---

## Ver também

* [app-mobile.md](app-mobile.md) — telas e responsabilidades gerais
