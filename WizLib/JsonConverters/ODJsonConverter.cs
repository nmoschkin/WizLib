using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace WizLib
{
    public class ODJsonConverter<TKey, TValue> : JsonConverter<ObservableDictionary<TKey, TValue>> where TValue : class
    {
        public string PropertyName { get; private set; }
        public ODJsonConverter(string propertyName)
        {
            PropertyName = propertyName;
        }
        
        public override ObservableDictionary<TKey, TValue> ReadJson(JsonReader reader, Type objectType, [AllowNull] ObservableDictionary<TKey, TValue> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            ObservableDictionary<TKey, TValue> des = new ObservableDictionary<TKey, TValue>(PropertyName);

            //var d = reader.Depth;

            //reader.Read();
            //int c = 0;

            //do
            //{
            //    var s = reader.Path;
            //    reader.Read();
            //} while (reader.Depth > d);




            serializer.Populate(reader, des);
            return des;
        }

        public override void WriteJson(JsonWriter writer, [AllowNull] ObservableDictionary<TKey, TValue> value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            foreach (var kv in value as IDictionary<TKey, TValue>)
            {
                writer.WritePropertyName(kv.Key.ToString());
                serializer.Serialize(writer, kv.Value, typeof(TValue));
            }

            writer.WriteEndObject();
        }
    }
        
}
