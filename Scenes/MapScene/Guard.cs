using GridTactics.SceneObjects.Maps;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiledCS;

namespace GridTactics.Scenes.MapScene
{
    public class Guard : Npc, IInteractive
    {

        public Guard(MapScene iMapScene, Tilemap iTilemap, TiledObject tiledObject, string spriteName, Orientation iOrientation = Orientation.Down)
            : base(iMapScene, iTilemap, tiledObject, spriteName, iOrientation)
        {
            
        }
    }
}
