using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace WiZ
{
    /// <summary>
    /// <see cref="BulbMethod"/> <see cref="JsonConverter"/> class.
    /// </summary>
    public sealed class BulbMethodJsonConverter : JsonConverter<BulbMethod>
    {
        public override BulbMethod ReadJson(JsonReader reader, Type objectType, BulbMethod existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.Value is string s)
            {
                return (BulbMethod)s;
            }
            else
            {
                return (BulbMethod)"";
            }
        }

        public override void WriteJson(JsonWriter writer, BulbMethod value, JsonSerializer serializer)
        {
            writer.WriteValue((string)value);
        }
    }
}
