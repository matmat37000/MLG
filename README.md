# Modot
**Modot** is a .NET tool for Godot that enables loading custom code in Godot C# projects.

> [!IMPORTANT]  
> Currently, only the Linux version is implemented. No release is available yet. A full release will be published once the program is fully functional.  
> At the moment, Godot can execute arbitrary code with Modot, but it only works on my development machine. Updates will be pushed soon to fix this limitation.

## What is Modot?  
Modot is a tool that replaces the original game assembly, allowing users to load custom DLLs and PCK files into a game.  
Currently, it only supports Godot projects built with C#. Support for GDScript may be added in future versions.

## Usage  
To install Modot into a game, run the CLI:

```bash
ModotInstaller <GameExecutable>
