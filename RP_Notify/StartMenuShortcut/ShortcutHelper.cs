using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using MS.WindowsAPICodePack.Internal;
using RP_Notify.Config;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace RP_Notify.StartMenuShortcut
{
    public class ShortcutHelper
    {
        private readonly IConfig _config;
        private readonly string shortcutPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Microsoft\\Windows\\Start Menu\\Programs\\RP Notify.lnk";

        // In order to display toasts, a desktop application must have a shortcut on the Start menu.
        // Also, an AppUserModelID must be set on that shortcut.
        // The shortcut should be created as part of the installer. The following code shows how to create
        // a shortcut and assign an AppUserModelID using Windows APIs. You must download and include the 
        // Windows API Code Pack for Microsoft .NET Framework for this code to function
        //
        // Included in this project is a wxs file that be used with the WiX toolkit
        // to make an installer that creates the necessary shortcut. One or the other should be used.

        public ShortcutHelper(IConfig config)
        {
            _config = config;
        }

        public bool TryCreateShortcut()
        {
            if (!File.Exists(shortcutPath))
            {
                InstallShortcut(shortcutPath);
                Thread.Sleep(3000);
                return true;
            }
            return false;
        }

        public void DeleteShortcut()
        {
            if (File.Exists(shortcutPath))
            {
                File.Delete(shortcutPath);
            }
        }

        private void InstallShortcut(String shortcutPath)
        {
            // Find the path to the current executable
            String exePath = Process.GetCurrentProcess().MainModule.FileName;
            IShellLinkW newShortcut = (IShellLinkW)new CShellLink();

            // Create a shortcut to the exe
            ErrorHelper.VerifySucceeded(newShortcut.SetPath(exePath));
            ErrorHelper.VerifySucceeded(newShortcut.SetArguments(""));

            // Open the shortcut property store, set the AppUserModelId property
            IPropertyStore newShortcutProperties = (IPropertyStore)newShortcut;

            using (PropVariant appId = new PropVariant(_config.StaticConfig.ToastAppID))
            {
                ErrorHelper.VerifySucceeded(newShortcutProperties.SetValue(SystemProperties.System.AppUserModel.ID, appId));
                ErrorHelper.VerifySucceeded(newShortcutProperties.Commit());
            }

            using (PropVariant guid = new PropVariant(_config.StaticConfig.ToastActivatorCLSID))
            {
                ErrorHelper.VerifySucceeded(newShortcutProperties.SetValue(SystemProperties.System.AppUserModel.ToastActivatorCLSID, guid));
                ErrorHelper.VerifySucceeded(newShortcutProperties.Commit());
            }

            // Commit the shortcut to disk
            IPersistFile newShortcutSave = (IPersistFile)newShortcut;

            ErrorHelper.VerifySucceeded(newShortcutSave.Save(shortcutPath, true));
        }

        // Create and show the toast.
        // See the "Toasts" sample for more detail on what can be done with toasts

    }
}
