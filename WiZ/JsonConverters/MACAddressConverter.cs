using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using System.Net.NetworkInformation;
using System.Diagnostics.CodeAnalysis;

namespace WiZ
{
    public class MACAddressConverter : JsonConverter<MACAddress>
    {
        public override MACAddress ReadJson(JsonReader reader, Type objectType, MACAddress existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.Value is string s)
            {
                MACAddress p;

                if (MACAddress.TryParse(s, "", out p))
                {
                    return p;
                }
            }

            return MACAddress.None;
        }

        public override void WriteJson(JsonWriter writer, MACAddress value, JsonSerializer serializer)
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
