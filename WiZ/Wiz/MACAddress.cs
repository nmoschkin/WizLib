using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
    public struct MACAddress : IComparable<MACAddress>, IComparable<PhysicalAddress>, IComparable<byte[]>, IEquatable<MACAddress>
    {
        public const int MAX_ADAPTER_ADDRESS_LENGTH = 8;

        public static readonly MACAddress None = new MACAddress();

        public static bool operator ==(MACAddress val1, MACAddress val2)
        {
            return val1.Equals(val2);
        }

        public static bool operator !=(MACAddress val1, MACAddress val2)
        {
            return !val1.Equals(val2);
        }

        public static MACAddress Parse(string s)
        {
            var strs = new string(s.Where(u => "abcdefABCDEF0123456789".Contains(u)).ToArray());

            var lng = BitConverter.GetBytes(long.Parse(strs, System.Globalization.NumberStyles.HexNumber)).Reverse().ToArray();

            unsafe
            {
                MACAddress mac = new MACAddress();
                MACAddress* ptr = &mac;
                fixed (byte* ptr2 = lng)
                {
                    *ptr = *(MACAddress*)ptr2;
                }
                var z = mac.ToString();
                return mac;
            }
        }

        public static bool TryParse(string s, out MACAddress value)
        {
            try
            {
                value = Parse(s);
                return true;
            }
            catch
            {
                value = 0;
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

        public static implicit operator MACAddress(long val)
        {
            unsafe
            {
                MACAddress m = new MACAddress();
                MACAddress* ptr = &m;

                *(long*)ptr = val;
                return m;
            }
        }

        public static implicit operator long(MACAddress val)
        {
            unsafe
            {
                MACAddress* ptr = &val;
                return *(long*)ptr;
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

        public int CompareTo(MACAddress other)
        {
            unsafe
            {
                fixed (MACAddress* ptr = &this)
                {
                    MACAddress* ptr2 = &other;
                    return (*(long*)ptr).CompareTo(*(long*)ptr2);
                }
            }
        }

        public int CompareTo(byte[] other)
        {
            unsafe
            {
                fixed (MACAddress* ptr = &this)
                {
                    fixed (byte* ptr2 = other)
                    {
                        return (*(long*)ptr).CompareTo(*(long*)ptr2);
                    }
                }
            }
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