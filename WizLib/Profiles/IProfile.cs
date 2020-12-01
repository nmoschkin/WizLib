using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WizLib.Profiles
{

    public interface IProfile
    {
        Guid ProjectId { get; set; }

        string Name { get; set; }

        List<Home> Homes { get; set; }

        List<LightMode> CustomLightModes { get; set; }

        List<BulbItem> Bulbs { get; set; }

        void AddUpdateBulbs(IEnumerable<IBulb> bulbs, bool removeMissing);

    }

}
