using System;
using System.IO;
using System.Reflection;
using Microsoft.VisualBasic;
using Mono.Cecil;

namespace MLG.Installer;

internal static class DllManipulation
{
    /// <summary>
    /// Duplicate DLL and return new path, the new DLL is created next to the original
    /// </summary>
    /// <param name="originalDllPath">The path to the dll to duplicate</param>
    /// <param name="newDllPath">The path where to copy it</param>
    internal static void CopyDll(string originalDllPath, string newDllPath)
    {
        // Delete old copy to avoid error
        if (File.Exists(newDllPath))
            File.Delete(newDllPath);
        File.Copy(originalDllPath, newDllPath);
        Console.WriteLine($"Copied {originalDllPath} to {newDllPath}");
    }

    /// <summary>
    /// Restore game's DLL with the backup if it exists
    /// </summary>
    /// <param name="originalDllPath">The DLL to restore</param>
    /// <param name="newDllPath">The backup</param>
    internal static void RestoreOriginalDll(string originalDllPath, string newDllPath)
    {
        if (File.Exists(newDllPath))
        {
            if (File.Exists(originalDllPath))
            {
                File.Delete(originalDllPath);
                Console.WriteLine("Removed shim...");
            }

            File.Copy(newDllPath, originalDllPath);
            File.Delete(newDllPath);
            Console.WriteLine("Restored original.");
        }
    }

    /// <summary>
    /// Create a shim of the original game DLL, this function patch the shim to look identical to Godot
    /// </summary>
    /// <param name="originalDllPath">The path of the original assembly to shim</param>
    /// <param name="gameAssemblyName">The game assembly name, used to patch the shim</param>
    /// <exception cref="Exception">If the function cannot found the GodotPlugin dll to patch</exception>
    internal static void ShimOriginalDll(string originalDllPath, string gameAssemblyName)
    {
        // Delete game DLL
        if (File.Exists(originalDllPath))
            File.Delete(originalDllPath);

        var exeDir = GetExecutingDir();

        var dllPath = Path.Combine(exeDir, "GodotPlugins.dll");

        if (!Path.Exists(dllPath))
            throw new Exception("GodotPlugins not found");

        Console.WriteLine($"Patching {dllPath}");

        var assembly = AssemblyDefinition.ReadAssembly(dllPath);

        assembly.Name.Name = gameAssemblyName;

        // Edit DLL's attributes to make it look like the original one
        foreach (var attr in assembly.CustomAttributes)
        {
            if (attr.AttributeType.FullName == "System.Reflection.AssemblyCompanyAttribute")
            {
                attr.ConstructorArguments[0] = new CustomAttributeArgument(
                    assembly.MainModule.TypeSystem.String,
                    gameAssemblyName
                );
            }

            if (attr.AttributeType.FullName == "System.Reflection.AssemblyProductAttribute")
            {
                attr.ConstructorArguments[0] = new CustomAttributeArgument(
                    assembly.MainModule.TypeSystem.String,
                    gameAssemblyName
                );
            }

            if (attr.AttributeType.FullName == "System.Reflection.AssemblyTitleAttribute")
            {
                attr.ConstructorArguments[0] = new CustomAttributeArgument(
                    assembly.MainModule.TypeSystem.String,
                    gameAssemblyName
                );
            }
        }

        assembly.Write(originalDllPath);

        Console.WriteLine($"Wrote {originalDllPath}");
    }

    private static string GetExecutingDir()
    {
        var exePath = Assembly.GetExecutingAssembly().Location;
        var exeDir = Path.GetDirectoryName(exePath);
        if (exeDir == null)
            throw new Exception("Unable to find an installation directory for the CLI");

        return exeDir;
    }

    /// <summary>
    /// Copy project dependencies to game data dir
    /// </summary>
    /// <param name="dataFolderPath">Folder to copy dll to</param>
    internal static void CopyDependencies(string dataFolderPath)
    {
        var fileManager = new FilesManager(dataFolderPath);
        var success = fileManager.CreateFolderStructure();

        if (!success)
            return;

        var exeDir = GetExecutingDir();
        foreach (var dll in Directory.GetFiles(exeDir))
        {
            if (dll.EndsWith(".dll") && !dll.EndsWith("MLGInstaller.dll"))
                File.Copy(dll, Path.Combine(dataFolderPath, "MLG", "core", Path.GetFileName(dll)));
        }

        // var exeDir = GetExecutingDir();
        // var path = Path.Combine(exeDir, "MLG", "Lib");
        //
        // if (Path.Exists(path))
        // {
        //     var destPath = Path.Combine(dataFolderPath, "MLG", "Lib");
        //     if (!Directory.Exists(destPath))
        //         Directory.CreateDirectory(destPath);
        //
        //     foreach (var file in Directory.GetFiles(path))
        //     {
        //         File.Copy(file, Path.Combine(destPath, Path.GetFileName(file)), true);
        //     }
        // }
    }

    /// <summary>
    /// Remove all dlls in game data directory
    /// </summary>
    /// <param name="dataFolderPath"></param>
    internal static void RemoveDependencies(string dataFolderPath)
    {
        var fileManager = new FilesManager(dataFolderPath);
        Console.WriteLine($"Removing {dataFolderPath}/MLG");
        fileManager.RemoveFolderStructure();
    }
}
