using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace AddinManager.Helper
{
    static class JsonHelper
    {
        public static string SerializeObject<T>(T obj)
        {
            var jsonString = string.Empty;
            try
            {
                var serializer = new DataContractJsonSerializer(typeof(T));

                using (var ms = new MemoryStream())
                {
                    serializer.WriteObject(ms, obj);
                    jsonString = Encoding.UTF8.GetString(ms.ToArray());
                }
            }
            catch
            {
                jsonString = string.Empty;
            }
            return jsonString;
        }


        public static T DeserializeObject<T>(string jsonString)
        {
            var obj = Activator.CreateInstance<T>();

            try
            {
                using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonString)))
                {
                    var ser = new DataContractJsonSerializer(obj.GetType());//typeof(T)  
                    var jsonObject = (T)ser.ReadObject(ms);

                    ms.Close();

                    return jsonObject;
                }
            }
            catch
            {
                return default(T);
            }
        }
    }
}
