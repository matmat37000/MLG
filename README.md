# MLG

> \[!IMPORTANT]
> **Status:** Proof of concept.
>
> No official release is available yet. A full release will be published once the program is feature-complete.
> At the moment, MLG allows Godot to execute arbitrary code, but the documentation is still incomplete.
> Updates will be pushed soon to address this limitation.

---

## What is MLG?

**MLG** (**M**LG **L**oader for **G**odot) is a tool that enables users to load custom DLLs and PCK files into games built with Godot.

* Currently, only Godot projects built with **C#** are supported.
* Support for **GDScript** may be added in future versions.

---

## Usage

### Help

Display the help message:

```bash
MLGInstaller -h
```

### Install

Install MLG into a game:

```bash
MLGInstaller -i <GameExecutable>
```

### Uninstall

Remove MLG and restore the original game:

```bash
MLGInstaller -u <GameExecutable>
```

---

## Building

MLG is split into two components:

1. **MLG.Installer** — Installs and manages MLG in a target game.
2. **MLG.Bootstrap** — The bootstrap DLL used at runtime.

### Build the Installer

You can build the installer using the .NET 8 SDK:

```bash
dotnet build MLG.Installer
```

### Build the Bootstrap

To build the bootstrap component:

```bash
dotnet build MLG.Bootstrap
```

After building, copy the generated DLL into the game’s user directory.

---

## License

This project is licensed under the [GNU General Public License v3.0](https://www.gnu.org/licenses/gpl-3.0.html).
