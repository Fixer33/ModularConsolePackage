# Modular Console - Usage Guide

Package `com.fixer33.modular-console` 1.2.0 ("Modular console"), Unity 6000.1 or newer, MIT license.
An in-game runtime console built on UI Toolkit: log view with filters, text commands, and pluggable tabs ("modules").

Anything marked **(assumed)** was inferred from the code rather than read directly.

## 1. Installation

### Via com.fixer33.package-installer (recommended route)

1. Install the installer package from `https://github.com/Fixer33/PackageInstaller.git` (Package Manager > **+** > Add package from git URL).
2. Open **Tools > Fixer33 > Package Installer**.
3. Find the entry "Modular console" (package id `com.fixer33.modular-console`) in the installer's package list and install it with one click.

The installer's list entry points to `https://github.com/Fixer33/ModularConsolePackage.git`.

### Manually via Git URL

Window > Package Manager > **+** > Add package from git URL, then enter:

```
https://github.com/Fixer33/ModularConsolePackage.git
```

The package declares no dependencies in `package.json`.

## 2. Minimal setup

There is no scene setup. `ConsoleInitializer` (internal) runs with `[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)]` in every play session (Editor play mode and builds) and creates a DontDestroyOnLoad GameObject named "ModularConsole - Don't destroy" carrying a `ConsoleUI`. The console starts hidden.

The package provides **no hotkey** to open it. You must call `ConsoleSystem.UI.Show()` yourself, for example from your own script:

```csharp
using ModularConsole;
using UnityEngine;

public class OpenConsoleOnF1 : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1)) // legacy Input API
        {
            if (ConsoleSystem.UI.IsVisible) ConsoleSystem.UI.Hide();
            else ConsoleSystem.UI.Show();
        }
    }
}
```

Alternatively import the sample "ConsoleSequenceOpener" (section 4 and 5).

Note: the console is not removed from release builds (intentional per the author). If you do not want it in a release build you must prevent it yourself (assumed: no package switch exists; none was found in code).

## 3. Using the public API step by step

### 3.1 Show and hide

```csharp
ConsoleSystem.UI.Show();   // unlocks the cursor (remembers previous lock state)
ConsoleSystem.UI.Hide();   // restores the remembered cursor lock state
bool open = ConsoleSystem.UI.IsVisible;
```

`ConsoleSystem.UI` is a `ConsoleUI` (public MonoBehaviour). It is assigned at the end of initialization, so access it after `BeforeSceneLoad` (for example from `Awake`/`Start` or `AfterSceneLoad`). Clicking the element named `close-region` in the UI also hides the console.

### 3.2 Register a simple command

Commands are typed in the "Console" tab's input field and run on Enter. Use `ActionCommand` for a delegate:

```csharp
using ModularConsole.Modules.Console;
using ModularConsole.Modules.Console.Commands;

new ActionCommand("god", () => Debug.Log("God mode toggled")).Register();
```

Typing `god` runs the action. `ActionCommand` ignores any arguments.

Remove it again with `UnRegister()` on the same command value:

```csharp
var cmd = new ActionCommand("god", ToggleGod);
cmd.Register();
// ...
cmd.UnRegister();
```

(The command list is a static list, so registering before the console UI is built is fine.)

### 3.3 Write a command that reads arguments

Implement `IConsoleCommand` (`CommandText`, `Execute(string arguments = null)`). The input is split at the first space: the first word is matched against `CommandText`, and the rest is passed as one string to `Execute` (see pitfalls for a parsing quirk).

```csharp
public class SetTimeScaleCommand : IConsoleCommand
{
    public string CommandText => "timescale";

    public void Execute(string arguments = null)
    {
        if (float.TryParse(arguments, out var value))
            Time.timeScale = value;
        else
            Debug.LogWarning("Usage: timescale <float>");
    }
}

new SetTimeScaleCommand().Register();
```

Unknown commands log a warning: "Failed to execute command <name>: No such command found".

### 3.4 Add or remove modules (tabs)

```csharp
ConsoleSystem.AddModule(myModule);    // IConsoleModule
ConsoleSystem.RemoveModule(myModule);
ConsoleSystem.Reset();                // clears all modules (also called at startup)
```

Tabs are built every time the console is shown (`ConsoleUI.OnEnable`), ordered by `ConsoleTabOrder`, so modules added while the console is hidden appear on the next `Show()`. Modules deriving from `PersistentModule` need no registration (section 6).

### 3.5 Built-in modules

- **ConsoleModule** (tab "Console", order -1): log view with filter toggles (Show Logs / Warnings / Errors / Assertions), consecutive identical messages collapsed with a counter, command input field.
- **SceneHierarchyModule** (tab "Scenes hierarchy"): tree of DontDestroyOnLoad objects and loaded scenes with their GameObjects and an enable toggle per object. It is built when the console UI is built, i.e. a snapshot taken each time the console is shown (assumed from code).

## 4. Editor windows and menu paths

The package has no EditorWindow. It adds Project-window/Assets menu items (class `ModularConsole.Editor.Editor.ConsoleSystemContextMenu`):

| Menu path | Effect |
|---|---|
| `Assets/Fixer33/ModularConsole/Regenerate console panel settings override` | Creates `Assets/Resources/ModularConsole/ConsolePanelSettingsOverride.asset` (sorting order 9999, package theme). Asks before overwriting. |
| `Assets/Fixer33/ModularConsole/Regenerate console style override` | Copies the package's `ModularConsoleStyles.uss` to `Assets/Resources/ModularConsole/ModularConsoleStyleBaseOverride.uss`. Asks before replacing. |
| `Assets/Fixer33/ModularConsole/Regenerate console style mobile override` | Same, to `ModularConsoleStyleBaseMobileOverride.uss`. |
| `Assets/Fixer33/Create console opener` (sample only) | Creates prefab `Assets/Resources/ModularConsole/ConsoleSequenceOpener.prefab`. Hidden once the prefab exists. |

All three override tools create `Assets/Resources` and `Assets/Resources/ModularConsole` if missing. "Regenerate" deletes and recreates the file when you confirm, so do it before customizing, or back up your edits.

For the package-installer, the window is under **Tools > Fixer33 > Package Installer**.

## 5. Configuration and scripting defines

There is no settings asset and the package defines **no scripting symbols**. Configuration is by asset names in a `Resources/ModularConsole/` folder, loaded at startup:

| Asset name (in `Resources/ModularConsole/`) | Type | Purpose |
|---|---|---|
| `ModularConsoleStyleBaseOverride` | StyleSheet (.uss) | Extra style sheet added after the built-in styles |
| `ModularConsoleStyleBaseMobileOverride` | StyleSheet (.uss) | Extra style sheet, loaded only when compiled for `UNITY_ANDROID` or `UNITY_IOS` |
| `ConsolePanelSettingsOverride` | PanelSettings | Replaces the auto-generated panel settings (otherwise created at runtime with sorting order 9999) |

The same names are public constants on `ConsoleSystem`: `CONSOLE_STYLE_OVERRIDE_NAME`, `CONSOLE_STYLE_MOBILE_OVERRIDE_NAME`, `CONSOLE_STYLE_PANEL_SETTINGS_OVERRIDE_NAME` (plus `THEME_STYLESHEET_FILE_NAME`, `INTERNAL_STYLES_FILE_NAME` for the package's own resources).

Defines the code reacts to: `UNITY_ANDROID || UNITY_IOS` (mobile style override) and, in the sample only, `UNITY_EDITOR`.

### Sample: ConsoleSequenceOpener

Import via Package Manager > Modular console > Samples. `ConsoleSequenceOpener` (namespace `ModularConsole.Samples`) opens the console after a key sequence (serialized `_sequence`, default C, N, S, L) or a click sequence (`_clickSequence`, default corner clicks with a pixel radius). Steps: import the sample, run `Assets/Fixer33/Create console opener`, and press play; the prefab is instantiated from Resources after scene load and survives scene changes. It uses the legacy `UnityEngine.Input` class, so it needs Active Input Handling set to "Input Manager" or "Both" (assumed to fail with the new Input System only). The sample has no asmdef (compiles into the default assembly).

## 6. Extension points and worked example

1. Subclass `PersistentModule` (public, concrete, public parameterless constructor): auto-discovered and registered at startup by reflection over all loaded assemblies.
2. Implement `IConsoleModule` and call `ConsoleSystem.AddModule` for manual lifetime control.
3. Implement `IConsoleCommand` and call `Register()`.
4. Override styles / panel settings (section 5).

`IConsoleModule` members: `ConsoleTabOrder`, `ModuleName`, `NeedsUpdate` (default false), `ConstructUI(VisualElement root)`, `Update()` (default empty). `ConsoleUI.Update` calls `Update()` of modules with `NeedsUpdate == true`, every frame. Because `Hide()` deactivates the GameObject, updates run only while the console is visible (assumed from `SetActive(false)`).

### Worked example: an FPS tab plus a command

```csharp
using ModularConsole.Contracts;
using ModularConsole.Modules.Console;
using ModularConsole.Modules.Console.Commands;
using UnityEngine;
using UnityEngine.UIElements;

public class FpsModule : PersistentModule
{
    private readonly Label _label = new Label("...");

    public override string ModuleName => "FPS";
    public override int ConsoleTabOrder => 10;      // after "Console" (-1) and default 0
    public bool NeedsUpdate => true;                 // see note below

    public FpsModule()                               // parameterless, public
    {
        new ActionCommand("fps", () => Debug.Log($"FPS: {1f / Time.unscaledDeltaTime:0}")).Register();
    }

    public override void ConstructUI(VisualElement root)
    {
        root.Add(_label);
    }

    public void Update()
    {
        _label.text = $"{1f / Time.unscaledDeltaTime:0} fps";
    }
}
```

No other code is required: because it derives from `PersistentModule`, `ConsoleInitializer` finds and instantiates it. Note on `NeedsUpdate`/`Update`: `PersistentModule` does not declare them, they are default interface members of `IConsoleModule`. `ConsoleModule` and `SceneHierarchyModule` follow the same pattern of declaring public members with matching names, which satisfies the interface implicitly (assumed to work for the example; the built-in `ConsoleModule` relies on it).

## 7. Common pitfalls (current behaviour)

- **No built-in opener.** `ConsoleSystem.UI.Show()` must be called by you; `ConsoleSystem.UI` is null before initialization or after the console object is destroyed.
- **Constructors.** `PersistentModule` subclasses are created with `Activator.CreateInstance`; a subclass without a public parameterless constructor throws at startup (assumed from code). The discovery uses `GetTypes()` on all assemblies, so a type that fails to load may throw `ReflectionTypeLoadException` (assumed).
- **Constructor side effects.** Every `PersistentModule` is instantiated at startup in every play session, including ones you never open the console in. Keep constructors cheap.
- **Argument parsing.** Arguments are computed with `text.Replace(commandText, "")`, which removes every occurrence of the command word, not only the first. For `say say hello`, arguments become `hello`, not `say hello`. Avoid commands whose words can appear in their own arguments.
- **Commands only through `Register()`.** `ConsoleSystem.Modules` and `ConsoleModule.Commands` are internal. Commands with equal `CommandText` can be registered twice; the first one found wins.
- **Assertions filter.** The "Show Assertions" toggle exists but the filter handler ignores it. Assertion logs are treated like warnings and are controlled by "Show Warnings".
- **Unbounded log.** Log records are never trimmed (assumed: memory grows in long sessions).
- **UI is rebuilt on every Show.** The scene hierarchy tree is a snapshot per show, not live.
- **Cursor.** `Hide()` during initialization restores the remembered lock state, which starts as `CursorLockMode.None`, so the cursor lock is set to None once at startup (read from code; effect on your game is assumed).
- **Release builds.** The console is included and active in release builds.
- **`CONSOLE_HEIGHT_PERCENTAGE_OF_SCREEN`** in `ConsoleUI` is private and unused; it has no effect on layout.
- **Style regeneration.** Regenerating a style/panel-settings override overwrites your edits if you confirm the dialog.
- **Editor namespace** is `ModularConsole.Editor.Editor` (doubled); do not rely on it.

## 8. Working with other com.fixer33.* packages

The package has no dependencies and no references to other com.fixer33.* packages in its code or asmdefs. It is listed in the installer:

- `com.fixer33.package-installer` - `https://github.com/Fixer33/PackageInstaller.git` (installation route).

No other integration is documented or present.

## 9. Remaining open questions

From the package's open questions, the ones affecting usage:

- **Q2 Assertions filter:** is assertion filtering unfinished? (affects how assertion logs can be filtered)
- **Q3 Command parsing:** is the "remove every occurrence" behavior intended, and are argument-aware built-in commands planned?
- **Q4 Log growth:** is a cap on stored log records wanted?
- **Q5 Rebuilding UI:** is the scene tree meant to be a snapshot?
- **Q7 Module discovery:** is reflection-based discovery with `Activator.CreateInstance` (no constructor arguments) the intended contract?
- **Q9 Visibility:** is it intentional that commands can be added only through `Register()`?
- **Q10 Legacy Input:** is new Input System support planned for the sample opener?
- **Q13 Consumption path:** whether consumers install via Git URL only or via the installer is not stated by the package (this guide documents both).
