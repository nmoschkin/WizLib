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
    public class MACADDRESSConverter : JsonConverter<MACADDRESS>
    {
        public override MACADDRESS ReadJson(JsonReader reader, Type objectType, [AllowNull] MACADDRESS existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.Value is string s)
            {
                MACADDRESS p;

                if (MACADDRESS.TryParse(s, "", out p))
                {
                    return p;
                }
            }

            return MACADDRESS.None;
        }

        public override void WriteJson(JsonWriter writer, [AllowNull] MACADDRESS value, JsonSerializer serializer)
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
