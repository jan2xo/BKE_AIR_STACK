using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace BKE_Air_Stack
{
    public partial class Form1
    {
        // Form1.cs is legacy code and calls Process.Start directly. Keep that
        // surface intact while giving vMix a clean launch boundary: resolve the
        // installed executable from Windows Program Files and run it from its
        // own directory instead of inheriting Air Stack's self-contained .NET
        // runtime directory.
        private static class Process
        {
            public static System.Diagnostics.Process Start(string fileName, string arguments)
            {
                if (string.Equals(Path.GetFileName(fileName), "vMix64.exe", StringComparison.OrdinalIgnoreCase))
                {
                    string vmixPath = FindVmixExecutable();
                    string workingDirectory = Path.GetDirectoryName(vmixPath)
                        ?? throw new InvalidOperationException("The vMix installation directory could not be resolved.");

                    return System.Diagnostics.Process.Start(new ProcessStartInfo
                    {
                        FileName = vmixPath,
                        Arguments = arguments,
                        WorkingDirectory = workingDirectory,
                        UseShellExecute = true
                    });
                }

                return System.Diagnostics.Process.Start(fileName, arguments);
            }

            public static System.Diagnostics.Process Start(ProcessStartInfo startInfo)
            {
                return System.Diagnostics.Process.Start(startInfo);
            }

            public static System.Diagnostics.Process Start(string fileName)
            {
                return System.Diagnostics.Process.Start(fileName);
            }

            public static System.Diagnostics.Process[] GetProcesses()
            {
                return System.Diagnostics.Process.GetProcesses();
            }

            public static System.Diagnostics.Process[] GetProcessesByName(string processName)
            {
                return System.Diagnostics.Process.GetProcessesByName(processName);
            }

            private static string FindVmixExecutable()
            {
                var roots = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                AddProgramFilesRoot(roots, Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));
                AddProgramFilesRoot(roots, Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86));
                AddProgramFilesRoot(roots, Environment.GetEnvironmentVariable("ProgramW6432"));
                AddProgramFilesRoot(roots, Environment.GetEnvironmentVariable("ProgramFiles(x86)"));

                var options = new EnumerationOptions
                {
                    RecurseSubdirectories = true,
                    IgnoreInaccessible = true,
                    MatchCasing = MatchCasing.CaseInsensitive,
                    ReturnSpecialDirectories = false
                };

                foreach (string root in roots.Where(Directory.Exists))
                {
                    string found = Directory
                        .EnumerateFiles(root, "vMix64.exe", options)
                        .FirstOrDefault();

                    if (!string.IsNullOrWhiteSpace(found))
                    {
                        return found;
                    }
                }

                throw new FileNotFoundException(
                    "vMix64.exe was not found under the Windows Program Files directories. Please install vMix before opening an Air Stack template.");
            }

            private static void AddProgramFilesRoot(ISet<string> roots, string root)
            {
                if (!string.IsNullOrWhiteSpace(root))
                {
                    roots.Add(root);
                }
            }
        }
    }
}
