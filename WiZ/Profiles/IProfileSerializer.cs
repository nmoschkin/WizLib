using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace WiZ.Profiles
{

    public interface IProfileSerializer
    {
        IProfile Deserialize();

        void Deserialize(IProfile obj);

        void Serialize(IProfile profile);

        string[] EnumProfiles();
    }

  

}
