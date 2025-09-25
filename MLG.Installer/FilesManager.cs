/*
* FilesManager.cs - Manage the folders and files of the installation of MLG
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
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MLG.Installer;

public static class FilesManager
{
    /// <summary>
    /// Searches for the game data directory where libraries are stored.
    /// Returns <c>null</c> if no matching directory is found.
    /// </summary>
    /// <param name="gameExe">The full path to the game executable.</param>
    /// <returns>
    /// The full path to the game data directory if found; otherwise, <c>null</c>.
    /// </returns>
    /// <exception cref="NullReferenceException">
    /// Thrown if the directory containing the game executable cannot be found.
    /// </exception>
    public static string? GetDataDirPath(string gameExe)
    {
        var parentDir =
            Path.GetDirectoryName(gameExe)
            ?? throw new NullReferenceException("Directory not found");
        var gameName = Path.GetFileNameWithoutExtension(gameExe);

        var dataDir = Directory
            .GetDirectories(parentDir, $"data_{gameName}_" + "*", SearchOption.TopDirectoryOnly)
            .FirstOrDefault();

        return dataDir;
    }

    /// <summary>
    /// Create the MLG folder structure
    /// </summary>
    /// <returns>Return true if success, else false</returns>
    public static bool CreateFolderStructure(string gamePath)
    {
        var baseDir = Path.Join(gamePath, "MLG");

        if (Directory.Exists(baseDir))
            return false;

        Directory.CreateDirectory(baseDir);
        foreach (var dir in (List<string>)["config", "core", "lib", "plugins"])
        {
            Directory.CreateDirectory(Path.Combine(baseDir, dir));
            Console.WriteLine($"Created {dir}");
        }

        return true;
    }

    /// <summary>
    /// Remove the MLG folder structure
    /// </summary>
    public static void RemoveFolderStructure(string gamePath)
    {
        var baseDir = Path.Join(gamePath, "MLG");
        if (Directory.Exists(baseDir))
            Directory.Delete(baseDir, recursive: true);
        Console.WriteLine($"Deleted {baseDir}");
    }
}
