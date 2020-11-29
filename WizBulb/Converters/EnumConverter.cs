using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

using WizBulb.Localization.Resources;


namespace WizBulb.Converters
{
    public class ResourceReferenceAttribute : Attribute
    {

        static readonly string DefaultResId = "WizBulb.Localization.Resources.AppResources";

        public static readonly Lazy<ResourceManager> ResMgr = new Lazy<ResourceManager>(
            () => new ResourceManager(DefaultResId, IntrospectionExtensions.GetTypeInfo(typeof(AppResources)).Assembly));

        protected ResourceManager LocalResMgr;

        public string ResourceId { get; protected set; }

        public string ResourceKey { get; protected set; }

        public ResourceReferenceAttribute(string resKey, string resId = null)
        {
            ResourceId = resId;
            ResourceKey = resKey;

            if (resId != null)
            {
                LocalResMgr = new ResourceManager(resId, Assembly.GetExecutingAssembly());
            }
        }

        public string Translate()
        {
            if (LocalResMgr != null)
            {
                return LocalResMgr.GetString(ResourceKey);
            }
            else
            {
                return ResMgr.Value.GetString(ResourceKey);
            }
        }


        public static string Translate(string key)
        {
            return ResMgr.Value.GetString(key);
        }

    }

    public class EnumConverter : IValueConverter
    {

        public Type EnumType { get; set; }


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Enum e)
            {
                var fields = e.GetType().GetFields(BindingFlags.Public | BindingFlags.Static);

                foreach (var field in fields)
                {
                    if ((int)field.GetValue(null) == (int)value)
                    {
                        var att = field.GetCustomAttribute<ResourceReferenceAttribute>();

                        if (att != null)
                        {
                            return att.Translate();
                        }
                        else
                        {
                            string tc = null;
                           
                            try
                            {
                                tc = ResourceReferenceAttribute.Translate(field.Name);
                            }
                            catch
                            {

                            }
                            
                            return tc == null ? e.ToString() : tc;
                        }
                    }
                }

            }

            return value?.ToString();

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType.Name == "Nullable`1")
            {
                targetType = Nullable.GetUnderlyingType(targetType);
            }

            if (targetType.BaseType == typeof(Enum))
            {
                var fields = targetType.GetFields(BindingFlags.Public | BindingFlags.Static);

                foreach (var field in fields)
                {
                    var att = field.GetCustomAttribute<ResourceReferenceAttribute>();
                    
                    if (att != null)
                    {
                        if (att.Translate() == (string)value)
                        {
                            return field.GetValue(null);
                        }
                    }
                    else if (field.Name == (string)value)
                    {
                        return field.GetValue(null);
                    }
                    else
                    {
                        string tc = null;
                        
                        try
                        {
                            tc = ResourceReferenceAttribute.Translate(field.Name);
                        }
                        catch
                        {

                        }

                        if (tc == (string)value)
                        {
                            return field.GetValue(null);
                        }

                    }
                }

                return value;

            }
            else
            {
                return value;
            }
        }
    }
}
