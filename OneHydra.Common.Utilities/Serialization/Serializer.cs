using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OneHydra.Common.Utilities.Serialization
{
    public static class Serializer
    {
        private static readonly JsonSerializer JsonSerializer = new JsonSerializer
        {
            //Include null values when serializing and deserializing objects.
            NullValueHandling = NullValueHandling.Include,
            // Always create new objects when deserializing
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            //Ignore a missing member and do not attempt to deserialize it.
            MissingMemberHandling = MissingMemberHandling.Ignore,
            //Ignore loop references and do not serialize.
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        private static void SerializeToFile<T>(FileInfo file, T data, IFormatter formatter)
        {
            var fileStream = new FileStream(file.FullName, FileMode.Create);
            //serialize the object and close the stream
            formatter.Serialize(fileStream, data);
        }

        public static void BinarySerializeToFile<T>(FileInfo file, T data)
        {
            var binaryFormatter = new BinaryFormatter(null, new StreamingContext(StreamingContextStates.File));
            SerializeToFile(file, data, binaryFormatter);

        }

        public static void SoapSerializeToFile<T>(FileInfo file, T data)
        {
            var soapFormatter = new SoapFormatter();
            SerializeToFile(file, data, soapFormatter);
        }

        private static T DeserializeFromFile<T>(FileInfo file, IFormatter formatter)
        {
            T returnValue;
            //open the stream from input path
            using (var fileStream = new FileStream(file.FullName, FileMode.Open))
            {
                //deserialize the object from the stream
                returnValue = (T)formatter.Deserialize(fileStream);
            }
            return returnValue;
        }

        public static T BinaryDeserialize<T>(FileInfo file)
        {
            //create a soap formater
            var binaryFormatter = new BinaryFormatter(null, new StreamingContext(StreamingContextStates.File));
            return DeserializeFromFile<T>(file, binaryFormatter);

        }

        public static T SoapDeserialize<T>(FileInfo file)
        {
            //create a soap formater
            var soapFormatter = new SoapFormatter();
            return DeserializeFromFile<T>(file, soapFormatter);
        }

        public static string SerializeToJsonString(object value)
        {
            var sw = new StringWriter();
            var writer = new JsonTextWriter(sw) { QuoteChar = '"' };

            JsonSerializer.Serialize(writer, value);

            var output = sw.ToString();
            writer.Close();
            sw.Close();

            return output;
        }

        public static object DeserializeJsonStringToObject(string jsonText, Type valueType)
        {
            var sr = new StringReader(jsonText);
            var reader = new JsonTextReader(sr);
            var result = JsonSerializer.Deserialize(reader, valueType);
            reader.Close();

            return result;
        }
        
        public static JToken DeserializeJsonString(string jsonText)
        {
            JToken result;
            using (var sr = new StringReader(jsonText))
            using (var reader = new JsonTextReader(sr))
            {
                var uncastResult = JsonSerializer.Deserialize(reader);
                result = uncastResult as JToken;
                reader.Close();
            }
            return result;
        }

        public static T DeserializeJsonString<T>(string jsonText, T exampleOfObject) where T : class
        {
            T result;
            using (var sr = new StringReader(jsonText))
            using (var reader = new JsonTextReader(sr))
            {
                if (typeof(T).Equals(typeof(object)))
                {
                    result = JsonSerializer.Deserialize(reader) as T;
                }
                else
                {
                    result = JsonSerializer.Deserialize(reader, typeof(T)) as T;
                }
                reader.Close();
            }
            return result;
        }

        public static T DeserializeJsonString<T>(string jsonText) where T : class
        {
            T result;
            using (var sr = new StringReader(jsonText))
            using (var reader = new JsonTextReader(sr))
            {
                if (typeof(T).Equals(typeof(object)))
                {
                    result = JsonSerializer.Deserialize(reader) as T;
                }
                else
                {
                    result = JsonSerializer.Deserialize(reader, typeof(T)) as T;
                }
                reader.Close();
            }
            return result;
        }
    }
}
