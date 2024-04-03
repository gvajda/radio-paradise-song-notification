using System;
using System.ComponentModel;

namespace RP_Notify.ToastHandler
{
    public enum RpToastUserAction
    {
        ConfigFolderChosen,
        FolderChoiceRefusedExitApp,
        LoginRequested,
        RatingToastRequested,
        RateSubmitted
    }

    public enum ConfigFolderChoiceOption
    {
        [Description("LocalCache")]
        localCache,
        [Description("AppData")]
        appdata,
        [Description("CleanOnExit")]
        cleanonexit
    }

    public static class ConfigFolderChoiceOptionExtensions
    {
        public static string ToDescriptionString(this Enum val)
        {
            DescriptionAttribute[] attributes = (DescriptionAttribute[])val
               .GetType()
               .GetField(val.ToString())
               .GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes.Length > 0 ? attributes[0].Description : string.Empty;
        }
    }
}
