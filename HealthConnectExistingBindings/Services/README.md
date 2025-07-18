# HealthConnect Service Implementation

Ce projet implémente une architecture de service de santé multi-plateforme pour .NET MAUI avec des implémentations spécifiques pour Android (Health Connect) et iOS (HealthKit).

## Architecture

### Interface IHealthService
L'interface `IHealthService` définit les méthodes communes pour interagir avec les services de santé sur toutes les plateformes :

- `InitializeAsync()` - Initialise le client de service de santé
- `HasAllPermissionsAsync()` - Vérifie si toutes les permissions nécessaires sont accordées
- `RequestPermissionsAsync()` - Demande les permissions à l'utilisateur
- `ReadStepsTodayAsync()` - Lit les données de pas pour aujourd'hui
- `AddStepsTodayAsync(int steps)` - Ajoute des données de pas
- `CheckSdkStatus()` - Vérifie le statut du SDK de santé

### Implémentations spécifiques

#### Android - AndroidHealthService
- Utilise Health Connect API
- Gère les coroutines Kotlin via des continuations
- Implémente la gestion des permissions Android spécifiques
- Localisation : `Platforms/Android/AndroidHealthService.cs`

#### iOS - iOSHealthService  
- Utilise HealthKit API
- Gère les permissions iOS spécifiques
- Localisation : `Platforms/iOS/iOSHealthService.cs`

### Factory Pattern
`HealthServiceFactory` crée automatiquement la bonne implémentation selon la plateforme courante.

## Structure des fichiers

```
Services/
├── IHealthService.cs              # Interface commune
├── HealthServiceFactory.cs        # Factory pour création d'instances
└── README.md                      # Documentation

Platforms/
├── Android/
│   ├── AndroidHealthService.cs    # Implémentation Android
│   ├── Continuation.cs           # Helper pour coroutines Kotlin
│   ├── test.cs                   # Wrapper pour StepsRecord
│   └── PermissionResultCallback.cs
└── iOS/
    └── iOSHealthService.cs        # Implémentation iOS

ViewModels/
└── HealthViewModel.cs             # ViewModel exemple

Pages/
├── HealthPage.xaml               # Page de test UI
└── HealthPage.xaml.cs            # Code-behind
```

## Configuration et utilisation

### 1. Configuration dans MauiProgram.cs
```csharp
// Enregistrement du service de santé
builder.Services.AddSingleton<IHealthService>(serviceProvider =>
{
#if ANDROID
    var activity = Platform.CurrentActivity ?? throw new InvalidOperationException("Current activity is null");
    return HealthServiceFactory.CreateHealthService(activity);
#else
    return HealthServiceFactory.CreateHealthService();
#endif
});
```

### 2. Injection dans un ViewModel
```csharp
public class HealthViewModel : INotifyPropertyChanged
{
    private readonly IHealthService _healthService;

    public HealthViewModel(IHealthService healthService)
    {
        _healthService = healthService;
    }
}
```

### 3. Utilisation dans le code
```csharp
// Initialiser le service
await _healthService.InitializeAsync();

// Vérifier les permissions
var hasPermissions = await _healthService.HasAllPermissionsAsync();

// Demander les permissions si nécessaire
if (!hasPermissions)
{
    await _healthService.RequestPermissionsAsync();
}

// Lire les données de pas
var steps = await _healthService.ReadStepsTodayAsync();

// Ajouter des pas
await _healthService.AddStepsTodayAsync(1000);
```

## Fonctionnalités spécifiques par plateforme

### Android (Health Connect)
- Gestion des permissions dynamiques
- Support des coroutines Kotlin
- Lecture/écriture des données de pas
- Vérification du statut du SDK Health Connect

### iOS (HealthKit)
- Gestion des autorisations HealthKit
- Support des requêtes statistiques
- Lecture/écriture des données de pas
- Vérification de la disponibilité de HealthKit

## Page de test
Une page de test complète (`HealthPage`) est fournie avec :
- Interface utilisateur pour tester toutes les fonctionnalités
- Affichage du statut et des permissions
- Boutons pour initialiser, demander des permissions, lire et ajouter des pas
- Instructions d'utilisation

## Notes importantes

1. **Permissions** : Assurez-vous que les permissions appropriées sont configurées dans les manifestes des plateformes
2. **Async/Await** : Toutes les opérations sont asynchrones pour une meilleure performance
3. **Gestion d'erreurs** : Chaque méthode inclut une gestion d'erreurs appropriée
4. **Thread-safe** : Les services peuvent être utilisés de manière sécurisée dans des contextes multi-threadés

## Extension future
Cette architecture permet facilement d'ajouter :
- Support pour d'autres types de données de santé (rythme cardiaque, sommeil, etc.)
- Nouvelles plateformes (Windows, macOS)
- Fonctionnalités avancées comme la synchronisation en temps réel
