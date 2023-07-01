using FMOD;
using GridTactics.Models;
using GridTactics.SceneObjects.Maps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiledCS;

namespace GridTactics.Scenes.MapScene
{
    public class Encounter
    {
        public EncounterTable EncounterTable { get; private set; }
        public int EncounterRate { get; private set; }
        public Rectangle Bounds { get; private set; }

        public Dictionary<EncounterResult, double> EncounterResults = new Dictionary<EncounterResult, double>();

        public Encounter(MapScene iMapScene, Tilemap iTilemap, TiledObject iTiledObject)
        {
            EncounterTable = MapScene.ENCOUNTER_TABLES.First(x => x.Name == iTiledObject.properties.First(x => x.name == "EncounterTable").value);
            Bounds = new Rectangle((int)iTiledObject.x, (int)iTiledObject.y, (int)iTiledObject.width, (int)iTiledObject.height);
            EncounterRate = int.Parse(iTiledObject.properties.First(x => x.name == "EncounterRate").value);

            foreach (var encounterResult in EncounterTable.Encounters)
            {
                EncounterResults.Add(encounterResult, encounterResult.Weight);
            }
        }
    }
}
