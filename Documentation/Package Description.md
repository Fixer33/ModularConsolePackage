# Modular Console - Package Description

Package `com.fixer33.modular-console` 1.2.0 (display name "Modular console"), Unity 6000.1 or newer. Author Fixer33, MIT license. Git URL: https://github.com/Fixer33/ModularConsolePackage.git (from README).

## 1. Purpose and users

An in-game (runtime) debug console built on UI Toolkit. It shows Unity log output with filters, lets developers type commands, and hosts additional "modules" as tabs (for example a scene hierarchy browser). Intended users are game developers who want a debug console in development builds (the package intentionally does not strip itself from release builds, confirmed by the author).

## 2. Architecture and main components

Namespaces: `ModularConsole`, `ModularConsole.Contracts`, `ModularConsole.Modules`, `ModularConsole.Modules.Console`, `ModularConsole.Modules.Console.Commands`, `ModularConsole.Editor.Editor`.

- **Bootstrap**: `ConsoleInitializer` (internal static). Its `Initialize()` runs with `[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)]`, so the console exists automatically in every play session; no scene setup is needed. It:
  1. calls `ConsoleSystem.Reset()` (clears modules);
  2. finds every non-abstract subclass of `PersistentModule` in all loaded assemblies by reflection and instantiates each with `Activator.CreateInstance`;
  3. loads resources `ModularConsoleStyles`, `UnityDefaultRuntimeTheme` and `ModularConsoleDocument` from `Runtime/Resources`;
  4. creates a GameObject "ModularConsole - Don't destroy" (DontDestroyOnLoad) with a `ConsoleUI` component;
  5. loads optional user overrides from `Resources/ModularConsole/`: style `ModularConsoleStyleBaseOverride`, a mobile style `ModularConsoleStyleBaseMobileOverride` (only on `UNITY_ANDROID || UNITY_IOS`), and a `PanelSettings` asset `ConsolePanelSettingsOverride`.
- **`ConsoleSystem`** (public static): registry and constants. `UI` (current `ConsoleUI`), `Reset()`, `AddModule(IConsoleModule)`, `RemoveModule(IConsoleModule)`, plus public const resource names `THEME_STYLESHEET_FILE_NAME`, `INTERNAL_STYLES_FILE_NAME`, `CONSOLE_STYLE_OVERRIDE_NAME`, `CONSOLE_STYLE_MOBILE_OVERRIDE_NAME`, `CONSOLE_STYLE_PANEL_SETTINGS_OVERRIDE_NAME`. The module list itself (`Modules`) is internal.
- **`ConsoleUI`** (public MonoBehaviour): adds a `UIDocument` (auto-generated `PanelSettings` with sortingOrder 9999 unless an override exists). `Show()` / `Hide()` toggle the GameObject; `Show()` unlocks the cursor and `Hide()` restores the previous lock state. `IsVisible` reads `gameObject.activeSelf`. On every `OnEnable` it clones the UXML, adds styles, and builds one `Tab` per module (ordered by `ConsoleTabOrder`) inside the `TabView` named `content-holder`; clicking the element `close-region` hides it. `Update()` calls `Update()` of modules whose `NeedsUpdate` is true. Starts hidden.
- **`IConsoleModule`** (public interface, `ModularConsole.Contracts`): `ConsoleTabOrder`, `ModuleName`, `NeedsUpdate` (default false), `ConstructUI(VisualElement root)`, `Update()` (default empty).
- **`PersistentModule`** (public abstract class): base for modules that are auto-discovered and registered at startup; virtual `ConsoleTabOrder` (0), abstract `ModuleName` and `ConstructUI`, public parameterless constructor.
- **`ConsoleModule`** (public, tab "Console", order -1): subscribes to `Application.logMessageReceived`, shows messages in a scroll view with four filter toggles (logs, warnings, errors, assertions); identical consecutive messages are collapsed with a counter; errors show stack traces and a click expands a record. Has a command input field; on Enter it splits the text at the first space, finds the registered `IConsoleCommand` whose `CommandText` equals the first word, and calls `Execute(argsString)`; unknown commands log a warning. Internal helpers: `ConsoleLogMessage`, `LogRecordBase`/`SimpleLog`/`WarningLog`/`ErrorLog`.
- **`IConsoleCommand`** (public): `CommandText`, `Execute(string arguments = null)`. Extension methods `Register()` / `UnRegister()` in `IConsoleCommandExtensions` add or remove a command from the console's command list.
- **`ActionCommand`** (public readonly struct): `new ActionCommand("name", () => ...)`; ignores arguments.
- **`SceneHierarchyModule`** (public, tab "Scenes hierarchy"): TreeView of the DontDestroyOnLoad objects and all loaded scenes with their GameObject children, and a toggle per object that calls `SetActive`. The tree is built when the console UI is built (each time the console is shown); it is not live-updated (assumed from code).

## 3. Public API surface and extension points

Entry points: `ConsoleSystem.UI.Show()` / `Hide()` / `IsVisible`; `ConsoleSystem.AddModule` / `RemoveModule`; `new ActionCommand(...).Register()` or any custom `IConsoleCommand`.

Extension points:
1. Subclass `PersistentModule` (concrete, public parameterless ctor) for an automatically registered tab.
2. Implement `IConsoleModule` and call `ConsoleSystem.AddModule` for manual lifetime control.
3. Implement `IConsoleCommand` and call `Register()`.
4. Styling and panel overrides: place assets in `Resources/ModularConsole/` (names above).

The package provides no built-in hotkey to open the console; the sample does it.

Because `PersistentModule` subclasses are created with `Activator.CreateInstance`, a subclass without a parameterless constructor will throw at startup (assumed from code).

## 4. Dependencies

- `package.json` declares no dependencies; Unity 6000.1 minimum.
- Implicit: UnityEngine, UnityEngine.UIElements (UI Toolkit runtime, `TabView`/`Tab`/`TreeView`), Unity `Resources` loading. Editor code uses UnityEditor.
- Runtime asmdef `com.fixer33.modular-console` (root namespace ModularConsole): no references, no define constraints, no version defines, auto-referenced.
- Editor asmdef `com.fixer33.modular-console.Editor`: Editor platform only, references the runtime asmdef.
- Code uses `UNITY_ANDROID || UNITY_IOS` and (in the sample) `UNITY_EDITOR`. The package defines no scripting symbols.
- The sample uses the legacy `UnityEngine.Input` API, so it needs the old Input Manager or "Both" active input handling (assumed to fail with the new Input System only).

## 5. Editor tools and menu items

`ConsoleSystemContextMenu` (Editor asmdef), Project-window context menus under `Assets/Fixer33/ModularConsole/`:
- "Regenerate console panel settings override": creates `Assets/Resources/ModularConsole/ConsolePanelSettingsOverride.asset` (sortingOrder 9999, package theme); asks before overwriting.
- "Regenerate console style override": copies the package's `ModularConsoleStyles.uss` to `Assets/Resources/ModularConsole/ModularConsoleStyleBaseOverride.uss`.
- "Regenerate console style mobile override": same, to `ModularConsoleStyleBaseMobileOverride.uss`.

The style tools ask before replacing an existing file; all three create the `Assets/Resources/ModularConsole` folder when needed.

## 6. Samples and tests

- Sample "ConsoleSequenceOpener" (`Samples~/ConsoleSequenceOpener`, imported via Package Manager): `ConsoleSequenceOpener` MonoBehaviour that opens the console after a key sequence (default C, N, S, L) or a sequence of screen clicks (default pattern of corner clicks with a radius). Menu item `Assets/Fixer33/Create console opener` creates a prefab at `Assets/Resources/ModularConsole/ConsoleSequenceOpener.prefab`; a `RuntimeInitializeOnLoadMethod(AfterSceneLoad)` instantiates it from Resources at startup. The sample has no asmdef (compiles in the project's default assembly once imported).
- No tests in the package.

## 7. How other projects consume it

Installed through Package Manager "Add package from git URL" with the URL above (README), or through the author's installer tooling (assumed). No setup is required: the console auto-initializes. Projects add their own commands/modules and provide an opener (the sample or custom code calling `ConsoleSystem.UI.Show()`).

## 8. Current state

Finished and usable (version 1.2.0; CHANGELOG up to 1.2.0 dated 2026-03-24). Observations from code:
- `IConsoleCommandExtensions.UnRegister` had an inverted check (it never removed a registered command); fixed at the author's request, it now removes the command when registered.
- The console is intentionally not gated out of release builds (author decision).
- `ConsoleUI.CONSOLE_HEIGHT_PERCENTAGE_OF_SCREEN` is declared but unused.
- The "Show Assertions" toggle in `ConsoleModule` is not used by the filter handler (assumed unfinished).
- The log collection is unbounded (assumed).
- `SceneHierarchyModule` has an empty destructor.
- Editor menu class lives in namespace `ModularConsole.Editor.Editor` (doubled "Editor").
- CHANGELOG has two entries labeled 1.0.0 and nothing after 1.2.0.
- README is minimal (installation and license only).
