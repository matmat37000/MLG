using System.Reflection;
using Mono.Cecil;

namespace Modot.Bootstrap;

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
                File.Delete(originalDllPath);

            File.Copy(newDllPath, originalDllPath);
            File.Delete(newDllPath);
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

        var exePath = Assembly.GetExecutingAssembly().Location;
        var exeDir = Path.GetDirectoryName(exePath);
        if (exeDir == null)
            throw new Exception("Unable to find an installation directory for the CLI");

        var dllPath = Path.Combine(exeDir, "GodotPlugins.dll");

        if (!Path.Exists(dllPath))
            throw new Exception("GodotPlugins not found");

        Console.WriteLine($"Copying {dllPath}");

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
}
