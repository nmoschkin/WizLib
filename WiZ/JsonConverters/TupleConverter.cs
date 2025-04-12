using Newtonsoft.Json;

using System;
using System.Linq;
using System.Text;

using WiZ.Command;

namespace WiZ
{
    /// <summary>
    /// 2-element <see cref="int[]"/> to nullable tuple <see cref="(int, int)?"/> (bidirectional)
    /// </summary>
    /// <remarks>
    /// Used exclusively for the <see cref="BulbParams.DriverConfig"/> property.
    /// </remarks>
    internal sealed class TupleConverter : JsonConverter<(int, int)?>
    {
        public override (int, int)? ReadJson(JsonReader reader, Type objectType, (int, int)? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader is JsonTextReader jr)
            {
                int x1 = 0;
                int x2 = 0;

                if (jr.Read() && jr.Value is long i)
                {
                    x1 = (int)i;

                    if (jr.Read() && jr.Value is long j)
                    {
                        x2 = (int)j;
                    }
                }

                jr.Read();

                return (x1, x2);
            }
            else
            {
                return null;
            }
        }

        public override void WriteJson(JsonWriter writer, (int, int)? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
            }
            else
            {
                writer.WriteStartArray();
                writer.WriteValue(value.Value.Item1);
                writer.WriteValue(value.Value.Item2);
                writer.WriteEndArray();
            }
        }
    }
}