using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;

namespace RP_Notify.Helpers
{
    public static class CookieHelper
    {
        public static bool ReadAndValidateCookieFromDisk(string cookieFilePath, Uri cookieUri, out CookieContainer resultCookieContainer)
        {
            if (CookieHelper.TryGetCookieFromCache(cookieFilePath, out resultCookieContainer))
            {
                var cookieCollection = resultCookieContainer.GetCookies(cookieUri);
                var cookieArray = new Cookie[cookieCollection.Count];
                cookieCollection.CopyTo(cookieArray, 0);

                if (!cookieArray.Any(c => c.Expired))
                {
                    return true;
                }
                else
                {
                    Retry.Do(() => File.Delete(cookieFilePath));
                }
            }

            return false;
        }

        private static bool TryGetCookieFromCache(string filePath, out CookieContainer cookieContainer)
        {
            cookieContainer = new CookieContainer();

            RenameOldCookieIfPresent(filePath);

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

        private static void RenameOldCookieIfPresent(string filePath)
        {
            var oldFilePath = Path.Combine(Path.GetDirectoryName(filePath), "_cookieCache");
            if (File.Exists(oldFilePath))
            {
                File.Move(oldFilePath, filePath);
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
        public static CookieCollection GetAllCookiesFromHeader(string strHeader, string strHost)
        {
            ArrayList al = new ArrayList();
            CookieCollection cc = new CookieCollection();
            if (strHeader != string.Empty)
            {
                al = ConvertCookieHeaderToArrayList(strHeader);
                cc = ConvertCookieArraysToCookieCollection(al, strHost);
            }
            return cc;
        }


        private static ArrayList ConvertCookieHeaderToArrayList(string strCookHeader)
        {
            strCookHeader = strCookHeader.Replace("\r", "");
            strCookHeader = strCookHeader.Replace("\n", "");
            string[] strCookTemp = strCookHeader.Split(',');
            ArrayList al = new ArrayList();
            int i = 0;
            int n = strCookTemp.Length;
            while (i < n)
            {
                if (strCookTemp[i].IndexOf("expires=", StringComparison.OrdinalIgnoreCase) > 0)
                {
                    al.Add(strCookTemp[i] + "," + strCookTemp[i + 1]);
                    i = i + 1;
                }
                else
                {
                    al.Add(strCookTemp[i]);
                }
                i = i + 1;
            }
            return al;
        }


        private static CookieCollection ConvertCookieArraysToCookieCollection(ArrayList al, string strHost)
        {
            CookieCollection cc = new CookieCollection();

            int alcount = al.Count;
            string strEachCook;
            string[] strEachCookParts;
            for (int i = 0; i < alcount; i++)
            {
                strEachCook = al[i].ToString();
                strEachCookParts = strEachCook.Split(';');
                int intEachCookPartsCount = strEachCookParts.Length;
                string strCNameAndCValue = string.Empty;
                string strPNameAndPValue = string.Empty;
                string strDNameAndDValue = string.Empty;
                string[] NameValuePairTemp;
                Cookie cookTemp = new Cookie();

                for (int j = 0; j < intEachCookPartsCount; j++)
                {
                    if (j == 0)
                    {
                        strCNameAndCValue = strEachCookParts[j];
                        if (strCNameAndCValue != string.Empty)
                        {
                            int firstEqual = strCNameAndCValue.IndexOf("=");
                            string firstName = strCNameAndCValue.Substring(0, firstEqual);
                            string allValue = strCNameAndCValue.Substring(firstEqual + 1, strCNameAndCValue.Length - (firstEqual + 1));
                            cookTemp.Name = firstName;
                            cookTemp.Value = allValue;
                        }
                        continue;
                    }
                    if (strEachCookParts[j].IndexOf("path", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        strPNameAndPValue = strEachCookParts[j];
                        if (strPNameAndPValue != string.Empty)
                        {
                            NameValuePairTemp = strPNameAndPValue.Split('=');
                            if (NameValuePairTemp[1] != string.Empty)
                            {
                                cookTemp.Path = NameValuePairTemp[1];
                            }
                            else
                            {
                                cookTemp.Path = "/";
                            }
                        }
                        continue;
                    }

                    if (strEachCookParts[j].IndexOf("domain", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        strDNameAndDValue = strEachCookParts[j];
                        if (strDNameAndDValue != string.Empty)
                        {
                            NameValuePairTemp = strDNameAndDValue.Split('=');

                            if (NameValuePairTemp[1] != string.Empty)
                            {
                                cookTemp.Domain = NameValuePairTemp[1];
                            }
                            else
                            {
                                cookTemp.Domain = strHost;
                            }
                        }
                        continue;
                    }
                }

                if (cookTemp.Path == string.Empty)
                {
                    cookTemp.Path = "/";
                }
                if (cookTemp.Domain == string.Empty)
                {
                    cookTemp.Domain = strHost;
                }
                cc.Add(cookTemp);
            }
            return cc;
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
