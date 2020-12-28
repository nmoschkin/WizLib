using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WiZ.Observable;    

namespace WiZ.Profiles
{

    public interface IProfile
    {
        Guid ProjectId { get; set; }

        string Name { get; set; }

        ObservableDictionary<int, Home> Homes { get; set; }

        ObservableDictionary<int, LightMode> CustomLightModes { get; set; }

        ObservableDictionary<MACAddress, BulbItem> Bulbs { get; set; }

        void BuildUpdateProfile(IEnumerable<IBulb> bulbs, bool removeMissing);

    }

}
