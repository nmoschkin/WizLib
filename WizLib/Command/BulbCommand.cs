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
    /// <summary>
    /// Bulb command structure root object.
    /// </summary>
    public sealed class BulbCommand : ViewModelBase
    {

        private BulbMethod method = BulbMethod.GetPilot;
        
        private BulbParams outparam;
        private BulbParams inparam;
        
        private string env;

        /// <summary>
        /// The <see cref="BulbMethod"/> of this instance.
        /// </summary>
        [JsonProperty("method")]
        public BulbMethod Method
        {
            get => method;
            set
            {
                SetProperty(ref method, value);
            }
        }

        /// <summary>
        /// Outbound paramters.
        /// </summary>
        [JsonProperty("params")]
        public BulbParams Params
        {
            get => outparam;
            set
            {
                SetProperty(ref outparam, value);
            }
        }

        /// <summary>
        /// Inbound results.
        /// </summary>
        [JsonProperty("result")]
        public BulbParams Result
        {
            get => inparam;
            set
            {
                SetProperty(ref inparam, value);
            }
        }

        /// <summary>
        /// Environment.
        /// </summary>
        [JsonProperty("env")]
        public string Environment
        {
            get => env;
            set
            {
                SetProperty(ref env, value);
            }
        }

        /// <summary>
        /// Assemble and return the JSON string for this command.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Create a new, blank instance.
        /// </summary>
        public BulbCommand()
        {
            Params = new BulbParams();
        }

        /// <summary>
        /// Create a new instance initialized to the specified <see cref="BulbMethod"/>.
        /// </summary>
        /// <param name="mtd"></param>
        public BulbCommand(BulbMethod mtd)
        {
            Method = mtd;
            Params = new BulbParams();
        }

        /// <summary>
        /// Create a new instance initialzed with a JSON string object.
        /// </summary>
        /// <param name="json"></param>
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
