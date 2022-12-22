using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;

namespace WiZ
{
    /// <summary>
    /// Represents a network adapter MAC address.
    /// </summary>
    /// <remarks></remarks>
    [StructLayout(LayoutKind.Sequential, Size = MAX_ADAPTER_ADDRESS_LENGTH)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public struct MACAddress : IComparable<MACAddress>, IComparable<PhysicalAddress>, IComparable<byte[]>
    {
        public const int MAX_ADAPTER_ADDRESS_LENGTH = 8;

        public static readonly MACAddress None = new MACAddress(new byte[0]);

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
            if (s == null || s.ToLower() == "none") return new MACAddress();

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
            unsafe
            {
                fixed (MACAddress* ptr = &this)
                {
                    fixed (byte* ptrx = address)
                    {
                        if (ptrx == null) return;
                        this = *(MACAddress*)ptrx;
                    }
                }
            }
        }

        private static MACAddress CreateFromBytes(byte[] bytes)
        {
            MACAddress nmx = new MACAddress();

            unsafe
            {
                fixed (byte* ptr = bytes)
                {
                    nmx = *(MACAddress*)ptr;
                }
            }

            return nmx;
        }

        public static explicit operator byte[](MACAddress obj) => obj.GetAddressBytes();

        public byte[] GetAddressBytes()
        {
            byte[] result = new byte[8];

            unsafe
            {
                fixed (MACAddress* ptr = &this)
                {
                    fixed (byte* ptr2 = result)
                    {
                        *(long*)ptr2 = *(long*)ptr;
                    }
                }
            }

            return result;
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
            if (obj is MACAddress ma) return Equals(ma);
            return false;
        }

        public bool Equals(MACAddress other)
        {
            unsafe
            {
                fixed (MACAddress* ptr1 = &this)
                {
                    MACAddress* ptr2 = &other;
                    return *(long*)ptr1 == *(long*)ptr2;
                }
            }
        }

        public override int GetHashCode()
        {
            unsafe
            {
                fixed (MACAddress* ptr = &this)
                {
                    return (*(long*)ptr).GetHashCode();
                }
            }
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

        public static explicit operator PhysicalAddress(MACAddress src)
        {
            return new PhysicalAddress(src.GetAddressBytes());
        }

        public static explicit operator MACAddress(PhysicalAddress src)
        {
            return new MACAddress(src?.GetAddressBytes() ?? new byte[0]);
        }

        public string ToString(bool delineate, bool upperCase = false, string sep = ":")
        {
            var address = GetAddressBytes();

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