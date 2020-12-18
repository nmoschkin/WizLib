using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;

namespace WiZ
{
    /// <summary>
    /// Represents a network adapter MAC address.
    /// </summary>
    /// <remarks></remarks>
    [StructLayout(LayoutKind.Sequential)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public struct MACAddress : IComparable<MACAddress>, IComparable<PhysicalAddress>, IComparable<byte[]>
    {
        public const int MAX_ADAPTER_ADDRESS_LENGTH = 8;

        public static readonly MACAddress None = new MACAddress(new byte[0]);

        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = MAX_ADAPTER_ADDRESS_LENGTH)]
        private byte[] address;

        public static bool operator ==(MACAddress val1, MACAddress val2)
        {
            return val1.Equals(val2);
        }

        public static bool operator !=(MACAddress val1, MACAddress val2)
        {
            return !val1.Equals(val2);
        }

        public static MACAddress Parse(string s) => Parse(s, "");

        public static MACAddress Parse(string s, string partition)
        {
            s = s.Replace(":", "").Replace("-", "").Replace(" ", "").Trim();

            List<string> sv = new List<string>();

            for (int z = 0; z < s.Length; z += 2)
            {
                sv.Add(s.Substring(z, 2));
            }

            byte[] b = new byte[MAX_ADAPTER_ADDRESS_LENGTH];

            int i, c = sv.Count - 1;
            int j = b.Length - 1;

            for (i = c; i >= 0; i--)
            {
                b[j] = byte.Parse(sv[i], System.Globalization.NumberStyles.HexNumber);
                j--;
            }

            return new MACAddress(b);
        }

        public static bool TryParse(string s, out MACAddress value) => TryParse(s, "", out value);

        public static bool TryParse(string s, string partition, out MACAddress value)
        {
            try
            {
                value = Parse(s, partition);
                return true;
            }
            catch
            {
                value = new MACAddress();
                return false;
            }
        }

        public MACAddress(byte[] address)
        {
            int i, c = address?.Length - 1 ?? throw new ArgumentNullException(nameof(address));

            this.address = new byte[MAX_ADAPTER_ADDRESS_LENGTH];
            int j = this.address.Length - 1;

            for (i = c; i >= 0; i--)
            {
                this.address[j] = address[i];
                j--;
            }
        }

        public static explicit operator byte[](MACAddress obj) => obj.GetAddressBytes();

        public byte[] GetAddressBytes()
        {
            var addr = address ?? new byte[0];
            int x = 0, c = addr.Length;

            for (int i = 0; i < c; i++)
            {
                if (addr[i] != 0) break;
                x++;
            }

            var outaddr = new byte[c - x];

            Array.Copy(addr, x, outaddr, 0, c - x);

            return outaddr;
        }

        public int CompareTo(PhysicalAddress other) => CompareTo(other?.GetAddressBytes());

        public int CompareTo(MACAddress other) => CompareTo(other.GetAddressBytes());

        public int CompareTo(byte[] other)
        {
            byte[] b1 = GetAddressBytes();
            byte[] b2 = other ?? new byte[0];

            if (b1.Length != b2.Length) return b1.Length - b2.Length;

            int c = b1.Length, i;

            for (i = 0; i < c; i++)
            {
                if (b1[i] < b2[i]) return -1;
                if (b1[i] > b2[i]) return 1;
            }

            return 0;
        }

        public override bool Equals(object obj)
        {
            byte[] b1 = address ?? new byte[0];
            byte[] b2;

            if (obj is MACAddress ma)
            {
                b2 = ma.address ?? new byte[0];
            }
            else if (obj is byte[])
            {
                b2 = (byte[])obj;
            }
            else if (obj is string s)
            {
                MACAddress other;

                bool b = TryParse(s, out other);
                if (!b) return false;

                b2 = other.address ?? new byte[0];
            }
            else
            {
                return false;
            }

            if (b1.Length != b2.Length) return false;
            int c = b1.Length;
          

            for (int i = 0; i < c; i++)
            {
                if (b1[i] != b2[i]) return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return (ToString().GetHashCode());
        }

        public override string ToString()
        {
            //return ToString(true, false);
            return ToString(true, true, ":");
        }


        public static explicit operator string(MACAddress b)
        {
            return b.ToString();
        }

        public static explicit operator MACAddress(string s)
        {
            return Parse(s);
        }

        public static explicit operator System.Net.NetworkInformation.PhysicalAddress(MACAddress src)
        {
            return new System.Net.NetworkInformation.PhysicalAddress(src.GetAddressBytes());
        }

        public static explicit operator MACAddress(System.Net.NetworkInformation.PhysicalAddress src)
        {
            return new MACAddress(src?.GetAddressBytes() ?? new byte[0]);
        }

        public string ToString(bool delineate, bool upperCase = false, string sep = ":")
        {
            int i, c = address?.Length ?? 0;
            if (c == 0) return "None";

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
                    if (sb.Length != 0) sb.Append(sep);
                }

                sb.Append(address[i].ToString(fmt));

            }

            return sb.ToString();

        }
    }
}
