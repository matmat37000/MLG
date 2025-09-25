/*
* Program.cs - Entry point of the installer
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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace MLG.Installer;

internal static class Program
{
    private static void Main(string[] args)
    {
        HandleArgs(args);
    }

    /// <summary>
    /// Processes and handles the command-line arguments provided to the program.
    /// </summary>
    /// <param name="args">An array of command-line arguments passed to the program.</param>
    private static void HandleArgs(string[] args)
    {
        // Show help if not argument passed
        if (args.Length == 0)
        {
            ShowHelp();
        }
        else
        {
            // Get the game executable path from the arguments
            var gameExe = args.Skip(1).FirstOrDefault();

            switch (args[0])
            {
                case "--help" or "-h":
                    ShowHelp();
                    break;
                case "--install" or "-i":
                    // Write the help message and error message
                    if (gameExe == null)
                    {
                        Console.WriteLine("Game exe not found in arguments");
                        ShowHelp();
                        return;
                    }
                    Command.InstallInGame(gameExe);
                    break;
                case "--uninstall" or "-u":
                    // Write the help message and error message
                    if (gameExe == null)
                    {
                        Console.WriteLine("Game exe not found in arguments");
                        ShowHelp();
                        return;
                    }
                    var gameName = Path.GetFileNameWithoutExtension(gameExe);
                    var dataDir = FilesManager.GetDataDirPath(gameExe);
                    if (dataDir == null)
                    {
                        Console.WriteLine("Data directory not found");
                        break;
                    }
                    Command.CleanGame(gameName, dataDir);
                    break;
            }
        }
    }

    /// <summary>
    /// Write the help message to the console
    /// </summary>
    private static void ShowHelp()
    {
        Console.WriteLine(
            """

            MLG Installer

            Usage:
                -h, --help
                    Show this help message.

                -i <game executable>, --install <game executable>
                    Install MLG into the game.

                -u <game executable>, --uninstall <game executable>
                    Uninstall MLG from the game.

            """
        );
    }
}
