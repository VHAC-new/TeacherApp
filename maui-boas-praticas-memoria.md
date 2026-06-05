# Boas práticas de memória para o app mobile .NET MAUI

> Documento de referência para orientar o desenvolvimento do aplicativo mobile Android/iOS em .NET MAUI, evitando crescimento de memória ao navegar entre telas, memory leaks, `System.OutOfMemoryException`, `Java.Lang.OutOfMemoryError` e outros problemas relacionados a retenção indevida de objetos.

---

## 1. Objetivo

Este documento define regras e padrões para o desenvolvimento do app mobile em **.NET MAUI**, com foco em:

- evitar vazamento de memória;
- evitar crescimento contínuo de memória ao navegar entre telas;
- reduzir risco de `System.OutOfMemoryException`;
- evitar retenção de `Page`, `ViewModel`, controles visuais e recursos pesados;
- padronizar navegação, ciclo de vida, injeção de dependência, imagens, áudio e listas;
- orientar a IA e os desenvolvedores durante a implementação.

O app mobile deve ser desenvolvido com foco em estabilidade, consumo moderado de memória e fácil manutenção.

---

## 2. Decisão arquitetural

Para este projeto, caso o app mobile seja feito em MAUI, a abordagem recomendada é:

```text
.NET MAUI XAML + MVVM
```

Evitar:

```text
MAUI Blazor Hybrid / BlazorWebView no app mobile do aluno
```

Motivo:

- o painel administrativo já usa Blazor;
- o app mobile do aluno deve ser mais leve;
- há relatos e issues envolvendo vazamento de memória com `BlazorWebView`;
- XAML + MVVM oferece mais controle sobre ciclo de vida, navegação e liberação de recursos.

---

## 3. Princípios obrigatórios

Durante o desenvolvimento, seguir estes princípios:

1. **Page não deve ser Singleton.**
2. **ViewModel não deve ser Singleton.**
3. **Serviços globais não devem guardar referência de Page ou ViewModel.**
4. **Eventos devem ser desinscritos quando a tela sair.**
5. **Timers, callbacks, tasks e subscriptions devem ser cancelados/limpos.**
6. **Imagens devem ser otimizadas antes de chegar ao app.**
7. **Áudios não devem ser carregados inteiros em memória sem necessidade.**
8. **Listas devem usar virtualização.**
9. **Navegação deve evitar empilhar telas desnecessariamente.**
10. **Telas pesadas devem implementar limpeza explícita de recursos.**
11. **Toda tela nova deve ser testada navegando ida/volta repetidas vezes.**
12. **O app deve ser testado em dispositivo físico, não apenas em emulador.**

---

## 4. O que costuma causar vazamento de memória no MAUI

### 4.1 Eventos não desinscritos

Exemplo ruim:

```csharp
public LessonPage()
{
    InitializeComponent();

    Connectivity.Current.ConnectivityChanged += OnConnectivityChanged;
}
```

Problema:

- `Connectivity.Current` é um serviço de vida longa;
- ele mantém referência para `OnConnectivityChanged`;
- o método mantém referência para a `Page`;
- a `Page` não é coletada pelo GC.

Exemplo correto:

```csharp
protected override void OnAppearing()
{
    base.OnAppearing();

    Connectivity.Current.ConnectivityChanged += OnConnectivityChanged;
}

protected override void OnDisappearing()
{
    Connectivity.Current.ConnectivityChanged -= OnConnectivityChanged;

    base.OnDisappearing();
}
```

---

### 4.2 Singleton segurando Page ou ViewModel

Exemplo ruim:

```csharp
public class NavigationStateService
{
    public Page? CurrentPage { get; set; }
    public LessonDetailsViewModel? CurrentViewModel { get; set; }
}
```

Problema:

- serviço singleton vive durante toda a aplicação;
- se ele guardar uma `Page` ou `ViewModel`, esses objetos não serão liberados.

Exemplo correto:

```csharp
public class NavigationStateService
{
    public Guid? CurrentLessonId { get; set; }
    public string? CurrentRoute { get; set; }
}
```

Guardar apenas dados leves:

```text
IDs
strings
enums
datas
estado simples
```

Nunca guardar:

```text
Page
View
ViewModel
BindingContext
CollectionView
Image
Stream
Player
Controle visual
```

---

### 4.3 Navegação empilhando telas sem necessidade

Evitar criar pilhas profundas de navegação sem controle.

Exemplo para resetar a área principal:

```csharp
await Shell.Current.GoToAsync("//home");
```

Exemplo para navegar para detalhe:

```csharp
await Shell.Current.GoToAsync(nameof(LessonDetailsPage), true, new Dictionary<string, object>
{
    ["LessonId"] = lessonId
});
```

Exemplo para voltar:

```csharp
await Shell.Current.GoToAsync("..");
```

Evitar recriar `AppShell` repetidamente, principalmente em login/logout:

```csharp
Application.Current.MainPage = new AppShell();
```

Esse tipo de troca deve ser usado com cuidado, porque pode manter referências antigas vivas se houver eventos, handlers ou serviços segurando objetos.

---

### 4.4 Tasks e CancellationTokenSource não cancelados

Exemplo ruim:

```csharp
public async Task LoadAsync()
{
    var result = await _api.GetLessonAsync(LessonId);
    Lesson = result;
}
```

Se o usuário sair da tela antes da resposta, a operação pode continuar e tentar atualizar uma ViewModel que já deveria ter sido descartada.

Exemplo melhor:

```csharp
private CancellationTokenSource? _cts;

public async Task LoadAsync()
{
    _cts?.Cancel();
    _cts?.Dispose();

    _cts = new CancellationTokenSource();

    try
    {
        var result = await _api.GetLessonAsync(LessonId, _cts.Token);
        Lesson = result;
    }
    catch (OperationCanceledException)
    {
        // Operação cancelada ao sair da tela.
    }
}
```

Na limpeza:

```csharp
public void Cleanup()
{
    _cts?.Cancel();
    _cts?.Dispose();
    _cts = null;
}
```

---

### 4.5 Timers não encerrados

Exemplo perigoso:

```csharp
Dispatcher.StartTimer(TimeSpan.FromSeconds(1), () =>
{
    CurrentTime = DateTime.Now;
    return true;
});
```

Problema:

- o timer continua executando;
- a lambda pode capturar `this`;
- a página ou ViewModel pode ficar presa.

Preferir controle explícito:

```csharp
private bool _isTimerRunning;

public void StartTimer()
{
    _isTimerRunning = true;

    Dispatcher.StartTimer(TimeSpan.FromSeconds(1), () =>
    {
        if (!_isTimerRunning)
            return false;

        CurrentTime = DateTime.Now;
        return true;
    });
}

public void StopTimer()
{
    _isTimerRunning = false;
}
```

Na limpeza da ViewModel:

```csharp
public void Cleanup()
{
    StopTimer();
}
```

---

## 5. Padrão de injeção de dependência

### 5.1 Registro recomendado

Pages e ViewModels devem ser `Transient`:

```csharp
builder.Services.AddTransient<LoginPage>();
builder.Services.AddTransient<HomePage>();
builder.Services.AddTransient<LessonsPage>();
builder.Services.AddTransient<LessonDetailsPage>();
builder.Services.AddTransient<ProfilePage>();

builder.Services.AddTransient<LoginViewModel>();
builder.Services.AddTransient<HomeViewModel>();
builder.Services.AddTransient<LessonsViewModel>();
builder.Services.AddTransient<LessonDetailsViewModel>();
builder.Services.AddTransient<ProfileViewModel>();
```

Serviços globais podem ser `Singleton`, desde que não guardem Page/ViewModel:

```csharp
builder.Services.AddSingleton<ITokenStorage, SecureTokenStorage>();
builder.Services.AddSingleton<IAuthService, AuthService>();
builder.Services.AddSingleton<IApiClient, ApiClient>();
builder.Services.AddSingleton<INavigationStateService, NavigationStateService>();
```

### 5.2 O que evitar

Evitar:

```csharp
builder.Services.AddSingleton<LessonDetailsPage>();
builder.Services.AddSingleton<LessonDetailsViewModel>();
```

Evitar também:

```csharp
public class AuthService
{
    public LoginPage? LoginPage { get; set; }
}
```

Serviço deve expor eventos, métodos ou estados leves, não guardar tela.

---

## 6. Padrão de Page + ViewModel

### 6.1 Interface de limpeza

Criar uma interface comum para ViewModels que precisam limpar recursos:

```csharp
public interface ICleanup
{
    void Cleanup();
}
```

### 6.2 Exemplo de ViewModel com limpeza

```csharp
public partial class LessonDetailsViewModel : ObservableObject, ICleanup
{
    private readonly ILessonApiClient _lessonApiClient;
    private CancellationTokenSource? _cts;

    [ObservableProperty]
    private LessonDetailsDto? lesson;

    public ObservableCollection<ExerciseDto> Exercises { get; } = new();

    public LessonDetailsViewModel(ILessonApiClient lessonApiClient)
    {
        _lessonApiClient = lessonApiClient;
    }

    public async Task LoadAsync(Guid lessonId)
    {
        _cts?.Cancel();
        _cts?.Dispose();

        _cts = new CancellationTokenSource();

        try
        {
            var result = await _lessonApiClient.GetLessonAsync(lessonId, _cts.Token);

            Lesson = result;

            Exercises.Clear();

            foreach (var exercise in result.Exercises)
                Exercises.Add(exercise);
        }
        catch (OperationCanceledException)
        {
            // Ignorado: tela saiu ou carregamento foi substituído.
        }
    }

    public void Cleanup()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;

        Exercises.Clear();
        Lesson = null;
    }
}
```

### 6.3 Exemplo de Page usando Cleanup

```csharp
public partial class LessonDetailsPage : ContentPage
{
    public LessonDetailsPage(LessonDetailsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnDisappearing()
    {
        if (BindingContext is ICleanup cleanup)
            cleanup.Cleanup();

        base.OnDisappearing();
    }
}
```

### 6.4 Limpeza agressiva apenas em casos necessários

Em telas que comprovadamente vazam memória, considerar:

```csharp
protected override void OnDisappearing()
{
    if (BindingContext is ICleanup cleanup)
        cleanup.Cleanup();

    BindingContext = null;

    base.OnDisappearing();
}
```

Atenção:

- não aplicar `BindingContext = null` em todas as telas sem testar;
- pode quebrar navegação, bindings, reaproveitamento visual ou comportamento esperado;
- usar apenas quando houver indício real de vazamento.

---

## 7. Listas e virtualização

Para listas novas, usar preferencialmente:

```text
CollectionView
```

Evitar `ListView` em telas novas, exceto se houver motivo específico.

Exemplo:

```xml
<CollectionView ItemsSource="{Binding Lessons}">
    <CollectionView.ItemTemplate>
        <DataTemplate>
            <Grid Padding="12">
                <Label Text="{Binding Title}" />
            </Grid>
        </DataTemplate>
    </CollectionView.ItemTemplate>
</CollectionView>
```

Boas práticas:

- evitar muitos controles aninhados;
- evitar imagens grandes dentro da lista;
- evitar binding complexo em cada item;
- usar paginação quando houver muitos registros;
- não carregar todas as aulas/exercícios de uma vez se o volume crescer.

Se for necessário usar `ListView`, usar reciclagem:

```xml
<ListView CachingStrategy="RecycleElement" />
```

---

## 8. Imagens

Imagens são uma das maiores fontes de consumo de memória em apps mobile.

### 8.1 Regra

Não enviar imagem gigante para o app apenas para exibir pequena na tela.

### 8.2 Recomendação para backend/S3

Gerar versões otimizadas:

```text
lesson-cover-320.webp
lesson-cover-720.webp
profile-avatar-128.webp
profile-avatar-256.webp
```

### 8.3 Evitar

```csharp
var bytes = await httpClient.GetByteArrayAsync(imageUrl);
ImageBytes = bytes;
```

### 8.4 Preferir

- URL direta para imagem otimizada;
- cache controlado;
- imagem no tamanho próximo ao exibido;
- formato leve, como WebP/JPEG otimizado;
- limpar referências em telas pesadas.

Exemplo:

```xml
<Image
    Source="{Binding CoverImageUrl}"
    Aspect="AspectFill"
    HeightRequest="180" />
```

---

## 9. Áudio

O app de inglês provavelmente usará áudios em aulas e exercícios. Áudio deve ser tratado como recurso pesado.

### 9.1 Evitar carregar áudio inteiro em memória

Evitar:

```csharp
byte[] audioBytes = await httpClient.GetByteArrayAsync(audioUrl);
```

Problema:

- aumenta consumo de memória;
- pode gerar pressão no GC;
- pode causar travamentos em dispositivos fracos.

### 9.2 Preferir streaming ou download controlado

Preferir:

```csharp
await using var stream = await httpClient.GetStreamAsync(audioUrl);
```

Ou:

```text
baixar arquivo para armazenamento local
tocar a partir do arquivo
remover arquivos antigos quando necessário
```

### 9.3 Regras para player de áudio

Ao sair da tela:

- pausar áudio;
- parar player se necessário;
- remover eventos do player;
- liberar stream;
- cancelar downloads em andamento;
- limpar referência do áudio atual.

Exemplo de limpeza:

```csharp
public void Cleanup()
{
    _cts?.Cancel();
    _cts?.Dispose();
    _cts = null;

    _audioPlayer?.Stop();
    _audioPlayer?.Dispose();
    _audioPlayer = null;

    CurrentAudio = null;
}
```

---

## 10. Eventos e mensagens

### 10.1 Eventos

Todo evento registrado deve ter ponto claro de remoção.

Se usar:

```csharp
SomeService.Changed += OnChanged;
```

Também deve existir:

```csharp
SomeService.Changed -= OnChanged;
```

### 10.2 WeakReferenceMessenger

Se usar `WeakReferenceMessenger` do CommunityToolkit.Mvvm, preferir registrar/desregistrar de forma explícita em ViewModels com ciclo de vida mais sensível.

Exemplo:

```csharp
public void RegisterMessages()
{
    WeakReferenceMessenger.Default.Register<LessonCompletedMessage>(this, OnLessonCompleted);
}

public void Cleanup()
{
    WeakReferenceMessenger.Default.UnregisterAll(this);
}
```

---

## 11. Shell e navegação

### 11.1 Regras

- usar rotas nomeadas;
- evitar recriar `AppShell` sem necessidade;
- evitar passar objetos grandes como parâmetro de navegação;
- passar IDs em vez de DTOs grandes;
- limpar estado ao fazer logout;
- evitar pilha profunda sem necessidade.

### 11.2 Passagem de parâmetro recomendada

Preferir:

```csharp
await Shell.Current.GoToAsync(nameof(LessonDetailsPage), new Dictionary<string, object>
{
    ["LessonId"] = lessonId
});
```

Evitar:

```csharp
await Shell.Current.GoToAsync(nameof(LessonDetailsPage), new Dictionary<string, object>
{
    ["Lesson"] = lessonDtoCompleto
});
```

Motivo:

- DTO grande pode ficar preso na navegação;
- imagens, listas e objetos relacionados podem ser mantidos em memória;
- buscar por ID é mais previsível.

---

## 12. Ciclo de vida das telas

### 12.1 OnAppearing

Usar para:

- carregar dados leves;
- iniciar escuta de eventos necessários;
- iniciar recursos temporários;
- atualizar estado visual.

### 12.2 OnDisappearing

Usar para:

- cancelar requests;
- parar áudio;
- parar timer;
- remover eventos;
- limpar collections grandes;
- chamar `Cleanup()` da ViewModel.

### 12.3 Exemplo

```csharp
protected override async void OnAppearing()
{
    base.OnAppearing();

    if (BindingContext is LessonDetailsViewModel vm)
        await vm.LoadAsync(LessonId);
}

protected override void OnDisappearing()
{
    if (BindingContext is ICleanup cleanup)
        cleanup.Cleanup();

    base.OnDisappearing();
}
```

Atenção:

- evitar lógica complexa em `async void`;
- tratar exceções;
- preferir comandos ou métodos controlados quando possível.

---

## 13. Recursos descartáveis

Toda classe que usar recursos descartáveis deve liberar corretamente.

Exemplos de recursos que precisam de atenção:

```text
Stream
HttpResponseMessage
CancellationTokenSource
MediaPlayer
AudioPlayer
Timer
FileStream
IDisposable
IAsyncDisposable
```

Exemplo:

```csharp
using var response = await _httpClient.GetAsync(url, cancellationToken);
response.EnsureSuccessStatusCode();

await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
```

---

## 14. DTOs e estado de tela

### 14.1 Evitar ViewModel gigante

A ViewModel não deve acumular dados de várias telas.

Ruim:

```csharp
public class AppViewModel
{
    public List<ModuleDto> Modules { get; set; }
    public List<LessonDto> Lessons { get; set; }
    public List<ExerciseDto> Exercises { get; set; }
    public LessonDetailsDto? CurrentLesson { get; set; }
    public StudentProgressDto? Progress { get; set; }
}
```

Melhor:

```text
HomeViewModel
LessonsViewModel
LessonDetailsViewModel
ExercisesViewModel
ProfileViewModel
```

Cada ViewModel deve carregar apenas o necessário para sua tela.

### 14.2 Passar IDs

Preferir:

```text
LessonId
ModuleId
ExerciseId
StudentId
```

Evitar passar objeto completo entre telas.

---

## 15. Checklist para cada tela nova

Antes de considerar uma tela pronta, verificar:

```text
[ ] Page registrada como Transient
[ ] ViewModel registrada como Transient
[ ] Nenhum service guarda referência da Page
[ ] Nenhum service guarda referência da ViewModel
[ ] Eventos são removidos no OnDisappearing ou Cleanup
[ ] Timers são parados
[ ] CancellationTokenSource é cancelado e descartado
[ ] Collections grandes são limpas quando necessário
[ ] Imagens são otimizadas
[ ] Áudio/vídeo não fica carregado em memória sem necessidade
[ ] Não passa DTO grande na navegação
[ ] Não usa static para armazenar Page/ViewModel
[ ] A tela foi testada com ida/volta repetidas vezes
[ ] A memória estabiliza após múltiplas navegações
```

---

## 16. Checklist específico para tela de aula

Para `LessonDetailsPage`:

```text
[ ] Carrega aula por LessonId
[ ] Não recebe DTO completo pela navegação
[ ] Usa imagem otimizada da aula
[ ] Não carrega áudio inteiro em byte[]
[ ] Cancela download/carregamento ao sair
[ ] Para o áudio no OnDisappearing se necessário
[ ] Remove eventos do player
[ ] Limpa lista de exercícios se for pesada
[ ] Não mantém referência da Page no player/service
```

---

## 17. Checklist específico para listas de aulas/módulos

```text
[ ] Usa CollectionView
[ ] Tem paginação ou carregamento incremental se necessário
[ ] Template visual é simples
[ ] Imagens são thumbnails
[ ] Não usa binding complexo demais em cada item
[ ] Não carrega detalhes completos de todas as aulas
[ ] Ao abrir detalhe, passa apenas LessonId
```

---

## 18. Checklist específico para login/logout

```text
[ ] Logout limpa token
[ ] Logout limpa dados sensíveis em memória
[ ] Logout cancela requests ativos
[ ] Logout não recria AppShell repetidamente sem necessidade
[ ] Logout reseta navegação de forma controlada
[ ] Nenhum singleton mantém dados do usuário anterior indevidamente
```

Exemplo:

```csharp
await _tokenStorage.ClearAsync();
_navigationState.Clear();

await Shell.Current.GoToAsync("//login");
```

---

## 19. Como testar vazamento de memória

### 19.1 Teste manual mínimo

Para cada tela importante:

```text
1. Abrir o app
2. Ir para a tela
3. Voltar
4. Repetir 20 a 50 vezes
5. Observar se a memória estabiliza ou cresce sem parar
```

Exemplo:

```text
Home -> Lessons -> LessonDetails -> Voltar
Home -> Lessons -> LessonDetails -> Voltar
Home -> Lessons -> LessonDetails -> Voltar
```

### 19.2 Teste com finalizador temporário

Pode ser usado apenas durante debug:

```csharp
public partial class LessonDetailsPage : ContentPage
{
    public LessonDetailsPage()
    {
        InitializeComponent();
    }

    ~LessonDetailsPage()
    {
        System.Diagnostics.Debug.WriteLine("LessonDetailsPage coletada pelo GC");
    }
}
```

Se a página nunca for coletada após várias navegações e coletas de GC em ambiente de teste, investigar retenções.

### 19.3 Forçar GC apenas em teste

Usar apenas para diagnóstico, nunca como solução de produção:

```csharp
GC.Collect();
GC.WaitForPendingFinalizers();
GC.Collect();
```

Não usar `GC.Collect()` como tentativa de “resolver” vazamento no app final.

---

## 20. Sinais de alerta

Investigar imediatamente se ocorrer:

```text
Memória cresce a cada navegação e nunca reduz
App fecha sozinho após uso prolongado
OutOfMemoryException
Java.Lang.OutOfMemoryError
Telas antigas continuam recebendo eventos
Áudio continua tocando após sair da tela
Requests continuam rodando após voltar
ViewModel antiga ainda recebe mensagens
Finalizador temporário da Page nunca é chamado
```

---

## 21. Ferramentas recomendadas

Usar conforme necessidade:

```text
Visual Studio Profiler
dotnet-gcdump
dotnet-trace
Android Studio Profiler
Xcode Instruments
MemoryToolkit.Maui
Logs de navegação e ciclo de vida
```

### 21.1 MemoryToolkit.Maui

Pode ser usado em ambiente de desenvolvimento/QA para detectar possíveis vazamentos em Views e BindingContexts.

Recomendação:

```text
Usar em DEBUG/QA para diagnóstico.
Não depender dele para compensar arquitetura ruim.
Avaliar com cuidado antes de usar em produção.
```

---

## 22. Regra para a IA durante desenvolvimento

Ao gerar código para o app MAUI, a IA deve seguir obrigatoriamente estas regras:

```text
1. Não criar Page como Singleton.
2. Não criar ViewModel como Singleton.
3. Não guardar Page/ViewModel em services.
4. Não usar static para armazenar estado de tela.
5. Passar IDs na navegação, não objetos grandes.
6. Usar CollectionView para listas.
7. Evitar BlazorWebView no app mobile.
8. Sempre limpar eventos, timers, mensagens e callbacks.
9. Usar CancellationToken em carregamentos assíncronos.
10. Implementar Cleanup em ViewModels com recursos pesados.
11. Não carregar imagem/áudio inteiro em memória sem necessidade.
12. Evitar templates visuais pesados em listas.
13. Testar navegação repetida em dispositivo físico.
14. Preferir arquitetura simples, previsível e descartável.
```

---


---

## 24. Exemplo de registro no MauiProgram

```csharp
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>();

        // Services
        builder.Services.AddSingleton<ITokenStorage, SecureTokenStorage>();
        builder.Services.AddSingleton<IAuthService, AuthService>();
        builder.Services.AddSingleton<ILessonApiClient, LessonApiClient>();
        builder.Services.AddSingleton<INavigationStateService, NavigationStateService>();

        // Pages
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<LessonsPage>();
        builder.Services.AddTransient<LessonDetailsPage>();
        builder.Services.AddTransient<ProfilePage>();

        // ViewModels
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<HomeViewModel>();
        builder.Services.AddTransient<LessonsViewModel>();
        builder.Services.AddTransient<LessonDetailsViewModel>();
        builder.Services.AddTransient<ProfileViewModel>();

        return builder.Build();
    }
}
```

---

## 25. Resumo da decisão

MAUI pode ser usado neste projeto, desde que o desenvolvimento siga disciplina de memória desde o início.

A maioria dos problemas pode ser reduzida com:

```text
boa arquitetura
DI correto
navegação controlada
limpeza de eventos
cancelamento de tasks
imagens otimizadas
áudio por stream/download controlado
testes em dispositivo físico
profiling periódico
```

Porém, MAUI também possui histórico de issues reais de memory leak em Shell, handlers e controles específicos. Portanto, o projeto deve considerar memória como um risco técnico a ser monitorado continuamente, não como algo para verificar apenas no final.

---

## 26. Regra final

Para cada nova tela implementada, responder antes de concluir:

```text
Essa tela pode ser descartada sem deixar Page, ViewModel, evento, timer, request, áudio, imagem ou collection presa em memória?
```

Se a resposta não for claramente “sim”, a tela ainda não está pronta.
