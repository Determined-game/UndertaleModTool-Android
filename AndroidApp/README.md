# UndertaleModTool Android UI

This is the first Android UI prototype for the UndertaleModTool fork.

## Where to put it

Copy this entire `AndroidApp` folder into the repository root:

    UndertaleModTool-Android/
      AndroidApp/
      UndertaleModLib/
      UndertaleModTool/
      ...

The project references:

    ../UndertaleModLib/UndertaleModLib.csproj

## Build

From the repository root:

    dotnet build AndroidApp/AndroidApp.csproj

To install/run on a connected Android device:

    dotnet build AndroidApp/AndroidApp.csproj -t:Install

## Current UI

- Open game file
- Save / Save As buttons (UI only for now)
- Undo / Redo buttons (UI only for now)
- Resource categories
- Editor area
- Dark touch-friendly layout

## Next step

Connect the existing UndertaleModLib parser to the Android file picker, then populate
Sprites, Rooms, Objects, Scripts, Sounds and Fonts from the loaded data.win/game.unx file.
