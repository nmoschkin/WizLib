using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Windows.Markup;

using WizBulb.Localization.Resources;

namespace WizBulb.Localization
{
    // You exclude the 'Extension' suffix when using in XAML
    [ContentProperty("Text")]
    public class TranslateExtension : MarkupExtension
    {
        CultureInfo ci = null;
        const string ResourceId = "WizBulb.Localization.Resources.AppResources";

        static readonly Lazy<ResourceManager> ResMgr = new Lazy<ResourceManager>(
            () => new ResourceManager(ResourceId, IntrospectionExtensions.GetTypeInfo(typeof(AppResources)).Assembly));

        public string ResourceKey { get; set; }

        public bool AddColon { get; set; } = false;

        public TranslateExtension()
        {
            ci = CultureInfo.CurrentCulture;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (ResourceKey == null)
                return string.Empty;

            string translation = null;

            try
            {
                translation = ResMgr.Value.GetString(ResourceKey, ci);
                if (AddColon && !string.IsNullOrEmpty(translation)) translation += ":";
            }
            catch
            {
                try
                {
                    translation = ResMgr.Value.GetString(ResourceKey, new CultureInfo("en")); // default to english
                    if (AddColon && !string.IsNullOrEmpty(translation)) translation += ":";
                }
                catch
                {
                    translation = "bad translation for " + ResourceKey;
                }
            }


            if (translation == null)
            {
                ArgumentException ex = new ArgumentException(
                    string.Format("Key '{0}' was not found in resources '{1}' for culture '{2}'.", ResourceKey, ResourceId, ci.Name),
                    "Text");
                //App.TrackError(ex);
#if DEBUG
                throw ex;
#else
                try
                {
                    translation = ResMgr.Value.GetString(Text, new CultureInfo("en")); // default to english
                }
                catch
                {
                    translation = Text; // HACK: returns the key, which GETS DISPLAYED TO THE USER
                }
#endif
            }
            return translation;
        }
    }
}
