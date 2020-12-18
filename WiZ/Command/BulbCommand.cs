using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using WiZ.Observable;
using WiZ.Profiles;

namespace WiZ
{
    /// <summary>
    /// Bulb command structure root object.
    /// </summary>
    public sealed class BulbCommand : ObservableBase
    {

        #region Internal Fields

        internal static List<JsonConverter> JsonConverters { get; } = new List<JsonConverter>(
            new JsonConverter[] 
            { 
                new TupleConverter(), 
                new MACAddressConverter(),
                new ODJsonConverter<MACAddress, Bulb>(nameof(Bulb.MACAddress)),
                new ODJsonConverter<int, Room>(nameof(Room.RoomId)),
                new ODJsonConverter<int, Home>(nameof(Home.HomeId)),
                new ODJsonConverter<Guid, Scene>(nameof(Scene.SceneId)),
                new BulbMethodJsonConverter(), 
                new IPAddressConverter() 
            });

        internal static readonly JsonSerializerSettings DefaultJsonSettings = new JsonSerializerSettings()
        {
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            NullValueHandling = NullValueHandling.Ignore,
            Converters = JsonConverters
        };

        internal static readonly JsonSerializerSettings DefaultProjectJsonSettings = new JsonSerializerSettings()
        {
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            Converters = JsonConverters
        };

        #endregion Internal Fields

        #region Private Fields

        private string env;
        private BulbParams inparam;
        private BulbMethod method = BulbMethod.GetPilot;

        private BulbParams outparam;

        #endregion Private Fields

        #region Public Constructors

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
                JsonConvert.PopulateObject(json, this, DefaultJsonSettings);
            }
            catch
            {

            }
        }

        #endregion Public Constructors

        #region Public Properties

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

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Assemble and return the JSON string for this command.
        /// </summary>
        /// <returns></returns>
        public string AssembleCommand()
        {
            return JsonConvert.SerializeObject(this, DefaultJsonSettings);
        }

        #endregion Public Methods
    }

}
