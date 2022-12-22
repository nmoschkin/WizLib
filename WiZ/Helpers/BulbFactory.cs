using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using WiZ.Contracts;

namespace WiZ.Helpers
{
    public class BulbFactory
    {
        public static readonly BulbFactory Instance = new BulbFactory();

        private BulbFactory()
        { }

        public T CreateBulb<T>(MACAddress macAddress) where T : IBulbController
        {
            if (typeof(T) == typeof(Bulb))
            {
                return (T)(IBulbController)new Bulb(macAddress);
            }

            var ctor = typeof(T).GetConstructor(new Type[] { typeof(MACAddress) });

            object[] p = new object[1];

            IBulbController obj;

            if (ctor != null)
            {
                p[0] = macAddress;
                obj = (IBulbController)ctor.Invoke(p);
            }
            else
            {
                ctor = typeof(T).GetConstructor(new Type[0]);

                if (ctor != null)
                {
                    obj = (IBulbController)ctor.Invoke(new object[0]);

                    var ptest1 = typeof(T).GetProperties().Where(pp => pp.PropertyType == typeof(MACAddress) && pp.CanWrite)?.FirstOrDefault();
                    var ptest2 = typeof(T).GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Where(pp => pp.FieldType == typeof(MACAddress))?.FirstOrDefault();

                    if (ptest1 != null)
                    {
                        ptest1.SetValue(obj, macAddress);
                    }
                    else if (ptest2 != null)
                    {
                        ptest2.SetValue(obj, macAddress);
                    }
                }
                else
                {
                    return default;
                }
            }

            return (T)obj;
        }
    }
}