# Modular Console - Open Questions

## Behaviour and bugs
2. **Assertions filter.** `ConsoleModule` has a "Show Assertions" toggle, but `OnLogFiltersChanged` ignores it and assertion log types fall into the warning branch. Is assertion filtering unfinished?
3. **Command parsing.** `text.Replace(commandText, "")` removes every occurrence of the command word from the arguments, not only the first. Intended? `ActionCommand` ignores arguments; are argument-aware commands planned?
4. **Log growth.** Log records are never trimmed. Is a cap wanted?
5. **Rebuilding UI.** The UI (including the `SceneHierarchyModule` tree) is rebuilt on each `OnEnable`, and `ConsoleModule` re-adds the same element instances. Is the scene tree meant to be a snapshot?

## Design intent
7. **Module discovery.** `PersistentModule` subclasses are found by scanning all assemblies' types and built with `Activator.CreateInstance`. A type that fails to load could throw `ReflectionTypeLoadException`; modules with constructor parameters cannot be used. Is that the intended contract?
8. **Unused constant.** `ConsoleUI.CONSOLE_HEIGHT_PERCENTAGE_OF_SCREEN` is unused; leftover or planned layout feature?
9. **Visibility.** `ConsoleSystem.Modules` and `ConsoleModule.Commands` are internal, so commands can only be added through `Register()`. Intentional encapsulation?

## Sample and input
10. **Legacy Input.** `ConsoleSequenceOpener` uses `UnityEngine.Input`, which does not work with the new Input System only. Is Input System support planned?
11. **Sample assembly.** The sample has no asmdef and relies on the default assembly. Acceptable?

## Release and docs
12. **CHANGELOG.** Two `1.0.0` entries and none after 1.2.0; README covers only installation. Known?
13. **Consumption path.** The package does not state whether consumers install via Git URL only or via an installer tool (assumed).
14. **Tests.** The package has none. Planned?
15. **Naming.** Editor namespace is `ModularConsole.Editor.Editor`, asmdef is `com.fixer33.modular-console.Editor`. Intentional?

## Answered

- **UnRegister never removes** (was Q1): author said to fix it. Fixed in `IConsoleCommandExtensions.UnRegister` (condition corrected to `Contains(command)`).
- **Release builds** (was Q6): author decided the console is not to be gated out of release builds; current behaviour is intentional.
