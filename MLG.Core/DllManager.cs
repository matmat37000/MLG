using System.Reflection;
using Godot.Collections;

namespace MLG.Core;

public sealed class DllManager
{
    // public static string? WorkingDirectory { get; set; }
    //
    // public DllManager(string workingDirectory)
    // {
    //     WorkingDirectory = workingDirectory;
    //     AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;
    // }
    //
    // public static Assembly? AssemblyResolve(object? sender, ResolveEventArgs args)
    // {
    //     if (WorkingDirectory == null)
    //         return null;
    //
    //     Console.WriteLine($"{sender} is requesting {args.Name}...");
    //
    //     foreach (var folder in new Array<string>() { "Core", "Lib", "Plugin" })
    //     {
    //         var path = Path.Combine(WorkingDirectory, "MLG", folder, $"{args.Name}.dll");
    //         if (File.Exists(path))
    //         {
    //             Console.WriteLine("Loading DLL...");
    //             return Assembly.LoadFrom(path);
    //         }
    //     }
    //     return null;
    // }
}
