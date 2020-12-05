using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace WizLib
{
    /// <summary>
    /// Represents the hardware address of a WiZ bulb.
    /// </summary>
    /// <remarks>
    /// Derived from <see cref="PhysicalAddress"/>.
    /// </remarks>
    public class BulbAddress : IComparable<PhysicalAddress>, IComparable<BulbAddress>
    {

        private byte[] address;

        public static readonly BulbAddress None = new BulbAddress(new byte[0]);

        /// <summary>
        /// Convert the string representation of a hardware address to a <see cref="BulbAddress"/> instance. A return value indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="address">A string containing the address that will be used to initialize the <see cref="BulbAddress"/> instance returned by this method.</param>
        /// <returns>the <see cref="BulbAddress"/> instance equivalent of the address contained in address.</returns>
        public static BulbAddress Parse(string address)
        {
            if (string.IsNullOrEmpty(address)) return None;
            address = address.Replace(":", "-").Replace(" ", "-");
            var p = PhysicalAddress.Parse(address);
            return new BulbAddress(p.GetAddressBytes());
        }

        /// <summary>
        /// Tries to convert the string representation of a hardware address to a <see cref="BulbAddress"/> instance. A return value indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="address">A string containing the address that will be used to initialize the <see cref="BulbAddress"/> instance returned by this method.</param>
        /// <param name="value">When this method returns, contains the <see cref="BulbAddress"/> instance equivalent of the address contained in address, if the conversion succeeded, or null if the conversion failed. If the address is null it contains <see cref="None"/>. This parameter is passed uninitialized; any value originally supplied in result will be overwritten.</param>
        /// <returns></returns>
        public static bool TryParse(string address, out BulbAddress value)
        {
            if (string.IsNullOrEmpty(address)) {
                value = (BulbAddress)BulbAddress.None;
                return false;
            }

            PhysicalAddress p;

            var b = PhysicalAddress.TryParse(address, out p);
            value = new BulbAddress(p.GetAddressBytes());
            return b;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BulbAddress"/> class.
        /// </summary>
        /// <param name="address">A byte array containing the address.</param>
        public BulbAddress(byte[] address)
        {
            this.address = address;
        }

        int Compare(byte[] address)
        {
            if (address == null) return 1;

            byte[] b1 = GetAddressBytes();
            byte[] b2 = address;

            if (b1.Length < b2.Length) return -1;
            if (b1.Length > b2.Length) return 1;

            int c = b1.Length;

            for (int i = 0; i < c; i++)
            {
                if (b1[i] < b2[i]) return -1;
                if (b1[i] > b2[i]) return 1;
            }

            return 0;
        }

        public byte[] GetAddressBytes()
        {
            return address;
        }

        public override string ToString()
        {
            return ToString(false, true);
        }
        public string ToString(bool delineate, bool upperCase = false)
        {
            int i, c = address?.Length ?? 0;
            if (c == 0) return null;

            StringBuilder sb = new StringBuilder();
            string fmt = upperCase ? "X2" : "x2";

            bool sc = true;

            for (i = c - 1; i >= 0; i--)
            {
                if (address[i] != 0)
                {
                    c = i + 1;
                    break;
                }
            }

            for (i = 0; i < c; i++)
            {
                if (sc)
                {
                    if (address[i] == 0) continue;
                    sc = false;
                }

                if (delineate)
                {
                    if (sb.Length != 0) sb.Append(':');
                }

                sb.Append(address[i].ToString(fmt));

            }

            return sb.ToString();

        }

        int IComparable<PhysicalAddress>.CompareTo(PhysicalAddress other)
        {
            return Compare(other.GetAddressBytes());
        }

        int IComparable<BulbAddress>.CompareTo(BulbAddress other)
        {
            return Compare(other?.GetAddressBytes());
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            return (this.ToString() == obj.ToString());

            //if (obj is BulbAddress ba && ba != null)
            //{
            //    if ((address?.Length ?? 0) != (ba.address?.Length ?? 0)) return false;
            //    int c = address.Length;
            //    for (int i = 0; i < c; i++)
            //    {
            //        if (address[i] != ba.address[i]) return false;
            //    }

            //    return true;                
            //}
            //else
            //{
            //    return false;
            //}
        }

        //public void GetObjectData(SerializationInfo info, StreamingContext context)
        //{
        //    info.AddValue(nameof(address), ToString());
        //}

        //public BulbAddress(SerializationInfo info, StreamingContext context) 
        //{

        //    var s = (string)info.GetValue(nameof(address), typeof(string));
        //    var p = PhysicalAddress.Parse(s);

        //    this.address = p.GetAddressBytes();
        //}

        public static explicit operator PhysicalAddress(BulbAddress value)
        {
            return new PhysicalAddress(value.address);
        }

        public static explicit operator BulbAddress(PhysicalAddress value)
        {
            return new BulbAddress(value.GetAddressBytes());
        }

        public static explicit operator BulbAddress(string value)
        {
            return Parse(value);
        }

        public static explicit operator byte[](BulbAddress value)
        {
            return value?.GetAddressBytes();
        }

        public static explicit operator string(BulbAddress value)
        {
            return value?.ToString();
        }

    }
}
