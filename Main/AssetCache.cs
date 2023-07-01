using GridTactics.Models;
using K4os.Compression.LZ4;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GridTactics.Main
{
    public static class AssetCache
    {
        public static Dictionary<GameView, string> VIEWS = new Dictionary<GameView, string>();
        public static Dictionary<GameShader, Effect> EFFECTS = new Dictionary<GameShader, Effect>();
        public static Dictionary<GameSprite, Texture2D> SPRITES = new Dictionary<GameSprite, Texture2D>();
        public static Dictionary<GameMap, string> MAPS = new Dictionary<GameMap, string>();
        public static Dictionary<string, string> DATA = new Dictionary<string, string>();

        public static void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            LoadData();
            LoadShaders(graphicsDevice);
            LoadViews();
            LoadSprites(graphicsDevice);
            LoadMaps();
        }

        public static List<Tuple<byte[], byte[]>> LoadAssetData(string jamPath)
        {
            byte[] packedData = File.ReadAllBytes(jamPath);
            int assetCount = BitConverter.ToInt32(packedData, 0);
            int uncompressedSize = BitConverter.ToInt32(packedData, 4);
            byte[] inflatedData = new byte[uncompressedSize];
            LZ4Codec.Decode(packedData, 8, packedData.Length - 8, inflatedData, 0, inflatedData.Length);

            List<Tuple<byte[], byte[]>> rawAssets = new List<Tuple<byte[], byte[]>>();
            int index = 0;
            for (int i = 0; i < assetCount; i++)
            {
                int nameLength = BitConverter.ToInt32(inflatedData, index);
                byte[] nameData = new byte[nameLength];
                Array.Copy(inflatedData, index + 4, nameData, 0, nameLength);
                index += nameLength + 4;

                int dataLength = BitConverter.ToInt32(inflatedData, index);
                byte[] assetData = new byte[dataLength];
                Array.Copy(inflatedData, index + 4, assetData, 0, dataLength);
                index += dataLength + 4;

                rawAssets.Add(new Tuple<byte[], byte[]>(nameData, assetData));
            }

            return rawAssets;
        }

        private static void LoadData()
        {
            if (!File.Exists("Data.jam")) return;

            List<Tuple<byte[], byte[]>> dataAssets = LoadAssetData("Data.jam");
            DATA = dataAssets.ToDictionary(x => Encoding.ASCII.GetString(x.Item1), x => Encoding.ASCII.GetString(x.Item2));
        }

        public static List<T> LoadRecords<T>(string dataFileName)
        {
            Newtonsoft.Json.JsonSerializer deserializer = Newtonsoft.Json.JsonSerializer.Create();
            Newtonsoft.Json.JsonTextReader reader = new Newtonsoft.Json.JsonTextReader(new StringReader(DATA[dataFileName]));
            return deserializer.Deserialize<List<T>>(reader);
        }

        private static void LoadShaders(GraphicsDevice graphicsDevice)
        {
            List<Tuple<byte[], byte[]>> shaderAssets = LoadAssetData("Shaders.jam");
            foreach (Tuple<byte[], byte[]> asset in shaderAssets)
            {
                string shaderName = Encoding.ASCII.GetString(asset.Item1);
                EFFECTS.Add((GameShader)Enum.Parse(typeof(GameShader), shaderName.Replace('\\', '_')), new Effect(graphicsDevice, asset.Item2));
            }
        }

        private static void LoadViews()
        {
            if (!File.Exists("Views.jam")) return;

            List<Tuple<byte[], byte[]>> viewAssets = LoadAssetData("Views.jam");
            foreach (Tuple<byte[], byte[]> asset in viewAssets)
            {
                string viewName = Encoding.ASCII.GetString(asset.Item1);
                VIEWS.Add((GameView)Enum.Parse(typeof(GameView), viewName.Replace('\\', '_')), Encoding.ASCII.GetString(asset.Item2));
            }
        }

        private static void LoadSprites(GraphicsDevice graphicsDevice)
        {
            List<Tuple<byte[], byte[]>> spriteAssets = LoadAssetData("Sprites.jam");
            foreach (Tuple<byte[], byte[]> asset in spriteAssets)
            {
                string spriteName = Encoding.ASCII.GetString(asset.Item1);
                SPRITES.Add((GameSprite)Enum.Parse(typeof(GameSprite), spriteName.Replace('\\', '_')), Texture2D.FromStream(graphicsDevice, new MemoryStream(asset.Item2)));
            }
        }

        private static void LoadMaps()
        {
            if (!File.Exists("Maps.jam")) return;

            List<Tuple<byte[], byte[]>> viewAssets = LoadAssetData("Maps.jam");
            foreach (Tuple<byte[], byte[]> asset in viewAssets)
            {
                string viewName = Encoding.ASCII.GetString(asset.Item1);
                MAPS.Add((GameMap)Enum.Parse(typeof(GameMap), viewName.Replace('\\', '_')), Encoding.ASCII.GetString(asset.Item2));
            }
        }
    }
}
