using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientService.Controllers
{
    public static class JsonContoller
    {
        public static T ReadJsonFromFile<T> (string path)
        {
            T list;
            using (StreamReader file = File.OpenText(path))
            using (JsonTextReader reader = new JsonTextReader(file))
            {
                JObject jObject = (JObject)JToken.ReadFrom(reader);
                list = JsonConvert.DeserializeObject<T>(jObject.ToString());

                reader.Close();
                file.Close();
            }

            return list;
        }

        public static byte[] SerializeToBson<T>(T obj)
        {
            MemoryStream memory = new MemoryStream();
            using(BsonWriter writer = new BsonWriter(memory))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(writer, obj);
            }

            return memory.ToArray();
        }

        public static T DeserializeFromBson<T>(byte[] data)
        {
            T @object;

            MemoryStream memory = new MemoryStream(data);
            using(BsonReader reader = new BsonReader(memory))
            {
                JsonSerializer serializer = new JsonSerializer();
                @object = serializer.Deserialize<T>(reader);
            }

            return @object;
        }
    }
}
