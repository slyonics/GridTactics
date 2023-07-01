using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace GridTactics.Main
{
    public static class Settings
    {
        private const string SETTINGS_FILE = "\\Settings.xml";

        private static Dictionary<string, object> DEFAULT_PROGRAM_SETTINGS = new Dictionary<string, object>()
        {
            { "DebugMode", false },
            { "Fullscreen", false },
            { "TargetResolution", "Best Fit" },
            { "SoundVolume", 100 },
            { "MusicVolume", 50 }
        };

        private static Dictionary<Command, Keys> DEFAULT_KEYBOARD_BINDINGS = new Dictionary<Command, Keys>()
        {
            { Command.Up, Keys.Up },
            { Command.Right, Keys.Right },
            { Command.Down, Keys.Down },
            { Command.Left, Keys.Left }
        };

        private static Dictionary<Command, Buttons> DEFAULT_GAMEPAD_BINDINGS = new Dictionary<Command, Buttons>()
        {
            { Command.Up, Buttons.DPadUp },
            { Command.Right, Buttons.DPadRight },
            { Command.Down, Buttons.DPadDown },
            { Command.Left, Buttons.DPadLeft }
        };

        private static Dictionary<string, object> programSettings = new Dictionary<string, object>(DEFAULT_PROGRAM_SETTINGS);
        private static Dictionary<Command, Keys> keyboardBindings = new Dictionary<Command, Keys>(DEFAULT_KEYBOARD_BINDINGS);
        private static Dictionary<Command, Buttons> gamePadBindings = new Dictionary<Command, Buttons>(DEFAULT_GAMEPAD_BINDINGS);

        public static void SaveSettings()
        {
            try
            {
                XmlDocument xml = new XmlDocument();
                StringBuilder xmlContents = new StringBuilder();

                xmlContents.AppendLine("<?xml version =\"1.0\" encoding=\"utf-8\" ?>");
                xmlContents.AppendLine("<Settings>");

                xmlContents.AppendLine("<ProgramSettings>");
                foreach (KeyValuePair<string, object> pair in programSettings) xmlContents.AppendLine("<" + pair.Key + ">" + pair.Value.ToString() + "</" + pair.Key + ">");
                xmlContents.AppendLine("</ProgramSettings>");

                xmlContents.AppendLine("<KeyboardBindings>");
                foreach (KeyValuePair<Command, Keys> pair in keyboardBindings) xmlContents.AppendLine("<" + pair.Key + ">" + pair.Value.ToString() + "</" + pair.Key + ">");
                xmlContents.AppendLine("</KeyboardBindings>");

                xmlContents.AppendLine("<GamePadBindings>");
                foreach (KeyValuePair<Command, Buttons> pair in gamePadBindings) xmlContents.AppendLine("<" + pair.Key + ">" + pair.Value.ToString() + "</" + pair.Key + ">");
                xmlContents.AppendLine("</GamePadBindings>");

                xmlContents.AppendLine("</Settings>");

                Directory.CreateDirectory(CrossPlatformGame.SETTINGS_DIRECTORY);
                xml.LoadXml(xmlContents.ToString());

                using (XmlTextWriter writer = new XmlTextWriter(CrossPlatformGame.SETTINGS_DIRECTORY + SETTINGS_FILE, null))
                {
                    writer.Formatting = Formatting.Indented;
                    xml.Save(writer);
                    writer.Close();
                }
            }
            catch (Exception)
            {

            }
        }

        public static void LoadSettings()
        {
            bool dirtySettings = false;
            XmlDocument xml = null;

            try
            {
                Directory.CreateDirectory(CrossPlatformGame.SETTINGS_DIRECTORY);

                xml = new XmlDocument();
                xml.Load(CrossPlatformGame.SETTINGS_DIRECTORY + SETTINGS_FILE);

                XmlNodeList nodeList = xml.SelectNodes("/Settings/ProgramSettings/*");
                foreach (XmlNode node in nodeList)
                {
                    if (programSettings[node.Name] is bool) programSettings[node.Name] = bool.Parse(node.InnerText);
                    else if (programSettings[node.Name] is int) programSettings[node.Name] = int.Parse(node.InnerText);
                    else if (programSettings[node.Name] is float) programSettings[node.Name] = float.Parse(node.InnerText, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture);
                    if (programSettings[node.Name] is string) programSettings[node.Name] = node.InnerText;
                }
            }
            catch (Exception)
            {
                programSettings = new Dictionary<string, object>(DEFAULT_PROGRAM_SETTINGS);
                dirtySettings = true;
            }

            try
            {
                xml = new XmlDocument();
                xml.Load(CrossPlatformGame.SETTINGS_DIRECTORY + SETTINGS_FILE);

                XmlNodeList nodeList = xml.SelectNodes("/Settings/KeyboardBindings/*");
                foreach (XmlNode node in nodeList) keyboardBindings[(Command)Enum.Parse(typeof(Command), node.Name)] = (Keys)Enum.Parse(typeof(Keys), node.InnerText, true);
            }
            catch (Exception)
            {
                keyboardBindings = new Dictionary<Command, Keys>(DEFAULT_KEYBOARD_BINDINGS);
                dirtySettings = true;
            }

            try
            {
                xml = new XmlDocument();
                xml.Load(CrossPlatformGame.SETTINGS_DIRECTORY + SETTINGS_FILE);

                XmlNodeList nodeList = xml.SelectNodes("/Settings/GamePadBindings/*");
                foreach (XmlNode node in nodeList) gamePadBindings[(Command)Enum.Parse(typeof(Command), node.Name)] = (Buttons)Enum.Parse(typeof(Buttons), node.InnerText, true);
            }
            catch (Exception)
            {
                gamePadBindings = new Dictionary<Command, Buttons>(DEFAULT_GAMEPAD_BINDINGS);
                dirtySettings = true;
            }

            if (dirtySettings) SaveSettings();
        }

        public static void ResetSettings()
        {
            programSettings = new Dictionary<string, object>(DEFAULT_PROGRAM_SETTINGS);
            keyboardBindings = new Dictionary<Command, Keys>(DEFAULT_KEYBOARD_BINDINGS);
            gamePadBindings = new Dictionary<Command, Buttons>(DEFAULT_GAMEPAD_BINDINGS);
        }

        public static void SetProgramSetting<T>(string name, T value)
        {
            if (programSettings.ContainsKey(name)) programSettings[name] = value;
            else programSettings.Add(name, value);
        }

        public static T GetProgramSetting<T>(string name)
        {
            return (T)programSettings[name];
        }

        public static Dictionary<Command, Keys> KeyboardBindings
        {
            get
            {
                return keyboardBindings;
            }
        }

        public static Dictionary<Command, Buttons> GamePadBindings
        {
            get
            {
                return gamePadBindings;
            }
        }
    }
}
