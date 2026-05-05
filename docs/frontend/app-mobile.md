# 📌 App Mobile Android e IOS (MAUI)

## Arquitetura

MVVM — detalhes, CommunityToolkit e fluxo View → ViewModel → Service em [app-mobile-mvvm.md](app-mobile-mvvm.md).

---

## Suporte para tema Claro/Escuro

**Requisito:** nas plataformas **Android** e **iOS** , a app deve respeitar **tema claro e tema escuro**: seguir a preferência do sistema ou permitir um override explícito na aplicação, mantendo contraste e legibilidade em todas as telas.

**Abordagem recomendada (.NET MAUI):**

* **Recursos e XAML:** usar `AppThemeBinding` e recursos em [TeacherApp.App/Resources/Styles](../../TeacherApp.App/Resources/Styles/) (por exemplo `Colors.xaml`, `Styles.xaml`) para que cores e brushes mudem com o tema ativo, em vez de cores fixas nas views.
* **`UserAppTheme`:** quando for necessário forçar modo claro/escuro independentemente do SO, usar `Application.Current.UserAppTheme` (`AppTheme.Light`, `AppTheme.Dark` ou `AppTheme.Unspecified` para seguir o sistema).
* **Android:** o modo escuro do dispositivo é refletido no tema da app quando os recursos estão preparados para `Dark` / `Light`; validar em dispositivos ou emuladores com “tema escuro” ativado nas definições.
* **iOS:** garantir que a app declara suporte à aparência dinâmica conforme a [documentação Microsoft para MAUI / iOS](https://learn.microsoft.com/dotnet/maui/user-interface/system-theme-changes) (incluindo `Info.plist` e comportamento em modo escuro do sistema); testar no simulador com aparência clara/escura.

**Arquitetura:** o tema é responsabilidade da **camada de UI e recursos**, não dos ViewModels — ver [app-mobile-mvvm.md](app-mobile-mvvm.md) para manter lógica de ecrã separada de cores e estilos.

---

## Estrutura

```text
Views
ViewModels
Models
Services
Platforms
```

---

## Telas

* Login
* Home
* Module
* Lesson
* Exercise
* Result

---

## Responsabilidade

* UI
* consumo da API
* estado da tela
