/*
* DllManipulation.cs - Code related to patching and moving dll around during the installation
* Copyright (C) 2025  BORDIER-AUPY Mathieu
*
* This program is free software: you can redistribute it and/or modify
* it under the terms of the GNU General Public License as published by
* the Free Software Foundation, either version 3 of the License, or
* (at your option) any later version.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.IO;
using System.Reflection;
using Microsoft.VisualBasic;
using Mono.Cecil;

namespace MLG.Installer;

internal static class DllManipulation
{
    /// <summary>
    /// Duplicates a DLL file and returns the path to the copy.
    /// The duplicated DLL is created alongside the original.
    /// </summary>
    /// <param name="originalDllPath">
    /// The full path to the DLL to duplicate.
    /// </param>
    /// <param name="newDllPath">
    /// The full path where the DLL copy will be created.
    /// </param>
    /// <returns>
    /// The full path to the duplicated DLL.
    /// </returns>
    internal static void CopyDll(string originalDllPath, string newDllPath)
    {
        // Delete old copy to avoid error
        if (File.Exists(newDllPath))
            File.Delete(newDllPath);
        File.Copy(originalDllPath, newDllPath);
        Console.WriteLine($"Copied {originalDllPath} to {newDllPath}");
    }

    /// <summary>
    /// Restores the game's original DLL from a backup, if the backup exists.
    /// </summary>
    /// <param name="originalDllPath">
    /// The full path to the game DLL that should be restored.
    /// </param>
    /// <param name="newDllPath">
    /// The full path to the backup DLL used for restoration.
    /// </param>
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
    /// Creates a shim for the original game DLL. The generated shim is patched
    /// to appear identical to the Godot assembly.
    /// </summary>
    /// <param name="originalDllPath">
    /// The full path to the original assembly that will be shimmed.
    /// </param>
    /// <param name="gameAssemblyName">
    /// The name of the game assembly, used to patch the shim.
    /// </param>
    /// <exception cref="Exception">
    /// Thrown if the required <c>GodotPlugin.dll</c> cannot be found for patching.
    /// </exception>
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

    /// <summary>
    /// Retrieves the directory from which the program is being executed.
    /// </summary>
    /// <returns>
    /// The full path to the program's execution directory.
    /// </returns>
    /// <exception cref="DirectoryNotFoundException">
    /// Thrown when the program's installation or execution directory cannot be determined.
    /// </exception>
    private static string GetExecutingDir()
    {
        var exePath = Assembly.GetExecutingAssembly().Location;
        var exeDir = Path.GetDirectoryName(exePath);
        if (exeDir == null)
            throw new DirectoryNotFoundException(
                "Unable to find an installation directory for the CLI"
            );

        return exeDir;
    }

    /// <summary>
    /// Copies the project dependency DLLs into the specified game data directory.
    /// </summary>
    /// <param name="dataFolderPath">
    /// The full path to the game data directory where the DLLs will be copied.
    /// </param>
    internal static void CopyDependencies(string dataFolderPath)
    {
        var success = FilesManager.CreateFolderStructure(dataFolderPath);

        // If there are no folders created we can't continue
        if (!success)
            return;

        var exeDir = GetExecutingDir();
        // Copy each dll from the execution directory to the MLG/core folder
        foreach (var dll in Directory.GetFiles(exeDir))
        {
            if (dll.EndsWith(".dll") && !dll.EndsWith("MLGInstaller.dll"))
            {
                var path = Path.Combine(dataFolderPath, "MLG", "core", Path.GetFileName(dll));
                File.Copy(dll, path);
                Console.WriteLine($"{dll} -> {path}");
            }
        }
    }

    /// <summary>
    /// Removes all dependency DLLs from the specified game data directory.
    /// </summary>
    /// <param name="dataFolderPath">
    /// The full path to the game data directory containing the DLLs to remove.
    /// </param>
    internal static void RemoveDependencies(string dataFolderPath)
    {
        Console.WriteLine($"Removing {dataFolderPath}/MLG");
        FilesManager.RemoveFolderStructure(dataFolderPath);
    }
}
