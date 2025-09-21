using System.Collections.Generic;
using System.IO;
using Godot.Collections;

namespace MLG.Installer;

public class FilesManager(string gamePath)
{
    /// <summary>
    /// Working directory
    /// </summary>
    public string GamePath { get; } = gamePath;

    /// <summary>
    /// Create the MLG folder structure
    /// </summary>
    /// <returns>Return true if success, else false</returns>
    public bool CreateFolderStructure()
    {
        var baseDir = Path.Join(GamePath, "MLG");

        if (Directory.Exists(baseDir))
            return false;

        Directory.CreateDirectory(baseDir);
        foreach (var dir in (List<string>)["config", "core", "lib", "plugins"])
        {
            Directory.CreateDirectory(Path.Combine(baseDir, dir));
        }

        return true;
    }

    /// <summary>
    /// Remove the MLG folder structure
    /// </summary>
    public void RemoveFolderStructure()
    {
        var baseDir = Path.Join(GamePath, "MLG");
        if (Directory.Exists(baseDir))
            Directory.Delete(baseDir, recursive: true);
    }
}
