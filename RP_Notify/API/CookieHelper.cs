using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;

namespace RP_Notify.API
{
    public class CookieHelper
    {
        public static CookieContainer TryGetCookiesFromCache(string filePath)
        {
            if (!File.Exists(filePath) || !(File.ReadAllText(filePath).Length > 0))
            {
                return null;
            }
            try
            {
                using (Stream stream = File.Open(filePath, FileMode.Open))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    return (CookieContainer)formatter.Deserialize(stream);
                }
            }
            catch (Exception)
            {

                return null;
            }
        }
        public static void WriteCookiesToDisk(string filePath, CookieContainer cookieJar)
        {
            if (cookieJar != null && cookieJar.Count > 0)
            {
                using (Stream stream = File.Create(filePath))
                {
                    try
                    {
                        BinaryFormatter formatter = new BinaryFormatter();
                        formatter.Serialize(stream, cookieJar);
                    }
                    catch (Exception)
                    {
                        //TODO
                    }
                }
            }

        }

        static CookieContainer ReadCookiesFromDisk(string filePath)
        {

            try
            {
                using (Stream stream = File.Open(filePath, FileMode.Open))
                {
                    Console.Out.Write("Reading cookies from disk... ");
                    BinaryFormatter formatter = new BinaryFormatter();
                    Console.Out.WriteLine("Done.");
                    return (CookieContainer)formatter.Deserialize(stream);
                }
            }
            catch (Exception)
            {
                return new CookieContainer();
            }
        }
    }
}
