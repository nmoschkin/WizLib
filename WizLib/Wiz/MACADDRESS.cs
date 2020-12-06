// ************************************************* ''
// DataTools C# Native Utility Library For Windows - Interop
//
// Module: IfDefApi
//         The almighty network interface native API.
//         Some enum documentation comes from the MSDN.
//
// (and an exercise in creative problem solving and data-structure marshaling.)
//
// Copyright (C) 2011-2020 Nathan Moschkin
// All Rights Reserved
//
// Licensed Under the Microsoft Public License   
// ************************************************* ''


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace WizLib
{
    /// <summary>
    /// Represents a network adapter MAC address.
    /// </summary>
    /// <remarks></remarks>
    [StructLayout(LayoutKind.Sequential)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public struct MACADDRESS : IComparable<MACADDRESS>
    {
        public const int MAX_ADAPTER_ADDRESS_LENGTH = 8;

        public static readonly MACADDRESS None = new MACADDRESS(new byte[0]);

        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = MAX_ADAPTER_ADDRESS_LENGTH)]
        private byte[] address;

        public static bool operator ==(MACADDRESS val1, MACADDRESS val2)
        {
            return val1.Equals(val2);
        }

        public static bool operator !=(MACADDRESS val1, MACADDRESS val2)
        {
            return !val1.Equals(val2);
        }

        public static MACADDRESS Parse(string s) => Parse(s, "");

        public static MACADDRESS Parse(string s, string partition)
        {
            if (!string.IsNullOrEmpty(partition))
                s = s.Replace(partition, "").Trim();
            else s = s.Trim();


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

            return new MACADDRESS(b);
        }

        public static bool TryParse(string s, out MACADDRESS value) => TryParse(s, "", out value);

        public static bool TryParse(string s, string partition, out MACADDRESS value)
        {
            try
            {
                value = Parse(s, partition);
                return true;
            }
            catch
            {
                value = new MACADDRESS();
                return false;
            }
        }

        public MACADDRESS(byte[] hwaddr)
        {
            int i, c = hwaddr?.Length - 1 ?? throw new ArgumentNullException(nameof(hwaddr));

            address = new byte[MAX_ADAPTER_ADDRESS_LENGTH];
            int j = address.Length - 1;

            for (i = c; i >= 0; i--)
            {
                address[j] = hwaddr[i];
                j--;
            }
        }

        public static explicit operator byte[](MACADDRESS obj) => obj.address ?? new byte[0];

        public byte[] GetAddressBytes()
        {
            return address ?? new byte[0];
        }

        public int CompareTo(MACADDRESS other)
        {
            byte[] b1 = address ?? new byte[0];
            byte[] b2 = other.address ?? new byte[0];

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

            if (obj is MACADDRESS ma)
            {
                b2 = ma.address ?? new byte[0];
            }
            else if (obj is byte[])
            {
                b2 = (byte[])obj;
            }
            else if (obj is string s)
            {
                MACADDRESS other;

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
            return ToString(false, true);
        }


        public static explicit operator string(MACADDRESS b)
        {
            return b.ToString();
        }

        public static explicit operator MACADDRESS(string s)
        {
            return Parse(s);
        }

        public static explicit operator System.Net.NetworkInformation.PhysicalAddress(MACADDRESS src)
        {
            return new System.Net.NetworkInformation.PhysicalAddress(src.address);
        }

        public static explicit operator MACADDRESS(System.Net.NetworkInformation.PhysicalAddress src)
        {
            return new MACADDRESS(src?.GetAddressBytes() ?? new byte[0]);
        }

        public string ToString(bool delineate, bool upperCase = false)
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
                    if (sb.Length != 0) sb.Append(':');
                }

                sb.Append(address[i].ToString(fmt));

            }

            return sb.ToString();

        }
    }
}
