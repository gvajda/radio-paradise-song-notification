using RP_Notify.ErrorHandler;
using System;
using System.IO;

namespace RP_Notify.Helpers
{
    internal static class ConfigDirectoryHelper
    {
        private const string appConfigDirectoryName = "RP_Notify_Cache";

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
            var exeContainingDirectoryPath = GetLocalPath(ConfigLocationOptions.ExeContainingDirectory);

            if (Directory.Exists(exeContainingDirectoryPath))
            {
                configLocationOptions = ConfigLocationOptions.ExeContainingDirectory;
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
        ExeContainingDirectory
    }
}


