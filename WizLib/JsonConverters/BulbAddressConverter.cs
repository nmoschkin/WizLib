using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using System.Net.NetworkInformation;
using System.Diagnostics.CodeAnalysis;

namespace WizLib
{
    public class BulbAddressConverter : JsonConverter<BulbAddress>
    {
        public override BulbAddress ReadJson(JsonReader reader, Type objectType, [AllowNull] BulbAddress existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.Value is string s)
            {
                BulbAddress p;

                if (BulbAddress.TryParse(s, out p))
                {
                    return p;
                }
            }

            return null;
        }

        public override void WriteJson(JsonWriter writer, [AllowNull] BulbAddress value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }
            writer.WriteValue(value.ToString());
        }
    }
}
