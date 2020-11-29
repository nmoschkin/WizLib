using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace WizLib
{

    public interface IProfile
    {
        Guid ProjectId { get; set; }

        string Name { get; set; }

        IList<Home> Homes { get; set;  }

        IList<LightMode> LightModes { get; set; }

        IList<Bulb> Bulbs { get; set; }

        IList<Room> Rooms { get; set; }

        IList<Scene> Scenes { get; set; }

    }


    public interface IProfileSerializer
    {
        IProfile Deserialize();

        void Serialize(IProfile profile);
    }

    public class JsonProfileSerializer : IProfileSerializer
    {
        public string Filename { get; set; }

        public static string DefaultExtension { get; set; } = ".wizj";

        public static string LastDirectory { get; set; }

        public static string[] EnumProfiles(string path, string ext = null, bool setLastDir = true)
        {
            if (ext == null)
                ext = DefaultExtension;

            if (!Directory.Exists(path)) throw new DirectoryNotFoundException(path);
            if (setLastDir) LastDirectory = path;

            if (string.IsNullOrEmpty(ext))
            {
                return Directory.GetFiles(path);
            }
            else
            {
                if (ext[0] != '.') ext = '.' + ext;
                ext = "*" + ext;

                return Directory.GetFiles(path, ext);
            }
        }


        public JsonProfileSerializer(string fileName, bool deferLoad = false)
        {
            Filename = fileName;
            if (!deferLoad)
            {
                Deserialize();
            }
        }

        public IProfile Deserialize()
        {
            if (!File.Exists(Filename)) throw new FileNotFoundException(nameof(Filename));

            string json = File.ReadAllText(Filename);

            Profile p = new Profile();

            JsonConvert.PopulateObject(json, p, BulbCommand.DefaultJsonSettings);
            return p;
        }

        public void Serialize(IProfile profile)
        {
            string json = JsonConvert.SerializeObject(profile, BulbCommand.DefaultJsonSettings);

            if (string.IsNullOrEmpty(Filename)) throw new NullReferenceException(nameof(Filename));
            File.WriteAllText(Filename, json);
        }

    }

}
