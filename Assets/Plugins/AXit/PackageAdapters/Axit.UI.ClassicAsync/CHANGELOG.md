# Changelog
All notable changes to this project will be documented in this file.

## [1.0.2] - 2026-03-22
### Added
- Created `.agent/skills` folder to empower AI agents with CLI and package architecture guidelines.
- Added automation shell scripts `create_ui.ps1` and `create_ui.sh` to scaffold standard MVP (Model/View/Presenter) classes in one command.
- Introduced `[MenuItem("GameObject/Axit/UI/Create RootUI")]` allowing developers to instantiate the core RootUI prefab directly into the Hierarchy via the right-click menu.

### Changed
- Refactored all references of "AXit" to "Axit" dynamically across folders, paths, and source code files (e.g., renamed root folder `AXit` to `Axit`, updated `AXitUiClassicEditor.cs` to `AxitUiClassicEditor.cs`).
