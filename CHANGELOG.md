## [1.2.1] - 2026-11-20

### Added
- `ConsoleToggleStyle` for styling the console toggle.
- Clear button in the console.
- "Tap outside to close" hint.
- Time shown for each log record.
- Assertion log records, so the Assertions filter now works.
- Package documentation.

### Changed
- Restyled the console shell, tabs, log cards, command input and scene tree.
- Command input stays at the bottom of the console.

### Fixed
- Assertions were shown as warnings.
- `ConsoleSystem.RemoveModule` removed nothing.
- `UnRegister` of console commands removed nothing.

## [1.2.0] - 2026-03-24

### Added
- `ActionCommand` to register simple command delegates.
- `SceneHierarchyModule` for browsing the scene hierarchy via console.

### Changed
- `PersistentModule` constructor made public to ease customization and persistence across sessions.
- Improved console tab ordering for better navigation.

### Fixed
- Style override handling improved for UI Toolkit resources.

## [1.0.0] - 2025-07-15

### Added

- Sequence opener sample
- Generating and using style overrides
- Generating and using panel settings overrides

## [1.0.0] - 2025-02-24

### Added

- Created first version of package

### Changed

### Fixed
