using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;
using System.IO;

namespace DLPEngineLibrary.Controllers
{
    internal static class JsonController
    {
        internal static T ReadObjectFromJsonFile<T> (string path)
        {
            T @object = default(T);

            using (var stream = File.OpenText(path))
            {
                using (JsonTextReader reader = new JsonTextReader(stream))
                {
                    var token = JToken.ReadFrom(reader);
                    switch (token)
                    {
                        case JObject obj:
                            @object = JsonConvert.DeserializeObject<T>(obj.ToString());
                            break;

                        case JArray array:
                            @object = JsonConvert.DeserializeObject<T>(array.ToString());
                            break;
                    }
                    reader.Close();
                }
                stream.Close();
            }
            return @object;
        }

        internal static void WriteObjectToJsonFile<T>(string path, T @object)
        {
            using(var file = File.Open(path, FileMode.Open))
            using (StreamWriter stream = new StreamWriter(file))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(stream, @object);

                stream.Close();
                file.Close();
            }
        }

        internal static byte[] SerializeToBson<T>(T @object)
        {
            if (object.Equals(@object, default(T)))
                return null;

            MemoryStream memory = new MemoryStream();
            using(BsonWriter writer = new BsonWriter(memory))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(writer, @object);
            }

            return memory.ToArray();
        }

        internal static T DeserializeFromBson<T>(byte[] data)
        {
            if (data == null)
                return default(T);

            T @object = default(T);

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
