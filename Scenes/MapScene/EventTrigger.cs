using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TiledCS;

namespace GridTactics.Scenes.MapScene
{
    public class EventTrigger : IInteractive
    {
        private MapScene mapScene;
        private TiledObject objectData;

        public string[] Script { get; set; }

        public bool TravelZone { get; set; }
        public bool DefaultTravelZone { get; set; } = true;

        public EventTrigger(MapScene iMapScene, TiledObject iObjectData)
        {
            mapScene = iMapScene;
            objectData = iObjectData;

            Bounds = new Rectangle((int)objectData.x, (int)objectData.y, (int)objectData.width, (int)objectData.height);

            switch (objectData.type)
            {
                case "Automatic":
                    Script = objectData.properties.FirstOrDefault(x => x.name == "Script").value.Split('\n');
                    Interactive = false;
                    break;

                case "Travel":
                    Interactive = true;
                    Label = objectData.properties.FirstOrDefault(x => x.name == "Label").value;
                    Script = new string[] { "ChangeMap " + objectData.name };
                    TravelZone = true;
                    if (objectData.properties.Any(x => x.name == "NoDefault")) DefaultTravelZone = false;
                    break;

                case "Interactive":
                    Interactive = true;
                    Label = objectData.properties.FirstOrDefault(x => x.name == "Label").value;
                    Script = objectData.properties.FirstOrDefault(x => x.name == "Script").value.Split('\n');
                    break;
            }
        }

        public bool Activate(Hero hero)
        {
            if (!Interactive) return false;

            mapScene.AddController(new EventController(mapScene, Script));

            return true;
        }

        public string GetProperty(string propertyname)
        {
            return objectData.properties.FirstOrDefault(x => x.name == propertyname).value;
        }

        public string Name { get => objectData.name; }
        public Rectangle Bounds { get; private set; }
        public bool Interactive { get; set; }
        public bool Terminated { get; set; }

        public string Label { get; set; } = "Trigger";
        public Vector2 LabelPosition { get => new Vector2(Bounds.Center.X, Bounds.Center.Y - Bounds.Height); }
    }
}
