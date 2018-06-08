using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace NetworkServiceNotifyUtility.Utility
{
    public class SerializeUtility
    {
        public static string Serialize<T>(T obj)
        {
            string resultString = string.Empty;
            try
            {
                using (var ms = new MemoryStream())
                {
                    var serializer = new DataContractJsonSerializer(typeof(T), new DataContractJsonSerializerSettings { SerializeReadOnlyTypes = true });
                    serializer.WriteObject(ms, obj);
                    ms.Position = 0;
                    resultString = new StreamReader(ms).ReadToEnd();
                }
            }
            catch (Exception)
            {
                resultString = string.Empty;
            }
            return resultString;
        }

        public static T DeSerialize<T>(string json) where T : class
        {
            T obj = null;
            try
            {
                using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
                {
                    ms.Position = 0;
                    var serializer = new DataContractJsonSerializer(typeof(T));
                    obj = serializer.ReadObject(ms) as T;
                }
            }
            catch (SerializationException)
            {
                obj = null;
            }
            return obj;
        }
    }
}
