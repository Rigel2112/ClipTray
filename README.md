# ClipTray

Tiny Windows tray application for copying canned text snippets to the clipboard.

## Requirements
- Windows 10/11
- .NET 10 SDK (or Visual Studio 2022 with .NET desktop development)

## Build and run
From the folder containing `ClipTray.csproj`:

```powershell
dotnet run
```

To publish a standalone executable (Windows x64):

```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

The EXE will be in `bin\Release\net8.0-windows\win-x64\publish\`.

## Usage
- Right-click the tray icon to select a snippet. Its text goes to the clipboard; press Ctrl+V to paste.
- Double-click the tray icon or choose **Edit clips.json** to open the configuration in Notepad.
- Save changes; the menu reloads whenever it is opened. No restart necessary.
- **Exit** stops the application.

## Configuration
On first run, sample entries are created at `%APPDATA%\ClipTray\clips.json`.

Each entry has a `Label` and either `Text` (for a snippet) or `Children` (for a submenu).
JSON newline escapes (`\n`) are supported. JSON must remain valid.

## Notes
- No automatic paste or keystroke injection.
- Do not store passwords or sensitive secrets in clipboard snippets.
- Uses the built-in Windows information icon; you can replace it with a custom icon later.
