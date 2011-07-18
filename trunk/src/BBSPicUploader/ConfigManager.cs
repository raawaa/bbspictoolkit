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
        public static readonly string ConfigFilename = AppDomain.CurrentDomain.BaseDirectory + "config.xml";
        public static readonly string BoardsFilename = AppDomain.CurrentDomain.BaseDirectory + "boards.txt";

        public static UploaderConfig Config {get; private set;}

        public static List<string> Boards { get; private set; }

        public static UploaderConfig LoadConfig()
        {
            var xmlSerializer = new XmlSerializer(typeof(UploaderConfig));

            if (!File.Exists(ConfigFilename))
            {
                CreateDefaultConfig();
            }
            
            using (var fs = new FileStream(ConfigFilename, FileMode.Open))
            {
                Config = (UploaderConfig)xmlSerializer.Deserialize(fs);
            }
            
            return Config;
        }

        public static void LoadBoards()
        {
            Boards = new List<string>();

            using (var sr = new StreamReader(BoardsFilename))
            {
                while (!sr.EndOfStream)
                {
                    var board = sr.ReadLine();

                    Boards.Add(board);
                }
            }
        }

        static void CreateDefaultConfig()
        {
            Config = new UploaderConfig();

            Config.LastUpdateTime = new DateTime(2000, 1, 1);            

            SaveConfig();
        }

        public static void SaveConfig()
        {            
            using (var fs = new FileStream(ConfigFilename, FileMode.Create))
            {
                var xmlSerializer = new XmlSerializer(typeof(UploaderConfig));

                xmlSerializer.Serialize(fs, Config);
            }            
        }
    }
}
