using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace BBSPicUploader
{
    public class ConfigManager
    {
        public static readonly string Filename = AppDomain.CurrentDomain.BaseDirectory + "config.xml";

        public static UploaderConfig Config {get; private set;}

        public static UploaderConfig LoadConfig()
        {
            var xmlSerializer = new XmlSerializer(typeof(UploaderConfig));

            if (!File.Exists(Filename))
            {
                CreateDefaultConfig();
            }
            
            using (var fs = new FileStream(Filename, FileMode.Open))
            {
                Config = (UploaderConfig)xmlSerializer.Deserialize(fs);
            }
            
            return Config;
        }

        static void CreateDefaultConfig()
        {
            Config = new UploaderConfig();

            Config.LastUpdateTime = new DateTime(2000, 1, 1);            

            SaveConfig();
        }

        public static void SaveConfig()
        {            
            using (var fs = new FileStream(Filename, FileMode.Create))
            {
                var xmlSerializer = new XmlSerializer(typeof(UploaderConfig));

                xmlSerializer.Serialize(fs, Config);
            }            
        }
    }
}
