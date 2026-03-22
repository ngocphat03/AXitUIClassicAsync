---
name: Axit UI CLI Workflow
description: Guide on using the included `create_ui.ps1` and `create_ui.sh` to aid UI generation.
---

# Axit UI Classic Async - CLI Documentation

This package includes CLI tools (`create_ui.ps1` for Windows, `create_ui.sh` for macOS/Linux) at the root of the package directory that AI agents can run to rapidly generate Screen and Popup MVP scripts.

## Usage

### On Windows (PowerShell)
```powershell
.\create_ui.ps1 -Name <UITitle> -Type <Screen|Popup> -OutDir <SaveFolderPath>
```
*Example:* `.\create_ui.ps1 -Name WinScreen -Type Screen -OutDir "Assets/Scripts/UI"`

### On macOS / Linux (Bash)
*Ensure it has executable permissions on the first run: `chmod +x create_ui.sh`*
```bash
./create_ui.sh -n <UITitle> -t <Screen|Popup> -o <SaveFolderPath>
```
*Example:* `./create_ui.sh -n WinScreen -t Screen -o "Assets/Scripts/UI"`

## Result
The CLI tool will generate all 3 standard MVP classes (Model, View, Presenter) packaged securely into a single `[Name]View.cs` file. 
This feature should be extensively used by AI agents to rapidly bootstrap UI scripts instead of sequentially writing file modifications line-by-line.
