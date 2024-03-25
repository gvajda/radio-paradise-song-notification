using MadMilkman.Ini;
using RP_Notify.ErrorHandler;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace RP_Notify.Helpers
{
    internal static class IniFileHelper
    {
        internal static IniFile ReadIniFile(string iniPath)
        {
            var iniFile = new IniFile();
            string iniContent = null;

            Retry.Do(() =>
            {
                iniContent = File.ReadAllText(iniPath, Encoding.Default);
            });

            iniFile.Load(new StringReader(iniContent));
            return iniFile;
        }

        internal static void CheckIniIntegrity(string iniPath)
        {
            var defaultIniFile = new IniFile();
            defaultIniFile.Load(new StringReader(Properties.Resources.config));

            var iniFile = ReadIniFile(iniPath);

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

            iniFile.Save(iniPath);
        }

        internal static void CreateIniWithDefaultValuesIfNotExists(string iniPath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(iniPath));
            if (!File.Exists(iniPath))
            {
                File.WriteAllText(iniPath, Properties.Resources.config);
            }
        }
    }
}
