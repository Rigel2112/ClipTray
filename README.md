# ClipTray2026

A lightweight Windows tray clipboard-snippet tool with a built-in editor.

## Build (free, with VS Code and .NET 10 SDK)

Open this folder in VS Code and run:

```powershell
dotnet build
dotnet run
```

Right-click the tray icon and choose **Edit Clips...**, or double-click the icon.

- Select a clip to edit its title and multiline clipboard text.
- Select a category to rename it.
- **+ Clip** adds a clip inside the selected category (or alongside a selected clip).
- **+ Category** creates a category alongside the selection.
- **Delete** removes the selected clip or category and its contents.
- **Move Up / Move Down** changes menu order within a category.
- **Save** writes your changes; **Close** prompts if changes aren't saved.

The original JSON format is preserved (`Label`, `Text`, `Children`). Clips are stored in `%APPDATA%\ClipTray\clips.json`, exactly as in the previous version. Existing data is not replaced by the sample data.

**Tip:** Back up your existing `clips.json` before trying the new editor.

## Publish a standalone Windows executable

```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

The app is Windows-only. It was source-reviewed but not compiled on Windows in the authoring environment; build and test on your own PC.
