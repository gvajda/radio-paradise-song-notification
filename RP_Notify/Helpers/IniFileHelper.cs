using MadMilkman.Ini;
using RP_Notify.ErrorHandler;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace RP_Notify.Helpers
{
    internal class IniFileHelper
    {

        internal readonly string _iniPath;
        internal readonly string _iniFolder;

        private const string appCacheFolderName = "RP_Notify_Cache";
        private const string configFileName = "config.ini";

        internal IniFileHelper()
        {

            _iniPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                appCacheFolderName,
                configFileName);

            var exeContainingDirecoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory);
            var customAppCacheFolderName = Path.Combine(exeContainingDirecoryPath, appCacheFolderName);

            if (Directory.Exists(exeContainingDirecoryPath))
            {
                _iniPath = Path.Combine(customAppCacheFolderName, configFileName);
            }

            if (CheckForCustomIniPath(out string iniPathFromArg))
            {
                _iniPath = iniPathFromArg;
            }

            _iniFolder = Path.GetDirectoryName(_iniPath);

            CreateIniWithDefaultValuesIfNotExists(_iniPath);
            CheckIniIntegrity();
        }

        internal IniFile ReadIniFile()
        {
            var iniFile = new IniFile();
            var iniContent = TryReadIniStringContent();
            iniFile.Load(new StringReader(iniContent));
            return iniFile;
        }

        internal void CheckIniIntegrity()
        {
            var defaultIniFile = new IniFile();
            defaultIniFile.Load(new StringReader(Properties.Resources.config));

            var iniFile = ReadIniFile();

            foreach (var defaultSection in defaultIniFile.Sections)
            {
                if (!iniFile.Sections.Where(s => s.Name == defaultSection.Name).Any())
                {
                    iniFile.Sections.Add(defaultSection.Name);
                }

                foreach (var defaultKey in defaultSection.Keys)
                {
                    if (!iniFile
                        .Sections[defaultSection.Name]
                        .Keys.Where(k => k.Name == defaultKey.Name)
                        .Any()
                        )
                    {
                        iniFile.Sections[defaultSection.Name].Keys.Add(defaultKey.Name, defaultKey.Value);
                    }
                }
            }

            iniFile.Save(_iniPath);
        }

        private void CreateIniWithDefaultValuesIfNotExists(string pIniPath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(pIniPath));
            if (!File.Exists(pIniPath))
            {
                File.WriteAllText(pIniPath, Properties.Resources.config);
            }
        }

        private bool CheckForCustomIniPath(out string iniPathFromArg)
        {
            iniPathFromArg = null;

            foreach (string arg in Environment.GetCommandLineArgs().Where(arg => arg.EndsWith(".ini")))
            {
                if (File.Exists(arg) && !string.IsNullOrEmpty(File.ReadAllText(arg)))
                {
                    iniPathFromArg = arg;
                    return true;
                }

                try
                {   // Should run once
                    CreateIniWithDefaultValuesIfNotExists(arg);
                    iniPathFromArg = arg;
                    return true;
                }
                catch (Exception)
                {
                    continue;
                }
            }

            return false;
        }

        private string TryReadIniStringContent()
        {
            return Retry.Do(() =>
            {
                string iniContent = null;
                iniContent = File.ReadAllText(_iniPath, Encoding.Default);

                return iniContent;
            });
        }
    }
}
