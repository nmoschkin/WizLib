using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using WiZ.Observable;

namespace WiZ
{
    public class ODJsonConverter<TKey, TValue> : JsonConverter<ObservableDictionary<TKey, TValue>> where TValue : class
    {
        public string PropertyName { get; private set; }
        public ODJsonConverter(string propertyName)
        {
            PropertyName = propertyName;
        }
        
        public override ObservableDictionary<TKey, TValue> ReadJson(JsonReader reader, Type objectType, ObservableDictionary<TKey, TValue> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            ObservableDictionary<TKey, TValue> des = new ObservableDictionary<TKey, TValue>(PropertyName);

            try
            {
                serializer.Populate(reader, des);
            }
            catch
            {

            }

            return des;
        }

        public override void WriteJson(JsonWriter writer, ObservableDictionary<TKey, TValue> value, JsonSerializer serializer)
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
