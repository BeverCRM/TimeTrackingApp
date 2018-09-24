using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Task_Time_Tracker.Model
{
    class Deserialiser
    {
        public static T DeserialiseObject<T>(string json)
        {
            DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(T));
            using (MemoryStream stream = new MemoryStream(Encoding.Unicode.GetBytes(json)))
            {
                T result = (T)deserializer.ReadObject(stream);
                return result;
            }
        }
    }
}
