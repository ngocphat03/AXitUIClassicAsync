# AXitUIClassicAsync

A Unity UI framework based on MVP (Model-View-Presenter) pattern with async/await support for managing screens and popups in Unity applications.

## Features

- **MVP Architecture**: Clean separation of concerns with Model, View, and Presenter pattern
- **Async/Await Support**: Built with UniTask for modern asynchronous programming
- **Screen Management**: Full-featured screen navigation system
- **Popup System**: Overlay popup management with stacking support
- **Dependency Injection Ready**: Compatible with Zenject and VContainer
- **Transition Animations**: Built-in UI transition support
- **Resource Management**: Supports both Resources and Addressables loading
- **Safe Area Support**: Mobile-friendly safe area handling

## Architecture Overview

The framework follows the MVP pattern where:
- **Model**: Contains data and business logic
- **View**: Contains UI components and visual elements
- **Presenter**: Handles user interactions and coordinates between Model and View

### Core Components

#### Screen System
**Definition**: A Screen is a main interface that can only be displayed one at a time. When a new screen is opened, it replaces the current screen. When a screen is closed, the previous screen is automatically restored.

Key characteristics:
- Only one screen can be active at a time
- Opening a new screen replaces the current one
- Closing a screen returns to the previous screen
- Manages the main navigation flow of the application

Components:
- `BaseScreenModel`: Data container for screens
- `BaseScreenView`: MonoBehaviour containing UI components
- `BaseScreenPresenter<TView, TModel>`: Logic handler for screen interactions
- `ScreenManager`: Manages screen lifecycle and navigation

#### Popup System
**Definition**: A Popup is an overlay interface that can be displayed on top of screens or other popups. Multiple popups can be open simultaneously and they stack on top of each other. Closing a popup only affects that specific popup and doesn't impact other screens or popups.

Key characteristics:
- Multiple popups can be open at the same time
- Popups stack on top of each other and the current screen
- Closing a popup only closes that specific popup
- Does not affect the underlying screen or other popups
- Used for dialogs, notifications, settings panels, etc.

Components:
- `BasePopupModel`: Data container for popups
- `BasePopupView`: MonoBehaviour containing popup UI components
- `BasePopupPresenter<TView, TModel>`: Logic handler for popup interactions

#### Transition System
**Definition**: The framework includes an automatic transition system that plays animations when opening or closing screens and popups.

Key characteristics:
- Automatically plays open/close animations using Timeline
- If no animation timeline is configured, screens and popups open/close instantly
- Supports custom transition animations through Timeline assets
- Smooth visual transitions enhance user experience
- Non-blocking async operations during transitions

## Installation

1. Import the package into your Unity project
2. Add the required dependencies:
   - UniTask (com.cysharp.unitask)
   - TextMeshPro (if using text components)

3. Optional dependencies for DI:
   - Zenject or VContainer (for dependency injection)
   - Addressables (for asset loading)

## Quick Start

### 1. Setup Scene

Simply add the `RootUI` prefab to your scene. The prefab contains all necessary components:

```
Scene
└── RootUI (prefab)
    ├── UICamera
    └── SimpleRootUI
        ├── UiOpen
        └── UiClose
```

**Steps:**
1. Drag the `RootUI` prefab from `Runtime/Prefabs/Components/` into your scene
2. That's it! The framework is ready to use

**Important Notes:**
- Most screens and popups are loaded dynamically from prefabs in the Resources folder
- Only use `[ViewInitInScene]` attribute for Views that are permanently placed in the Scene
- Views with `[ViewInitInScene]` should be placed under `UiOpen` in the Scene hierarchy

## Quick Creation Tools

The framework includes Unity Editor tools to quickly create screens and popups with proper MVP structure.

### Creating UI Scripts

1. **Right-click in Project window** on any folder
2. Go to **Create > AXit > UI > Create Classic UI Script**
3. **Enter the class name** (e.g., "MainMenu", "Settings", "Inventory")
4. **Select UI Mode**: Screen or Popup
5. Click **Create UI Script**

This will generate a complete script file with:
- Model class (`[ClassName]Model`)
- View class (`[ClassName]View`) 
- Presenter class (`[ClassName]Presenter`)
- All required overrides and proper inheritance

**Example**: Creating "MainMenu" Screen will generate:
```csharp
using AXitUnityTemplate.UI.Classic.Async;

public class MainMenuModel : BaseScreenModel
{
}

public class MainMenuView : BaseScreenView
{
}

public class MainMenuPresenter : BaseScreenPresenter<MainMenuView, MainMenuModel>
{
    public override string ScreenPath => nameof(MainMenuView);
    
    public override void Awake() { }
    public override void OnEnable() { }
    public override void OnDisable() { }
    public override void OnDestroy() { }
}
```

### Creating UI Prefabs

1. **Right-click in Project window** on any folder
2. Go to **Create > AXit > UI > Create Classic UI Prefab**
3. **Enter the prefab name** (should match your script name)
4. **Select UI Mode**: Screen or Popup
5. Click **Create UI Prefab**

This will create a prefab variant based on the framework's base templates:
- **Screen**: Creates prefab variant of `UITemplateScreenBase`
- **Popup**: Creates prefab variant of `UITemplatePopupBase`

**Recommended Workflow**:
1. Create the prefab first using the prefab creation tool
2. Create the matching script using the script creation tool
3. Open the prefab you just created in Prefab Mode
4. Attach your View script to the prefab's root GameObject
5. Design your UI layout within the prefab
6. The prefab is ready to be loaded by your Presenter

### 2. Create Your First Screen (Manual Method)

#### Model
```csharp
public class MainMenuModel : BaseScreenModel
{
    public string WelcomeMessage { get; set; } = "Welcome!";
}
```

#### View
```csharp
// Note: Only use [ViewInitInScene] for Views that are pre-placed in the Scene (in UiOpen),
// not for Views that are instantiated at runtime from prefabs
public class MainMenuView : BaseScreenView
{
    public Button playButton;
    public Button settingsButton;
    public TMP_Text welcomeText;
}
```

#### Presenter
```csharp
public class MainMenuPresenter : BaseScreenPresenter<MainMenuView, MainMenuModel>
{
    public override string ScreenPath => "Screens/MainMenuView";

    public override void Awake()
    {
        View.welcomeText.text = Model.WelcomeMessage;
    }

    public override void OnEnable()
    {
        View.playButton.onClick.AddListener(OnPlayClicked);
        View.settingsButton.onClick.AddListener(OnSettingsClicked);
    }

    public override void OnDisable()
    {
        View.playButton.onClick.RemoveListener(OnPlayClicked);
        View.settingsButton.onClick.RemoveListener(OnSettingsClicked);
    }

    private async void OnPlayClicked()
    {
        await ScreenManager.Instance.OpenScreen<GamePresenter, GameModel>();
    }

    private async void OnSettingsClicked()
    {
        await ScreenManager.Instance.OpenPopup<SettingsPresenter, SettingsModel>();
    }

    public override void OnDestroy() { }
}
```

### 3. Create Your First Popup

#### Model
```csharp
public class SettingsModel : BasePopupModel
{
    public float MasterVolume { get; set; } = 1.0f;
}
```

#### View
```csharp
public class SettingsView : BasePopupView
{
    public Slider volumeSlider;
    public Button closeButton;
}
```

#### Presenter
```csharp
public class SettingsPresenter : BasePopupPresenter<SettingsView, SettingsModel>
{
    public override string PopupPath => "Popups/SettingsView";

    public override void Awake()
    {
        View.volumeSlider.value = Model.MasterVolume;
    }

    public override void OnEnable()
    {
        View.volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        View.closeButton.onClick.AddListener(() => OnCloseView());
    }

    public override void OnDisable()
    {
        View.volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);
    }

    private void OnVolumeChanged(float value)
    {
        Model.MasterVolume = value;
        // Apply volume settings
    }

    public override void OnDestroy() { }
}
```

## API Reference

### ScreenManager

The main entry point for screen and popup management.

#### Methods

```csharp
// Open a new screen
await ScreenManager.Instance.OpenScreen<TPresenter, TModel>();
await ScreenManager.Instance.OpenScreen<TPresenter, TModel>(model);

// Open a popup
await ScreenManager.Instance.OpenPopup<TPresenter, TModel>();
await ScreenManager.Instance.OpenPopup<TPresenter, TModel>(model);

// Close current screen
ScreenManager.Instance.CloseCurrentScreen();

// Close specific popup
ScreenManager.Instance.ClosePopup<TPresenter>();

// Close all popups
ScreenManager.Instance.CloseAllPopups();
```

### Base Classes

#### BaseScreenPresenter<TView, TModel>

Required overrides:
- `string ScreenPath`: Path to the screen prefab (supports both Resources and Addressables loading)
- `void Awake()`: Called when screen is first created
- `void OnEnable()`: Called when screen becomes active
- `void OnDisable()`: Called when screen becomes inactive
- `void OnDestroy()`: Called when screen is destroyed

**Important Note**: To close a screen from within its Presenter, call `OnCloseView()` method, not `CloseView()`.

#### BasePopupPresenter<TView, TModel>

Required overrides:
- `string PopupPath`: Path to the popup prefab (supports both Resources and Addressables loading)
- `void Awake()`: Called when popup is first created
- `void OnEnable()`: Called when popup becomes active
- `void OnDisable()`: Called when popup becomes inactive
- `void OnDestroy()`: Called when popup is destroyed

**Important Note**: To close a popup from within its Presenter, call `OnCloseView()` method, not `CloseView()`.

### Attributes

#### ViewInitInSceneAttribute

Marks a View class that should be initialized within the Scene instead of being loaded from Resources or instantiated at runtime. This attribute is **only used for Views that are pre-placed in the Scene** (typically in the UiOpen parent), not for Views created dynamically from prefabs.

**Usage scenarios:**
- Views that are always visible (like HUD, main menu that's always in scene)
- Views that need to be pre-configured in the Scene
- Views that don't need to be loaded/unloaded dynamically

**Do NOT use for:**
- Views that are loaded from Resources folder at runtime
- Views that are instantiated from prefabs
- Views that need dynamic loading/unloading

```csharp
// Correct usage: For a View that exists in the Scene
[ViewInitInScene(typeof(HUDPresenter))]
public class HUDView : BaseScreenView
{
    // This View is pre-placed in the Scene, not loaded from prefab
}

// Incorrect usage: For Views loaded from Resources
// Don't use the attribute for these cases
public class GameMenuView : BaseScreenView
{
    // This View will be loaded from "Screens/GameMenuView" prefab
}
```

## Advanced Usage

### Passing Data Between Screens

```csharp
// Create model with data
var gameModel = new GameModel 
{ 
    Level = 1, 
    PlayerName = "Player1" 
};

// Open screen with data
await ScreenManager.Instance.OpenScreen<GamePresenter, GameModel>(gameModel);
```

## Sample Project Structure

The package includes a complete sample demonstrating:

- Screen navigation (`FirstScreenView` ↔ `SecondScreenView`)
- Popup management (`FirstPopupView` ↔ `SecondPopupView`)
- Data passing between screens
- Proper MVP implementation

### Sample Files

```
Sample/
├── Scenes/
│   └── SampleUIClassicAsync.unity      # Demo scene
├── Scripts/
│   ├── Screens/
│   │   ├── FirstScreenView.cs          # Main menu screen
│   │   └── SecondScreenView.cs         # Secondary screen
│   └── Popups/
│       ├── FirstPopupView.cs           # First popup
│       └── SecondPopupView.cs          # Second popup
├── Resources/
│   ├── Screens/                        # Screen prefabs
│   └── Popups/                         # Popup prefabs
└── TimeLine/                           # Animation timelines
    ├── Open.playable                   # Opening transition animation
    └── Close.playable                  # Closing transition animation
```

**Note**: The sample includes Timeline assets for smooth open/close transitions. You can customize these animations or remove them for instant transitions.

## Best Practices

1. **Separation of Concerns**: Keep business logic in Models, UI logic in Presenters, and only UI components in Views
2. **Async/Await**: Use async methods for screen transitions to ensure smooth UI flow
3. **Memory Management**: Always unsubscribe from events in `OnDisable()` or `OnDestroy()`
4. **Closing Views**: Use `OnCloseView()` method to close screens or popups from within their Presenters, not `CloseView()`
5. **Transition Animations**: Configure Timeline assets for smooth open/close animations, or leave them empty for instant transitions
6. **Resource Paths**: Use consistent naming conventions for prefab paths (compatible with both Resources and Addressables systems)
7. **Model Validation**: Validate model data in Presenter's `Awake()` method

**Path Configuration Notes:**
- The framework automatically detects and switches between Resources and Addressables loading based on project configuration
- Use the same path format for both systems (e.g., "Screens/MainMenuView")
- For Resources: Place prefabs in `Assets/Resources/Screens/` folder
- For Addressables: Set the same path as the Addressable asset address

## Troubleshooting

### Common Issues

1. **Screen not opening**: Check if the `ScreenPath` or `PopupPath` matches the actual prefab location (works with both Resources and Addressables)
2. **Memory leaks**: Ensure all event listeners are properly removed in `OnDisable()`
3. **Missing dependencies**: Verify UniTask is installed and properly imported

### Debug Tips

- Enable debug logs in ScreenManager to trace screen lifecycle
- Use Unity Profiler to monitor memory usage
- Check Console for any initialization errors

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

For issues and questions, please create an issue in the project repository.