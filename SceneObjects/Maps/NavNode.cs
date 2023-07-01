using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTactics.SceneObjects.Maps
{
    public class NavNode
    {
        public const int NODE_SIZE = 8;

        private int nodeX;
        private int nodeY;
        private Vector2 center;

        private Tilemap map;
        private List<Tile> parentTiles = new List<Tile>();
        private List<NavNode> neighborList = new List<NavNode>();

        public NavNode(Tilemap iMap, int x, int y)
        {
            nodeX = x;
            nodeY = y;
            center = new Vector2(x * NODE_SIZE + NODE_SIZE, y * NODE_SIZE + NODE_SIZE);
            map = iMap;

            parentTiles.Add(map.GetTile(new Vector2((int)center.X - 2, (int)center.Y - 2)));
            parentTiles.Add(map.GetTile(new Vector2((int)center.X + 2, (int)center.Y - 2)));
            parentTiles.Add(map.GetTile(new Vector2((int)center.X - 2, (int)center.Y + 2)));
            parentTiles.Add(map.GetTile(new Vector2((int)center.X + 2, (int)center.Y + 2)));
            parentTiles = parentTiles.Distinct().ToList();
        }

        public void AssignNeighbors(Tilemap map)
        {
            if (nodeX > 0) neighborList.Add(map.GetNavNode(nodeX - 1, nodeY));
            if (nodeY > 0) neighborList.Add(map.GetNavNode(nodeX, nodeY - 1));
            if (nodeY < map.Height * 2 - 2) neighborList.Add(map.GetNavNode(nodeX, nodeY + 1));
            if (nodeX < map.Width * 2 - 2) neighborList.Add(map.GetNavNode(nodeX + 1, nodeY));

            if (nodeX > 0 && nodeY > 0) neighborList.Add(map.GetNavNode(nodeX - 1, nodeY - 1));
            if (nodeX > 0 && nodeY < map.Height * 2 - 2) neighborList.Add(map.GetNavNode(nodeX - 1, nodeY + 1));
            if (nodeX < map.Width * 2 - 2 && nodeY > 0) neighborList.Add(map.GetNavNode(nodeX + 1, nodeY - 1));
            if (nodeX < map.Width * 2 - 2 && nodeY < map.Height * 2 - 2) neighborList.Add(map.GetNavNode(nodeX + 1, nodeY + 1));
        }

        public bool AccessibleFromNode(NavNode origin, Actor actor)
        {
            List<Rectangle> potentialColliders = new List<Rectangle>();
            potentialColliders.AddRange(origin.ColliderList);
            potentialColliders.AddRange(actor.NearbyColliders);
            if (actor.ActorColliders != null) potentialColliders.AddRange(actor.ActorColliders);

            Rectangle startRectangle = origin.BoundsForActor(actor);
            Rectangle endRectangle = BoundsForActor(actor);
            Rectangle pathRectangle = Rectangle.Union(startRectangle, endRectangle);

            foreach (Rectangle collider in potentialColliders)
            {
                if (collider.Intersects(pathRectangle)) return false;
            }

            return true;
        }

        public bool AccessibleFromActor(Actor actor)
        {
            List<Rectangle> potentialColliders = new List<Rectangle>();
            potentialColliders.AddRange(this.ColliderList);
            potentialColliders.AddRange(actor.NearbyColliders);
            if (actor.ActorColliders != null) potentialColliders.AddRange(actor.ActorColliders);

            Rectangle endRectangle = BoundsForActor(actor);
            Rectangle pathRectangle = Rectangle.Union(actor.Bounds, endRectangle);

            foreach (Rectangle collider in potentialColliders)
            {
                if (collider.Intersects(pathRectangle)) return false;
            }

            return true;
        }

        public Rectangle BoundsForActor(Actor actor)
        {
            return new Rectangle((int)(this.Center.X + actor.BoundingBox.Left), (int)(this.Center.Y + actor.BoundingBox.Top), actor.BoundingBox.Width, actor.BoundingBox.Height);
        }

        public bool FitsActor(Actor actor)
        {
            Rectangle boundsForActor = BoundsForActor(actor);

            int tileStartX = boundsForActor.Left / map.TileWidth;
            int tileEndX = boundsForActor.Right / map.TileWidth;
            int tileStartY = boundsForActor.Top / map.TileHeight;
            int tileEndY = boundsForActor.Bottom / map.TileHeight;

            List<Rectangle> colliderList = new List<Rectangle>();
            for (int x = tileStartX; x <= tileEndX; x++)
            {
                for (int y = tileStartY; y <= tileEndY; y++)
                {
                    colliderList.AddRange(map.GetTile(x, y).ColliderList);
                }
            }

            foreach (Rectangle collider in colliderList)
            {
                if (collider.Intersects(boundsForActor)) return false;
            }

            return true;
        }

        public bool Collides(Rectangle bounds)
        {
            List<Rectangle> colliderList = ColliderList;
            if (colliderList.Exists(x => x.Intersects(bounds))) return true;

            return false;
        }

        public List<Rectangle> ColliderList
        {
            get
            {
                List<Rectangle> result = new List<Rectangle>();
                foreach (Tile tile in parentTiles) result.AddRange(tile.ColliderList);
                return result;
            }
        }

        public Vector2 Center { get => center; }
        public List<NavNode> NeighborList { get => neighborList; }
    }
}
