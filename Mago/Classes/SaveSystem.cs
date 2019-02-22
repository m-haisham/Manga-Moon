using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Mago
{
    public class SaveSystem
    {
        private static readonly string settingsPath = "settings.xml";

        public static void SaveBinary<T>(T obj, string path)
        {
            //Create directory if it doesnt exist
            Directory.CreateDirectory(Directory.GetParent(path).FullName);

            path = path.Replace(":", string.Empty);
            path = path.Replace("|", string.Empty);
            path = path.Replace("?", string.Empty);
            path = path.Replace("__", "_");

            //create file, if file exists overwrite
            using(FileStream stream = new FileStream(path, FileMode.Create))
            {
                //initialize formattter
                BinaryFormatter formatter = new BinaryFormatter();

                //serialize the data and write to stream
                formatter.Serialize(stream, obj);
            }
        }

        public static T LoadBinary<T>(string path)
        {
            //if file doesnt exist, return default value
            if (!File.Exists(path))
                return default(T);

            //open file as read only
            using (FileStream stream = new FileStream(path, FileMode.Open))
            {
                //initialize formatter
                BinaryFormatter formatter = new BinaryFormatter();

                //Read data from file onto type T
                T newT = (T)formatter.Deserialize(stream);

                //return read data
                return newT;
            }
        }
        
        public static Settings LoadSettings()
        {
            if (!File.Exists(settingsPath))
            {
                Settings newSettings = new Settings();
                SaveSettings(newSettings);
                return newSettings;
            }

            //open file as read only
            using (FileStream stream = new FileStream(settingsPath, FileMode.Open))
            {
                //initialize xml serializer
                XmlSerializer serializer = new XmlSerializer(typeof(Settings));

                //read data from file
                Settings newSettings = (Settings)serializer.Deserialize(stream);

                //return read data
                return newSettings;
            }
        }

        public static void SaveSettings(Settings settings)
        {
            //Create directory if it doesnt exist
            Directory.CreateDirectory(Directory.GetParent(settingsPath).FullName);

            //create file, if file exists overwrite
            using (FileStream stream = new FileStream(settingsPath, FileMode.Create))
            {
                //initialize xml serializer
                XmlSerializer serializer = new XmlSerializer(typeof(Settings));

                //serialize and write data to stream
                serializer.Serialize(stream, settings);
            }
        }

    }
}
