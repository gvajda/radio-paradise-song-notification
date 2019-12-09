
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using MS.WindowsAPICodePack.Internal;
using RP_Notify.Toast.Helpers;
using System;
using System.Diagnostics;
using System.IO;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace Ctc
{
    internal class Toaster
    {
        
        // In order to display toasts, a desktop application must have a shortcut on the Start menu.
        // Also, an AppUserModelID must be set on that shortcut.
        // The shortcut should be created as part of the installer. The following code shows how to create
        // a shortcut and assign an AppUserModelID using Windows APIs. You must download and include the
        // Windows API Code Pack for Microsoft .NET Framework for this code to function
        //
        // Included in this project is a wxs file that be used with the WiX toolkit
        // to make an installer that creates the necessary shortcut. One or the other should be used.
        public bool TryCreateShortcut(string pAppId, )
        {
            string shortcutPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Microsoft\\Windows\\Start Menu\\Programs\\Desktop Toasts Sample CS.lnk";
            if (!File.Exists(shortcutPath))
            {
                InstallShortcut(shortcutPath);
                return true;
            }
            return false;
        }


        private void InstallShortcut(string shortcutPath, string pAppId)
        {
            // Find the path to the current executable
            string exePath = Process.GetCurrentProcess().MainModule.FileName;
            IShellLinkW newShortcut = (IShellLinkW)new CShellLink();

            // Create a shortcut to the exe
            ErrorHelper.VerifySucceeded(newShortcut.SetPath(exePath));
            ErrorHelper.VerifySucceeded(newShortcut.SetArguments("this was a callback"));
            newShortcut.SetShowCmd(0);
            // Open the shortcut property store, set the AppUserModelId property
            IPropertyStore newShortcutProperties = (IPropertyStore)newShortcut;

            using (PropVariant appId = new PropVariant(pAppId))
            {
                ErrorHelper.VerifySucceeded(newShortcutProperties.SetValue(SystemProperties.System.AppUserModel.ID, appId));
                ErrorHelper.VerifySucceeded(newShortcutProperties.Commit());
            }

            // Commit the shortcut to disk
            IPersistFile newShortcutSave = (IPersistFile)newShortcut;

            ErrorHelper.VerifySucceeded(newShortcutSave.Save(shortcutPath, true));
        }

        private void SetValue(XmlDocument toastXml, XmlNodeList stringElements, int idx, string value)
        {
            if (stringElements.Count > idx)
            {
                stringElements[idx].AppendChild(toastXml.CreateTextNode(value));
            }
        }
    }
}
