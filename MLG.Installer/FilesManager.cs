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
            Console.WriteLine($"Created {dir}");
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
        Console.WriteLine($"Deleted {baseDir}");
    }
}
