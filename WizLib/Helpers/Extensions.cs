using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.NetworkInformation;

namespace System.Net
{
    /// <summary>
    /// <see cref="IPAddress"/> extensions.
    /// </summary>
    public static class IPAddressExtensions
    {
        /// <summary>
        /// Make a copy of an <see cref="IPAddress"/>
        /// </summary>
        /// <param name="original"><see cref="IPAddress"/> object to copy.</param>
        /// <returns>A copy of original <see cref="IPAddress"/>.</returns>
        public static IPAddress Clone(this IPAddress original)
        {
            if (original == null) return null;
            return new IPAddress(original.GetAddressBytes());
        }
    }

    /// <summary>
    /// <see cref="PhysicalAddress"/> extensions.
    /// </summary>
    public static class PhysicalAddressExtensions
    {
        /// <summary>
        /// Make a copy of an <see cref="PhysicalAddress"/>
        /// </summary>
        /// <param name="original"><see cref="PhysicalAddress"/> object to copy.</param>
        /// <returns>A copy of original <see cref="PhysicalAddress"/>.</returns>
        public static PhysicalAddress Clone(this PhysicalAddress original)
        {
            if (original == null) return null;
            return new PhysicalAddress(original?.GetAddressBytes());
        }
    }


}

namespace WizLib
{

    /// <summary>
    /// <see cref="MACADDRESS"/> extensions.
    /// </summary>
    public static class BulbAddressExtensions
    {
        /// <summary>
        /// Make a copy of an <see cref="MACADDRESS"/>
        /// </summary>
        /// <param name="original"><see cref="MACADDRESS"/> object to copy.</param>
        /// <returns>A copy of original <see cref="MACADDRESS"/>.</returns>
        public static MACADDRESS Clone(this MACADDRESS original)
        {
            if (original == null) return MACADDRESS.None;
            return new MACADDRESS(original.GetAddressBytes());
        }
    }

}
