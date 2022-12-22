using System;
using System.Collections.Generic;
using System.Text;

using WiZ.Command;

namespace WiZ.Contracts
{
    public interface ILightMode : IComparable<ILightMode>, IComparable<int>
    {
        string Name { get; }

        LightModeType Type { get; }

        int Code { get; }

        BulbParams Settings { get; }
    }
}