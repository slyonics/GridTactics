using GridTactics.GameObjects.Maps;
using GridTactics.Models;
using GridTactics.SceneObjects.Maps;
using GridTactics.SceneObjects.Shaders;
using GridTactics.Scenes.BattleScene;
using GridTactics.Scenes.StatusScene;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiledCS;

namespace GridTactics.Scenes.MapScene
{
    public class MapScene : Scene
    {
        public static MapScene Instance;

        public const float SIGHT_RANGE = 20.0f;

        public Tilemap Tilemap { get; set; }

        public List<Hero> Party { get; private set; } = new List<Hero>();
        public Hero PartyLeader { get => Party.FirstOrDefault(); }

        public List<Npc> NPCs { get; private set; } = new List<Npc>();
        public List<Encounter> Encounters { get; private set; } = new List<Encounter>();
        public List<EventTrigger> EventTriggers { get; private set; } = new List<EventTrigger>();

        private ParallaxBackdrop parallaxBackdrop;

        private List<Tuple<Rectangle, string>> musicZones = new List<Tuple<Rectangle, string>>();

        public WeatherController WeatherController { get; set; }

        public MapScene(string mapName)
        {
            Instance = this;

            Color mapColor = Color.White;

            Tilemap = AddEntity(new Tilemap(this, (GameMap)Enum.Parse(typeof(GameMap), mapName)));
            foreach (TiledProperty tiledProperty in Tilemap.MapData.Properties)
            {
                switch (tiledProperty.name)
                {
                    case "Music": if (!GameProfile.GetSaveData<bool>("NewGame")) Audio.PlayMusic((GameMusic)Enum.Parse(typeof(GameMusic), tiledProperty.value)); else Audio.StopMusic(); break;
                    case "Script": AddController(new EventController(this, tiledProperty.value.Split('\n'))); break;

                    case "ColorFilter": SceneShader = new SceneObjects.Shaders.ColorFade(Graphics.ParseHexcode("#" + tiledProperty.value.Substring(3)), 0.75f); break;
                    case "DayNight": SceneShader = new SceneObjects.Shaders.DayNight(Graphics.ParseHexcode("#" + tiledProperty.value.Substring(3)), 1.2f); mapColor = Graphics.ParseHexcode("#" + tiledProperty.value.Substring(3)); break;
                    case "HeatDistortion": SceneShader = new SceneObjects.Shaders.HeatDistortion(); break;

                    case "Background": BuildParallaxBackground(tiledProperty.value); break;
                }
            }

            if (GameProfile.GetSaveData<bool>("NewGame")) SceneShader = new SceneObjects.Shaders.DayNight(Color.Black, 1.0f);

            Camera = new Camera(new Rectangle(0, 0, Tilemap.Width, Tilemap.Height));
            Tilemap.ClearFieldOfView();

            var leaderHero = new Hero(this, Tilemap, new Vector2(32, 96), 0, GameProfile.PlayerProfile.PlayerSprite.Value);
            Party.Add(leaderHero);

            // add followers

            foreach (var partymember in Party.Reverse<Hero>())
            {
                AddEntity(partymember);
            }

            foreach (Tuple<TiledLayer, TiledGroup> layer in Tilemap.ObjectData)
            {
                foreach (TiledObject tiledObject in layer.Item1.objects)
                {
                    var prop = tiledObject.properties.FirstOrDefault(x => x.name == "EnableIf");
                    if (prop != null && !GameProfile.GetSaveData<bool>(prop.value)) continue;

                    prop = tiledObject.properties.FirstOrDefault(x => x.name == "DisableIf");
                    if (prop != null && GameProfile.GetSaveData<bool>(prop.value)) continue;

                    switch (layer.Item1.name)
                    {
                        case "Triggers":
                            {
                                EventTriggers.Add(new EventTrigger(this, tiledObject));
                            }
                            break;

                        case "NPCs":
                            {
                                Npc npc = new Npc(this, Tilemap, tiledObject, "Base");
                                NpcController npcController = new NpcController(this, npc);
                                NPCs.Add(npc);
                                AddEntity(npc);
                                AddController(npcController);
                            }
                            break;

                        case "Music":
                            {
                                Rectangle zone = new Rectangle((int)tiledObject.x, (int)tiledObject.y, (int)tiledObject.width, (int)tiledObject.height);
                                musicZones.Add(new Tuple<Rectangle, string>(zone, tiledObject.name));
                            }
                            break;

                        case "Encounters":
                            {
                                Encounter encounter = new Encounter(this, Tilemap, tiledObject);
                                Encounters.Add(encounter);
                            }
                            break;
                    }
                }
            }

            var dayNight = SceneShader as DayNight;
            if (dayNight != null)
            {
                WeatherController = new WeatherController(dayNight, GameProfile.WorldTime, 2, !mapName.Contains("Surface"));
                AddController(WeatherController);
                WeatherController.PreUpdate(new GameTime());

                if (GameProfile.GetSaveData<bool>("NewGame")) WeatherController.AmbientLight = new Color(0, 0, 0, 255);
                else if (!WeatherController.Indoors) dayNight.Ambient = WeatherController.AmbientLight.ToVector4();
                else WeatherController.AmbientLight = mapColor;
            }
        }

        public MapScene(string mapName, int startX, int startY, Orientation orientation)
            : this(mapName)
        {
            PartyLeader.CenterOn(Tilemap.GetTile(startX, startY).Center);
            PartyLeader.Orientation = orientation;
            PartyLeader.Idle();

            PartyLeader.UpdateBounds();

            int i = 1;
            foreach (Hero hero in Party.Skip(1))
            {
                hero.CenterOn(Tilemap.GetTile(startX, startY).Center);
                hero.Orientation = orientation;
                hero.Idle();

                i++;
            }

            Camera.Center(PartyLeader.Center);

            foreach (Hero hero in Party) Tilemap.CalculateFieldOfView(Tilemap.GetTile(hero.Center), SIGHT_RANGE);
            Tilemap.UpdateVisibility();

            SaveMapPosition();
        }

        public MapScene(string mapName, Vector2 leaderPosition)
            : this(mapName)
        {
            PartyLeader.Position = leaderPosition;
            PartyLeader.CenterLight();
            PartyLeader.Orientation = Orientation.Down;
            PartyLeader.Idle();

            PartyLeader.UpdateBounds();
            Camera.Center(PartyLeader.Center);

            int i = 1;
            foreach (Hero hero in Party.Skip(1))
            {
                hero.CenterOn(new Vector2(PartyLeader.SpriteBounds.Left + i * 6, PartyLeader.SpriteBounds.Bottom - 12 + (i % 2) * 6));
                hero.Orientation = Orientation.Down;
                hero.Idle();

                i++;
            }

            foreach (Hero hero in Party) Tilemap.CalculateFieldOfView(Tilemap.GetTile(hero.Center), SIGHT_RANGE);
            Tilemap.UpdateVisibility();

            SaveMapPosition();
        }

        public MapScene(string mapName, string sourceMapName)
            : this(mapName)
        {
            var spawnZone = EventTriggers.First(x => x.Name == sourceMapName);

            Orientation orientation = (Orientation)Enum.Parse(typeof(Orientation), spawnZone.GetProperty("Direction"));

            Vector2 spawnPosition = Tilemap.GetTile(new Vector2(spawnZone.Bounds.Center.X, spawnZone.Bounds.Center.Y)).Center;
            switch (orientation)
            {
                case Orientation.Up: spawnPosition.Y -= 16; break;
                case Orientation.Right: spawnPosition.X += spawnZone.Bounds.Width; break;
                case Orientation.Down: spawnPosition.Y += 16; break;
                case Orientation.Left: spawnPosition.X -= 16; break;
            }
            PartyLeader.CenterOn(spawnPosition);
            PartyLeader.Orientation = orientation;
            PartyLeader.Idle();

            PartyLeader.UpdateBounds();
            Camera.Center(PartyLeader.Center);

            int i = 1;
            foreach (Hero hero in Party.Skip(1))
            {
                hero.CenterOn(new Vector2(PartyLeader.SpriteBounds.Left + i * 6, PartyLeader.SpriteBounds.Bottom - 12 + (i % 2) * 6));
                hero.Orientation = orientation;
                hero.Idle();

                i++;
            }

            foreach (Hero hero in Party) Tilemap.CalculateFieldOfView(Tilemap.GetTile(hero.Center), SIGHT_RANGE);
            Tilemap.UpdateVisibility();

            SaveMapPosition();
        }

        public static void Initialize()
        {
            ENCOUNTER_TABLES = AssetCache.LoadRecords<EncounterTable>("EncounterTables");
        }

        public override void BeginScene()
        {
            base.BeginScene();

            CheckMusicZone();
        }

        public void SaveMapPosition()
        {
            GameProfile.SetSaveData<string>("LastMapName", Tilemap.Name);
            GameProfile.SetSaveData<int>("LastPositionX", (int)PartyLeader.Position.X);
            GameProfile.SetSaveData<int>("LastPositionY", (int)PartyLeader.Position.Y);
            GameProfile.SetSaveData<string>("PlayerLocation", Tilemap.MapData.Properties.First(x => x.name == "LocationName").value);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            Camera.Center(PartyLeader.Center);

            

            NPCs.RemoveAll(x => x.Terminated);

            parallaxBackdrop?.Update(gameTime, Camera);

            if (!ControllerStack.Any(x => x.Any(y => y is CaterpillarController)))
            {
                AddController(new CaterpillarController(this));
            }
        }

        public bool ProcessAutoEvents()
        {
            bool eventTriggered = false;
            foreach (EventTrigger eventTrigger in EventTriggers)
            {
                if (eventTrigger.Bounds.Intersects(PartyLeader.Bounds) && !eventTrigger.Interactive)
                {
                    eventTriggered = true;
                    eventTrigger.Terminated = true;
                    AddController(new EventController(this, eventTrigger.Script));
                }
            }
            EventTriggers.RemoveAll(x => x.Terminated);

            return eventTriggered;
        }

        public override void DrawBackground(SpriteBatch spriteBatch)
        {
            parallaxBackdrop?.Draw(spriteBatch);

            Tilemap.DrawBackground(spriteBatch, Camera);
        }

        private void BuildParallaxBackground(string background)
        {
            string[] tokens = background.Split(' ');

            parallaxBackdrop = new ParallaxBackdrop(tokens[0], tokens.Skip(1).Select(x => float.Parse(x, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture)).ToArray());
        }

        public override void DrawGame(SpriteBatch spriteBatch, Effect shader, Matrix matrix)
        {
            base.DrawGame(spriteBatch, shader, matrix);
        }

        public void HandleOffscreen()
        {
            var travelZone = EventTriggers.Where(x => x.TravelZone && x.DefaultTravelZone).OrderBy(x => Vector2.Distance(new Vector2(x.Bounds.Center.X, x.Bounds.Center.Y), PartyLeader.Position)).First();
            travelZone.Activate(PartyLeader);
        }

        public void CheckMusicZone()
        {
            var tuple = musicZones.FirstOrDefault(x => x.Item1.Intersects(PartyLeader.SpriteBounds));
            if (tuple != null)
            {
                Audio.PlayMusic((GameMusic)Enum.Parse(typeof(GameMusic), tuple.Item2));
            }
        }
    }
}
