using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace Matritsa.LegacyLauncher {
    internal class Program {
        static void Main(string[] args) {
            CultureInfo ci = CultureInfo.InstalledUICulture;
            IStrings strings = new EnglishStrings();
            if (ci.Name == "ru-RU") {
                strings = new RussianStrings();
            }
            string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string dnArgument = "matrigen.dll";
            if (args.Length >= 2) {
                if (args[0] == "--matrigen-location") {
                    string mtrgLocation = "Debug";
                    if (args[1] == "debug") {
                        mtrgLocation = "Debug";
                    } else if (args[1] == "release") {
                        mtrgLocation = "Release";
                    } else {
                        MessageBox.Show(
                            strings.UnknownDebugMatrigen,
                            strings.Error,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );
                        Application.Exit();
                    }
                    dnArgument = Path.Combine(
                        "..",
                        "..",
                        "..",
                        "..",
                        "matritsa",
                        "matritsa.Desktop",
                        "bin",
                         mtrgLocation,
                        "net6.0",
                        dnArgument
                    );
                }
            }
            string dotnet = Path.Combine(programFiles, Path.Combine("dotnet", "dotnet.exe"));
            if (!File.Exists(dotnet)) {
                MessageBox.Show(
                    strings.NoDotnet,
                    strings.Error,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }
            if (!File.Exists(dnArgument)) {
                MessageBox.Show(
                    strings.NoMatrigen,
                    strings.Error,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }

            ProcessStartInfo psi = new ProcessStartInfo(dotnet, dnArgument) {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            Process prc = Process.Start(psi);
            prc.WaitForExit();

            string stderr = prc.StandardError.ReadToEnd();
            if (prc.ExitCode != 0) {
                if (stderr.Contains("file was not found")) {
                    MessageBox.Show(
                        strings.NoMatrigen,
                        strings.Error,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                } else {
                    MessageBox.Show(
                        string.Format(strings.UnknownError, stderr),
                        strings.Error,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
            Console.ReadLine();
        }
    }
}
