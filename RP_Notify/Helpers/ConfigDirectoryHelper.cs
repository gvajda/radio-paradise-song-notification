using System;
using System.IO;

namespace RP_Notify.Helpers
{
    internal static class ConfigDirectoryHelper
    {
        private const string appConfigDirectoryName = Constants.ConfigBaseFolder;
        private const string obsoleteAppConfigDirectoryName = Constants.ObsoleteConfigBaseFolder;

        internal static bool MoveConfigToNewLocation(ConfigLocationOptions currentConfigLocationOptions, ConfigLocationOptions targetConfigLocationOptions)
        {
            var currentPath = GetLocalPath(currentConfigLocationOptions);
            var targetPath = GetLocalPath(targetConfigLocationOptions);

            if (Directory.Exists(currentPath))
            {
                if (Directory.Exists(targetPath))
                {
                    Directory.Delete(targetPath);
                }

                Directory.Move(currentPath, targetPath);

                return true;
            }

            return false;
        }

        internal static bool TryFindConfigDirectory(out ConfigLocationOptions configLocationOptions)
        {
            var appDataPath = GetLocalPath(ConfigLocationOptions.AppData);
            var obsoleteAppDataPath = GetLocalPath(ConfigLocationOptions.ObsoleteAppdata);
            var exeContainingDirectoryPath = GetLocalPath(ConfigLocationOptions.ExeContainingDirectory);

            if (Directory.Exists(exeContainingDirectoryPath))
            {
                configLocationOptions = ConfigLocationOptions.ExeContainingDirectory;
                return true;
            }

            if (Directory.Exists(obsoleteAppDataPath))
            {
                Directory.Move(obsoleteAppDataPath, appDataPath);
                configLocationOptions = ConfigLocationOptions.AppData;
                return true;
            }

            if (Directory.Exists(appDataPath))
            {
                configLocationOptions = ConfigLocationOptions.AppData;
                return true;
            }

            configLocationOptions = ConfigLocationOptions.AppData;
            return false;
        }

        internal static void DeleteConfigDirectory(ConfigLocationOptions configLocationOptions)
        {
            var path = GetLocalPath(configLocationOptions);

            if (Directory.Exists(path))
            {
                Retry.Do(() => Directory.Delete(path, true));
            }
        }

        internal static string GetLocalPath(ConfigLocationOptions configLocationOptions)
        {
            switch (configLocationOptions)
            {
                case ConfigLocationOptions.AppData:
                    return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), appConfigDirectoryName);
                case ConfigLocationOptions.ObsoleteAppdata:
                    return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), obsoleteAppConfigDirectoryName);
                case ConfigLocationOptions.ExeContainingDirectory:
                    return Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, appConfigDirectoryName);
                default:
                    throw new NotImplementedException();
            }
        }
    }

    public enum ConfigLocationOptions
    {
        AppData,
        ObsoleteAppdata,
        ExeContainingDirectory
    }
}


