using RP_Notify.ErrorHandler;
using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;

namespace RP_Notify.API
{
    public static class CookieHelper
    {
        public static bool TryGetCookieFromCache(string filePath, out CookieContainer cookieContainer)
        {
            cookieContainer = null;

            if (!File.Exists(filePath) || !(File.ReadAllText(filePath).Length > 0))
            {
                return false;
            }

            try
            {
                cookieContainer = Retry.Do(() => ReadCookieFromDisk(filePath));
                return true;
            }
            catch (Exception)
            {
                Retry.Do(() => File.Delete(filePath));
                return false;
            }
        }

        public static bool TryWriteCookieToDisk(string filePath, CookieContainer cookieJar)
        {
            if (cookieJar != null && cookieJar.Count > 0)
            {
                try
                {
                    Retry.Do(() => WriteCookieToDisk(filePath, cookieJar));
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private static CookieContainer ReadCookieFromDisk(string filePath)
        {
            using (Stream stream = File.Open(filePath, FileMode.Open))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                return (CookieContainer)formatter.Deserialize(stream);
            }
        }

        private static void WriteCookieToDisk(string filePath, CookieContainer cookieJar)
        {
            using (Stream stream = File.Create(filePath))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, cookieJar);
            }
        }
    }
}
