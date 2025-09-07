using System.Reflection;
using System.Runtime.InteropServices;

namespace MLG.Installer;

internal static class Program
{
    private static void Main(string[] args)
    {
        var gameExe = HandleArgs(args);
        if (gameExe == null)
            return;

        Console.WriteLine(
            $"Platform:  {Environment.OSVersion.Platform} {RuntimeInformation.OSArchitecture}"
        );
        Console.WriteLine(
            "Installing in: " + Path.GetDirectoryName(gameExe)
                ?? throw new NullReferenceException("Directory not found")
        );

        AppDomain.CurrentDomain.AssemblyResolve += LoadDependencies;

        Run.RunGame(gameExe, Environment.OSVersion.Platform);
    }

    /// <summary>
    /// Try to load DLL from the DLLs dir instead of the execution directory.
    /// </summary>
    /// <returns>The assembly if found, else null</returns>
    private static Assembly? LoadDependencies(object? _, ResolveEventArgs args)
    {
        var requestedAssembly = new AssemblyName(args.Name);
        var path = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "DLLs",
            requestedAssembly.Name + ".dll"
        );
        return File.Exists(path) ? Assembly.LoadFrom(path) : null;
    }

    private static string? HandleArgs(string[] args)
    {
        if (args.Length < 1)
            args = ["-h"];

        switch (args[0])
        {
            case "-h" or "--help" or "-?" or "/?":
                Console.WriteLine("Usage: ModotInstaller <Game Executable>");
                break;
            case "clean":
                try
                {
                    if (args.GetValue(1) is string gameExe)
                    {
                        Console.WriteLine("Cleaning up...");
                        Run.CleanGame(gameExe);
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    Console.WriteLine("Please give the game executable");
                }

                break;
            default:
                return args[0];
        }

        return null;
    }
}
