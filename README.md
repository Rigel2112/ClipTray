# ClipTray 2026

A lightweight Windows system tray clipboard utility inspired by the original Microsoft ClipTray from the Windows XP era.

ClipTray lets you store frequently used text snippets in organized categories and copy them to your clipboard with just two clicks.

**No accounts. No cloud services. No unnecessary features.**

## Download and Installation

**You do not need Visual Studio, the .NET SDK, or any programming knowledge to use ClipTray.**

1. Go to the [GitHub Releases](https://github.com/Rigel2112/ClipTray/releases) page.
2. Download `ClipTray.exe` from the latest release.
3. Place the EXE in a folder of your choice.
4. Double-click `ClipTray.exe` to run it.

That's it! No installation required.

The executable is self-contained and includes the necessary .NET runtime.

**Requirements:** Windows 10 or Windows 11 (64-bit).

*Note: Windows SmartScreen may display an unknown publisher warning because the executable is not digitally signed.*

## How to Use

1. Find the ClipTray icon in your Windows system tray (notification area).
2. Right-click the icon to display your saved clips and categories.
3. Select any clip to copy its text to the Windows clipboard.
4. Paste the text anywhere using `Ctrl+V`.

ClipTray runs quietly in the background until you choose Exit.

## Built-in Clip Editor

Version 0.2 introduces a built-in editor for managing your clips.

Right-click the ClipTray icon and select **Edit Clips...**

The editor allows you to:

- Create and delete clips.
- Create and organize categories.
- Rename clips and categories.
- Edit multiline text snippets.
- Reorder clips and categories.
- Save or discard changes.

Your changes are reflected in the tray menu after saving.

## Configuration

ClipTray stores its clips in a simple JSON file:

`%APPDATA%\ClipTray\clips.json`

You can back up this file or copy it to another computer to transfer your saved clips.

No internet connection is required.

## Features

- Lightweight system tray application.
- Quick access to frequently used text.
- Organized categories and submenus.
- Multiline text support.
- Built-in clip editor.
- Persistent JSON configuration.
- Portable, self
