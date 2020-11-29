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
    public class PhysicalAddressConverter : JsonConverter<PhysicalAddress>
    {
        public override PhysicalAddress ReadJson(JsonReader reader, Type objectType, [AllowNull] PhysicalAddress existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.Value is string s)
            {
                PhysicalAddress p;

                if (PhysicalAddress.TryParse(s, out p))
                {
                    return p;
                }
            }

            return null;
        }

        public override void WriteJson(JsonWriter writer, [AllowNull] PhysicalAddress value, JsonSerializer serializer)
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
