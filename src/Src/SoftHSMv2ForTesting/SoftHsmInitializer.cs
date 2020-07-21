using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftHSMv2ForTesting
{
    /// <summary>
    /// The SoftHSMv2 initializer.
    /// </summary>
    public static class SoftHsmInitializer
    {
        /// <summary>
        /// Initializes SoftHSMv2 with specified settings.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns>The SoftHSMv2 context.</returns>
        /// <exception cref="ArgumentNullException">settings</exception>
        public static SoftHsmContext Init(SoftHsmSettings settings)
        {
            PlatformHelper.CheckPlatform();
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            CheckSettings(settings);

            SoftHsmFolderStructure paths = InitFileStructures(settings);

            Environment.SetEnvironmentVariable("SOFTHSM2_CONF", paths.ConfigFilePath, EnvironmentVariableTarget.Process);

            string path = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("PATH", string.Concat(path, ";", paths.LibFilderPath), EnvironmentVariableTarget.Process);
            InitToken(paths.UtilPath, 0U, settings.LabelName, settings.Pin, settings.SoPin);

            return new SoftHsmContext(paths.BasePath);
        }

        /// <summary>
        /// Initializes SoftHSMv2 with specified options.
        /// </summary>
        /// <param name="optionsAction">The options.</param>
        /// <returns>
        /// The SoftHSMv2 context.
        /// </returns>
        /// <exception cref="ArgumentNullException">optionsAction</exception>
        public static SoftHsmContext Init(Action<SoftHsmSettings> optionsAction)
        {
            PlatformHelper.CheckPlatform();
            if (optionsAction == null)
            {
                throw new ArgumentNullException(nameof(optionsAction));
            }

            SoftHsmSettings settings = new SoftHsmSettings();
            optionsAction.Invoke(settings);

            return Init(settings);
        }

        internal static void InitToken(string utilPath, ulong slotNumber, string labelName, string pin, string soPin)
        {
            ProcessStartInfo info = new ProcessStartInfo()
            {
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = utilPath,
                WorkingDirectory = Path.GetDirectoryName(utilPath),
                Arguments = $"--init-token --slot {slotNumber} --label {EscapeCmdArg(labelName)} --so-pin {EscapeCmdArg(soPin)} --pin {EscapeCmdArg(pin)}"
            };

            using (Process process = Process.Start(info))
            {
                process.WaitForExit(5000);
                if (process.ExitCode != 0)
                {
                    throw new InvalidProgramException($"softhsm2-util.exe exited with {process.ExitCode}");
                }
            }
        }

        private static SoftHsmFolderStructure InitFileStructures(SoftHsmSettings settings)
        {
            string basePath = settings.DeployFolder ?? Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("D"));

            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }

            Directory.CreateDirectory(Path.Combine(basePath, "bin"));
            Directory.CreateDirectory(Path.Combine(basePath, "etc"));
            Directory.CreateDirectory(Path.Combine(basePath, "lib"));
            Directory.CreateDirectory(Path.Combine(basePath, "token"));

            GzipHelper.ExtractContentFileTo("READMY.txt.gz", Path.Combine(basePath, "READMY.txt"));
            GzipHelper.ExtractContentFileTo("softhsm2-util.exe.gz", Path.Combine(basePath, "bin", "softhsm2-util.exe"));
            GzipHelper.ExtractContentFileTo("softhsm2-x64.dll.gz", Path.Combine(basePath, "lib", "softhsm2-x64.dll"));
            GzipHelper.ExtractContentFileTo("softhsm2.dll.gz", Path.Combine(basePath, "lib", "softhsm2.dll"));


            File.WriteAllLines(Path.Combine(basePath, "etc", "softhsm2.conf"),
                new string[]
                {
                    "# SoftHSM v2 configuration file",
                    string.Empty,
                    $"directories.tokendir = {Path.GetFullPath(Path.Combine(basePath, "token"))}",
                    "objectstore.backend = file",
                    "log.level = INFO",
                    "slots.removable = false",
                    string.Empty
                });

            return new SoftHsmFolderStructure()
            {
                ConfigFilePath = Path.GetFullPath(Path.Combine(basePath, "etc", "softhsm2.conf")),
                UtilPath = Path.GetFullPath(Path.Combine(basePath, "bin", "softhsm2-util.exe")),
                TokenPath = Path.GetFullPath(Path.Combine(basePath, "token")),
                LibFilderPath = Path.GetFullPath(Path.Combine(basePath, "lib")),
                BasePath = basePath
            };
        }

        private static void CheckSettings(SoftHsmSettings settings)
        {
            if (string.IsNullOrEmpty(settings.DeployFolder))
            {
                throw new ArgumentException("DeployFolder is not set.");
            }

            if (string.IsNullOrEmpty(settings.LabelName))
            {
                throw new ArgumentException("LabelName is not set.");
            }

            if (string.IsNullOrEmpty(settings.Pin))
            {
                throw new ArgumentException("Pin is not set.");
            }

            if (string.IsNullOrEmpty(settings.SoPin))
            {
                throw new ArgumentException("SoPin is not set.");
            }
        }

        private static string EscapeCmdArg(string argument)
        {
            string escapedArgument = argument.Replace("\\", "\\\\").Replace("\"", "\\\"");
            if (argument.Any(t => char.IsWhiteSpace(t)))
            {
                return string.Concat("\"", escapedArgument, "\"");
            }
            else
            {
                return escapedArgument;
            }
        }
    }
}
