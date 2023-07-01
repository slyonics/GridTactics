using GridTactics.Main;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace GridTactics.Models
{
    public static class GameProfile
    {
        [Serializable]
        public class MapState
        {
            public string name;
            public int seed;
        }

        public const string SAVE_FOLDER = "\\Save";
        private static Dictionary<string, object> DEFAULT_SAVE_VALUES = new Dictionary<string, object>()
        {
            { "NewPlayer", true }
        };

        private static int worldTime;
        public static int WorldTime { get => worldTime; set => worldTime = value; }

        private static int saveSlot;
        public static int SaveSlot { get => saveSlot; set => saveSlot = value; }

        private static Dictionary<string, object> saveData;
        public static Dictionary<string, object> SaveData { get => saveData; }

        private static PlayerProfile playerProfile;

        public static void NewState()
        {
            saveSlot = -1;
            //saveSlot = 0;
            saveData = new Dictionary<string, object>(DEFAULT_SAVE_VALUES);
            playerProfile = new PlayerProfile();
        }

        public static void LoadState(string saveFileName)
        {
            int slotStart = saveFileName.LastIndexOf(SAVE_FOLDER) + 5;
            int slotEnd = saveFileName.LastIndexOf('.');
            saveSlot = int.Parse(saveFileName.Substring(slotStart, slotEnd - slotStart));

            string savePath = CrossPlatformGame.SETTINGS_DIRECTORY + SAVE_FOLDER + "//" + saveFileName;

            BinaryFormatter binaryFormatter = new BinaryFormatter();
            FileInfo fileInfo = new FileInfo(savePath);

            using (FileStream fileStream = fileInfo.OpenRead())
            {
                saveData = (Dictionary<string, object>)binaryFormatter.Deserialize(fileStream);
                playerProfile = (PlayerProfile)binaryFormatter.Deserialize(fileStream);
            }
        }

        public static void SaveState()
        {
            if (saveSlot == -1)
            {
                List<string> saveSlots = SaveList;
                saveSlot = 0;

                while (saveSlots.Exists(x => ParseSlotNumber(x) == saveSlot))
                    saveSlot++;
            }

            BinaryFormatter binaryFormatter = new BinaryFormatter();

            string directory = CrossPlatformGame.SETTINGS_DIRECTORY + SAVE_FOLDER;
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using (FileStream fileStream = File.Open(directory + "\\Save" + saveSlot + ".sav", FileMode.OpenOrCreate))
            {
                binaryFormatter.Serialize(fileStream, saveData);
                fileStream.Flush();

                binaryFormatter.Serialize(fileStream, playerProfile);
                fileStream.Flush();
            }
        }

        public static void DeleteSave(int slot)
        {
            File.Delete(CrossPlatformGame.SETTINGS_DIRECTORY + SAVE_FOLDER + "\\" + slot + ".sav");
        }

        public static Dictionary<int, Dictionary<string, object>> GetAllSaveData()
        {
            Dictionary<int, Dictionary<string, object>> results = new Dictionary<int, Dictionary<string, object>>();

            string savePath = CrossPlatformGame.SETTINGS_DIRECTORY + SAVE_FOLDER;
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }

            BinaryFormatter binaryFormatter = new BinaryFormatter();
            foreach (string saveFile in Directory.GetFiles(savePath).Where(x => Path.GetExtension(x) == ".sav"))
            {
                Dictionary<string, object> saveData;
                FileInfo fileInfo = new FileInfo(saveFile);
                using (FileStream fileStream = fileInfo.OpenRead())
                {
                    saveData = (Dictionary<string, object>)binaryFormatter.Deserialize(fileStream);
                }

                results.Add(int.Parse(Path.GetFileNameWithoutExtension(saveFile).Replace("Save", "")), saveData);
            }

            return results;
        }

        public static void SetSaveData<T>(string name, T value)
        {
            if (saveData.ContainsKey(name)) saveData[name] = value;
            else saveData.Add(name, value);
        }

        public static T GetSaveData<T>(string name)
        {
            if (saveData.ContainsKey(name)) return (T)saveData[name];

            T newValue = default(T);
            saveData.Add(name, newValue);
            return newValue;
        }

        public static int ParseSlotNumber(string saveName)
        {
            int slotStart = saveName.LastIndexOf(SAVE_FOLDER) + 6;
            int slotEnd = saveName.LastIndexOf('.');

            return int.Parse(Path.GetFileNameWithoutExtension(saveName).Replace("Save", ""));
        }

        public static PlayerProfile PlayerProfile { get => playerProfile; }

        public static List<string> SaveList
        {
            get
            {
                if (!Directory.Exists(CrossPlatformGame.SETTINGS_DIRECTORY + SAVE_FOLDER)) Directory.CreateDirectory(CrossPlatformGame.SETTINGS_DIRECTORY + SAVE_FOLDER);
                return Directory.GetFiles(CrossPlatformGame.SETTINGS_DIRECTORY + SAVE_FOLDER).ToList().FindAll(x => x.Substring(x.IndexOf('.'), 4) == ".sav");
            }
        }
    }
}
