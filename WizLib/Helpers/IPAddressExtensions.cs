using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

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
            return IPAddress.Parse(original.ToString());
        }
    }
}
