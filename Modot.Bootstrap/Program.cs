using System.Reflection;
using System.Runtime.InteropServices;
using Mono.Cecil;

namespace Modot.Bootstrap;

class Program
{
    static void Main(string[] args)
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

        Run.RunGame(gameExe, Environment.OSVersion.Platform);
    }

    static string? HandleArgs(string[] args)
    {
        if (args.Length < 1)
            args[0] = "-h";

        switch (args[0])
        {
            case "-h":
            case "--help":
            case "-?":
            case "/?":
                Console.WriteLine("Usage: ModotInstaller <Game Executable>");
                break;
            default:
                return args[0];
        }

        return null;
    }
}
