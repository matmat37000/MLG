/*
* Run.cs - Code to install and patch a game
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
using System.Runtime.InteropServices;

namespace MLG.Installer;

internal static class Run
{
    internal static void RunGame(string gameExe, PlatformID platform)
    {
        switch (platform)
        {
            case PlatformID.Unix:
                RunGame_Unix(gameExe);
                break;
            case PlatformID.MacOSX:
                RunGame_MacOSX(gameExe);
                break;
            case PlatformID.Win32NT or PlatformID.WinCE:
                RunGame_Win(gameExe);
                break;
            default:
                throw new Exception("Unknown platform");
        }
    }

    internal static void CleanGame(string gameExe)
    {
        var gameName = Path.GetFileNameWithoutExtension(gameExe);
        var dllDir = Path.Combine(
            Path.GetDirectoryName(gameExe)
                ?? throw new NullReferenceException("Directory not found"),
            $"data_{gameName}_linuxbsd_x86_{RuntimeInformation.OSArchitecture.ToString()[1..]}" // get the dll directory name
        );
        var originalDllPath = Path.Combine(dllDir, gameName + ".dll");
        var newDllPath = originalDllPath.Replace(".dll", "_original.dll"); // Get the new path for the original dll

        DllManipulation.RestoreOriginalDll(originalDllPath, newDllPath);
        DllManipulation.RemoveDependencies(dllDir);
        Console.WriteLine("Done.");
    }

    private static void RunGame_Unix(string gameExe)
    {
        var gameName = Path.GetFileNameWithoutExtension(gameExe);
        var dllDir = Path.Combine(
            Path.GetDirectoryName(gameExe)
                ?? throw new NullReferenceException("Directory not found"),
            $"data_{gameName}_linuxbsd_x86_{RuntimeInformation.OSArchitecture.ToString()[1..]}" // get the dll directory name
        );
        var originalDllPath = Path.Combine(dllDir, gameName + ".dll");
        var newDllPath = originalDllPath.Replace(".dll", "_original.dll"); // Get the new path for the original dll

        CleanGame(gameExe);
        DllManipulation.CopyDll(originalDllPath, newDllPath);
        DllManipulation.ShimOriginalDll(originalDllPath, gameName);
        DllManipulation.CopyDependencies(dllDir);
        Console.WriteLine("Done.");
    }

    private static void RunGame_MacOSX(string gameExe) { }

    private static void RunGame_Win(string gameExe) { }
}
