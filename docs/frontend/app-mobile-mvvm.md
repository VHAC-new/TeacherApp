# App MAUI — MVVM e CommunityToolkit

## Padrão

O app utiliza **MVVM (Model-View-ViewModel)** com **CommunityToolkit.Mvvm**.

---

## Estrutura interna sugerida

```text
/App
 ├─ Views
 ├─ ViewModels
 ├─ Models
 ├─ Services
 └─ Resources
```

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
* Autenticação (JWT)
* Abstração de chamadas remotas

---

## MVVM Toolkit — recursos

* `ObservableObject` — notificação de alterações
* `[ObservableProperty]` — geração de propriedades
* `[RelayCommand]` — comandos

### Exemplo

```csharp
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
