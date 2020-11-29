using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace WizLib
{
    public sealed class BulbCommand : ViewModelBase
    {

        private BulbMethod method = BulbMethod.GetPilot;
        
        private BulbParams outparam;
        private BulbParams inparam;
        
        private string env;

        [JsonProperty("method")]
        public BulbMethod Method
        {
            get => method;
            set
            {
                SetProperty(ref method, value);
            }
        }

        [JsonProperty("params")]
        public BulbParams Params
        {
            get => outparam;
            set
            {
                SetProperty(ref outparam, value);
            }
        }

        [JsonProperty("result")]
        public BulbParams Result
        {
            get => inparam;
            set
            {
                SetProperty(ref inparam, value);
            }
        }

        [JsonProperty("env")]
        public string Environment
        {
            get => env;
            set
            {
                SetProperty(ref env, value);
            }
        }

        public string AssembleCommand()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                NullValueHandling = NullValueHandling.Ignore,
                Converters = { new TupleConverter() }
            };

            return JsonConvert.SerializeObject(this, settings);
        }

        public BulbCommand()
        {
            Params = new BulbParams();
        }

        public BulbCommand(BulbMethod mtd)
        {
            Method = mtd;
            Params = new BulbParams();
        }

        public BulbCommand(string json)
        {
            try
            {
                JsonSerializerSettings settings = new JsonSerializerSettings()
                {
                    ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                    NullValueHandling = NullValueHandling.Ignore,
                    Converters = { new TupleConverter() }
                };

                Params?.ClearPilot();
                Result?.ClearPilot();

                JsonConvert.PopulateObject(json, this, settings);
            }
            catch
            {

            }
        }

    }

}
