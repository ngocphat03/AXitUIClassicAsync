---
name: Axit UI Framework Guidelines
description: The core rules for developing standard UI using the Axit.UI.ClassicAsync framework.
---

# Axit.UI.ClassicAsync Implementation Guidelines

Whenever an AI is tasked with creating a new Screen or Popup using the `Axit.UI.ClassicAsync` framework, it MUST strictly follow these rules:

## 1. MVP Architecture Definition
A new Screen or Popup invariably consists of **3 interdependent components** (usually written consecutively in a single `.cs` file for simplicity):
1. **Model** (`[Name]Model`): Inherits `BaseScreenModel` or `BasePopupModel`. Contains data arguments strictly tied to view state.
2. **View** (`[Name]View`): Inherits `BaseScreenView` or `BasePopupView`. Houses Unity Inspector references (`public Button btn`, `public TMP_Text lbl`). NEVER contains business logic.
3. **Presenter** (`[Name]Presenter`): Inherits `BaseScreenPresenter<TView, TModel>` or `BasePopupPresenter<TView, TModel>`. Contains lifecycle hooks and functional logic.

## 2. Dependency Injection
The framework relies entirely on **VContainer** dependency injection.
- Do NOT utilize singletons, `FindObjectOfType`, or static references to access services.
- Define dependent services (like `ScreenManager`, `AudioService`) exclusively via the Presenter's **Constructor**.

## 3. Mandatory Lifecycle Rules
- **ScreenPath / PopupPath Override**: Must be declared returning the exact Addressables or Resource path string (e.g. `"UI/SettingsPopupView"`).
- **Event Subscriptions in `OnEnable`**: Register interface listeners (e.g. `this.View.button.onClick.AddListener(...)`) and signal subscriptions ONLY inside `OnEnable()`. 
- **Event Unsubscriptions in `OnDisable`**: CRITICAL RULE. Every event bound in `OnEnable()` MUST be mirrored with an unbind (e.g. `RemoveListener()`) in `OnDisable()` to plug Memory Leaks.
- **Closure**: Never call `View.CloseView()` directly! Use `this.OnCloseView()` inside the Presenter to safely delegate destruction to the ScreenManager.

## 4. Automation Usage
Instead of manually typing the entire structural boilerplate for new UIs, the AI should invoke the `.agent/skills/cli_workflow.md` CLI scripts (`create_ui.sh` or `create_ui.ps1`) to scaffold out standard files instantly.
