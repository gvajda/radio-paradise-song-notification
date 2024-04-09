namespace RP_Notify.Helpers
{
    using System;
    using System.Text;
    using System.Text.Json;

    public static class ObjectSerializer
    {
        public static string SerializeToBase64(object obj)
        {
            try
            {
                string jsonStr = JsonSerializer.Serialize(obj);
                byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonStr);
                string base64Str = Convert.ToBase64String(jsonBytes);
                return base64Str;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error during serialization: {e.Message}");
                return null;
            }
        }

        public static T DeserializeFromBase64<T>(string base64Str)
        {
            try
            {
                byte[] jsonBytes = Convert.FromBase64String(base64Str);
                string jsonStr = Encoding.UTF8.GetString(jsonBytes);
                T obj = JsonSerializer.Deserialize<T>(jsonStr);
                return obj;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error during deserialization: {e.Message}");
                return default;
            }
        }
    }

}
