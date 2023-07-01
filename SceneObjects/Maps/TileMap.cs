using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiledCS;

namespace GridTactics.SceneObjects.Maps
{
    public class Tilemap : Entity
    {
        private class Tileset
        {
            public Tileset(TiledTileset iTiledTileset)
            {
                TiledTileset = iTiledTileset;
                SpriteAtlas = AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), "Tiles_" + Path.GetFileNameWithoutExtension(TiledTileset.Image.source))];
            }

            public TiledTileset TiledTileset { get; private set; }
            public Texture2D SpriteAtlas { get; private set; }
        }

        private GameMap gameMap;
        private TiledMap tiledMap;
        private Dictionary<int, Tileset> tilesets = new Dictionary<int, Tileset>();

        private Tile[,] tiles;

        private NavNode[,] navMesh;

        private List<Tile> visibleTiles = new List<Tile>();
        private bool revealAll;

        public string Name { get => gameMap.ToString(); }

        public Tilemap(Scene iScene, GameMap iGameMap)
            : base(iScene, Vector2.Zero)
        {
            gameMap = iGameMap;

            tiledMap = new TiledMap();
            tiledMap.ParseXml(AssetCache.MAPS[gameMap]);
            foreach (TiledMapTileset tiledMapTileset in tiledMap.Tilesets)
            {
                TiledTileset tiledTileset = new TiledTileset();
                tiledTileset.ParseXml(AssetCache.MAPS[(GameMap)Enum.Parse(typeof(GameMap), "Tilesets_" + Path.GetFileNameWithoutExtension(tiledMapTileset.source))]);
                tilesets.Add(tiledMapTileset.firstgid, new Tileset(tiledTileset));
            }

            tiles = new Tile[Columns, Rows];
            for (int y = 0; y < Rows; y++)
            {
                for (int x = 0; x < Columns; x++)
                {
                    tiles[x, y] = new Tile(this, x, y);
                }
            }

            foreach (TiledGroup tiledGroup in tiledMap.Groups) LoadLayers(tiledGroup.layers, tiledGroup);
            LoadLayers(tiledMap.Layers, null);

            for (int y = Rows - 1; y >= 0; y--)
            {
                for (int x = 0; x < Columns; x++)
                {
                    tiles[x, y].AssignNeighbors();
                }
            }

            BuildNavMesh();
        }

        protected virtual void LoadLayers(TiledLayer[] tiledLayers, TiledGroup tiledGroup)
        {
            foreach (TiledLayer tiledLayer in tiledLayers)
            {
                switch (tiledLayer.type)
                {
                    case TiledLayerType.TileLayer: LoadTileLayer(tiledLayer, tiledGroup); break;
                    case TiledLayerType.ObjectLayer: LoadObjectLayer(tiledLayer, tiledGroup); break;
                    case TiledLayerType.ImageLayer: LoadImageLayer(tiledLayer, tiledGroup); break;
                }
            }
        }

        protected virtual void LoadTileLayer(TiledLayer tiledLayer, TiledGroup tiledGroup)
        {
            if (tiledGroup == null || tiledGroup.name == "Background")
            {
                int i = 0;
                for (int y = 0; y < Rows; y++)
                {
                    for (int x = 0; x < Columns; x++, i++)
                    {
                        int tileId = tiledLayer.data[i];
                        if (tileId == 0) continue;

                        TiledMapTileset tiledMapTileset = tiledMap.GetTiledMapTileset(tileId);
                        Tileset tileset = tilesets[tiledMapTileset.firstgid];
                        TiledTileset tiledTileset = tileset.TiledTileset;
                        TiledTile tilesetTile = tiledMap.GetTiledTile(tiledMapTileset, tiledTileset, tileId);
                        TiledSourceRect spriteSource = tiledMap.GetSourceRect(tiledMapTileset, tiledTileset, tileId);

                        tiles[x, y].ApplyBackgroundTile(tilesetTile, tiledLayer, new Rectangle(spriteSource.x, spriteSource.y, spriteSource.width, spriteSource.height), tileset.SpriteAtlas);
                    }
                }
            }
            else if (tiledGroup.name.Contains("Height"))
            {
                int height = int.Parse(tiledGroup.name.Remove(0, 6));

                int i = 0;
                for (int y = 0; y < Rows; y++)
                {
                    for (int x = 0; x < Columns; x++, i++)
                    {
                        int tileId = tiledLayer.data[i];
                        if (tileId == 0) continue;

                        TiledMapTileset tiledMapTileset = tiledMap.GetTiledMapTileset(tileId);
                        Tileset tileset = tilesets[tiledMapTileset.firstgid];
                        TiledTileset tiledTileset = tileset.TiledTileset;
                        TiledTile tilesetTile = tiledMap.GetTiledTile(tiledMapTileset, tiledTileset, tileId);
                        TiledSourceRect spriteSource = tiledMap.GetSourceRect(tiledMapTileset, tiledTileset, tileId);

                        tiles[x, y].ApplyEntityTile(tilesetTile, tiledLayer, new Rectangle(spriteSource.x, spriteSource.y, spriteSource.width, spriteSource.height), tileset.SpriteAtlas, height);
                    }
                }
            }
        }

        protected virtual void LoadObjectLayer(TiledLayer tiledLayer, TiledGroup tiledGroup)
        {
            ObjectData.Add(new Tuple<TiledLayer, TiledGroup>(tiledLayer, tiledGroup));
        }

        protected virtual void LoadImageLayer(TiledLayer tiledLayer, TiledGroup tiledGroup)
        {

        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            for (int y = 0; y < Rows; y++)
            {
                for (int x = 0; x < Columns; x++)
                {
                    tiles[x, y].Update(gameTime);
                }
            }
        }

        public void DrawBackground(SpriteBatch spriteBatch, Camera camera)
        {
            int startTileX = Math.Max((int)(camera.View.Left / tiledMap.TileWidth) - 1, 0);
            int startTileY = Math.Max((int)(camera.View.Top / tiledMap.TileHeight) - 1, 0);
            int endTileX = Math.Min((int)(camera.View.Right / tiledMap.TileWidth), Columns - 1);
            int endTileY = Math.Min((int)(camera.View.Bottom / tiledMap.TileHeight), Rows - 1);

            for (int y = startTileY; y <= endTileY; y++)
            {
                for (int x = startTileX; x <= endTileX; x++)
                {
                    tiles[x, y].DrawBackground(spriteBatch, camera);
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch, Camera camera)
        {
            int startTileX = Math.Max((int)(camera.View.Left / tiledMap.TileWidth) - 1, 0);
            int startTileY = Math.Max((int)(camera.View.Top / tiledMap.TileHeight) - 1, 0);
            int endTileX = Math.Min((int)(camera.View.Right / tiledMap.TileWidth), Columns - 1);
            int endTileY = Math.Min((int)(camera.View.Bottom / tiledMap.TileHeight), Rows - 1);

            for (int y = startTileY; y <= endTileY; y++)
            {
                for (int x = startTileX; x <= endTileX; x++)
                {
                    tiles[x, y].Draw(spriteBatch, camera);
                }
            }
        }

        public override void DrawShader(SpriteBatch spriteBatch, Camera camera, Matrix matrix)
        {

        }

        public Tile GetTile(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Columns || y >= Rows) return null;
            return tiles[x, y];
        }

        public Tile GetTile(Vector2 position)
        {
            int tileX = (int)(position.X / TileWidth);
            int tileY = (int)(position.Y / TileHeight);

            if (tileX < 0 || tileY < 0 || tileX >= Columns || tileY >= Rows) return null;

            return tiles[tileX, tileY];
        }

        public NavNode GetNavNode(Vector2 position)
        {
            int nodeX = (int)(position.X / NavNode.NODE_SIZE) - 1;
            int nodeY = (int)(position.Y / NavNode.NODE_SIZE) - 1;
            if (nodeX < 0) nodeX = 0;
            if (nodeY < 0) nodeY = 0;
            if (nodeX > Columns * 2 - 2) nodeX = Columns * 2 - 2;
            if (nodeY > Rows * 2 - 2) nodeY = Rows * 2 - 2;

            return navMesh[nodeX, nodeY];
        }

        public NavNode GetNavNode(Actor actor)
        {
            int nodeX = (int)(actor.Center.X / NavNode.NODE_SIZE) - 1;
            int nodeY = (int)(actor.Bounds.Bottom / NavNode.NODE_SIZE) - 1;
            if (nodeX < 0) nodeX = 0;
            if (nodeY < 0) nodeY = 0;
            if (nodeX > Columns * 2 - 2) nodeX = Columns * 2 - 2;
            if (nodeY > Rows * 2 - 2) nodeY = Rows * 2 - 2;

            NavNode closestNode = navMesh[nodeX, nodeY];

            List<NavNode> nodeList = new List<NavNode>();
            nodeList.Add(closestNode);
            nodeList.AddRange(closestNode.NeighborList);

            IOrderedEnumerable<NavNode> sortedNodes = nodeList.OrderBy(x => Vector2.Distance(x.Center, new Vector2(actor.Center.X, actor.Bounds.Bottom)));
            return sortedNodes.FirstOrDefault(x => x.AccessibleFromActor(actor));
        }

        public NavNode GetNavNode(Actor seeker, Actor target)
        {
            NavNode targetNode = GetNavNode(target);
            if (targetNode == null) return null;
            if (targetNode.FitsActor(seeker)) return targetNode;

            List<NavNode> nodeList = new List<NavNode>();
            nodeList.Add(targetNode);
            nodeList.AddRange(targetNode.NeighborList);

            IOrderedEnumerable<NavNode> sortedNodes = nodeList.OrderBy(x => Vector2.Distance(x.Center, new Vector2(seeker.Center.X, seeker.Bounds.Bottom)));
            return sortedNodes.FirstOrDefault(x => x.FitsActor(seeker));
        }

        public NavNode GetNavNode(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Columns * 2 - 1 || y >= Rows * 2 - 1) return null;

            return navMesh[x, y];
        }

        private void BuildNavMesh()
        {
            navMesh = new NavNode[Columns * 2 - 1, Rows * 2 - 1];
            for (int y = 0; y < Rows * 2 - 1; y++)
            {
                for (int x = 0; x < Columns * 2 - 1; x++)
                {
                    navMesh[x, y] = new NavNode(this, x, y);
                }
            }

            for (int y = 0; y < Rows * 2 - 1; y++)
            {
                for (int x = 0; x < Columns * 2 - 1; x++)
                {
                    navMesh[x, y].AssignNeighbors(this);
                }
            }
        }

        public List<NavNode> GetPath(NavNode startNode, NavNode endNode, Actor actor, int searchLimit)
        {
            List<NavNode> processedNodes = new List<NavNode>();
            List<NavNode> unprocessedNodes = new List<NavNode> { startNode };
            Dictionary<NavNode, NavNode> cameFrom = new Dictionary<NavNode, NavNode>();
            Dictionary<NavNode, int> currentDistance = new Dictionary<NavNode, int>();
            Dictionary<NavNode, int> predictedDistance = new Dictionary<NavNode, int>();

            int searchCount = 0;

            currentDistance.Add(startNode, 0);
            predictedDistance.Add(startNode, (int)Vector2.Distance(startNode.Center, endNode.Center));

            while (unprocessedNodes.Count > 0 && searchCount < searchLimit)
            {
                searchCount++;

                // get the node with the lowest estimated cost to finish
                NavNode current = (from p in unprocessedNodes orderby predictedDistance[p] ascending select p).First();

                // if it is the finish, return the path
                if (current == endNode)
                {
                    // generate the found path
                    return ReconstructPath(cameFrom, endNode);
                }

                // move current node from open to closed
                unprocessedNodes.Remove(current);
                processedNodes.Add(current);

                foreach (NavNode neighbor in current.NeighborList)
                {
                    if (neighbor.AccessibleFromNode(current, actor))
                    {
                        int tempCurrentDistance = currentDistance[current] + (int)Vector2.Distance(current.Center, neighbor.Center);

                        // if we already know a faster way to this neighbor, use that route and ignore this one
                        if (currentDistance.ContainsKey(neighbor) && tempCurrentDistance >= currentDistance[neighbor]) continue;

                        // if we don't know a route to this neighbor, or if this is faster, store this route
                        if (!processedNodes.Contains(neighbor) || tempCurrentDistance < currentDistance[neighbor])
                        {
                            if (cameFrom.Keys.Contains(neighbor)) cameFrom[neighbor] = current;
                            else cameFrom.Add(neighbor, current);

                            currentDistance[neighbor] = tempCurrentDistance;
                            predictedDistance[neighbor] = currentDistance[neighbor] + (int)Vector2.Distance(neighbor.Center, endNode.Center);

                            if (!unprocessedNodes.Contains(neighbor)) unprocessedNodes.Add(neighbor);
                        }
                    }
                }
            }

            return null;
        }

        private static List<NavNode> ReconstructPath(Dictionary<NavNode, NavNode> cameFrom, NavNode current)
        {
            if (!cameFrom.Keys.Contains(current))
            {
                return new List<NavNode> { current };
            }

            List<NavNode> path = ReconstructPath(cameFrom, cameFrom[current]);
            path.Add(current);
            return path;
        }

        public bool CanTraverse(Actor actor, Tile destinationTile)
        {
            Rectangle destinationBounds = new Rectangle((int)destinationTile.Center.X + actor.BoundingBox.Left, (int)destinationTile.Center.Y + actor.BoundingBox.Top, actor.BoundingBox.Width, actor.BoundingBox.Height);
            Rectangle boundsForActor = Rectangle.Union(actor.Bounds, destinationBounds);

            int tileStartX = boundsForActor.Left / TileWidth;
            int tileEndX = boundsForActor.Right / TileWidth;
            int tileStartY = boundsForActor.Top / TileHeight;
            int tileEndY = boundsForActor.Bottom / TileHeight;

            List<Rectangle> colliderList = new List<Rectangle>();
            for (int x = tileStartX; x <= tileEndX; x++)
            {
                for (int y = tileStartY; y <= tileEndY; y++)
                {
                    colliderList.AddRange(GetTile(x, y).ColliderList);
                }
            }

            foreach (Rectangle collider in colliderList)
            {
                if (collider.Intersects(boundsForActor)) return false;
            }

            return true;
        }

        public void ClearFieldOfView()
        {
            for (int y = 0; y < tiledMap.Height; y++)
            {
                for (int x = 0; x < tiledMap.Width; x++)
                {
                    tiles[x, y].Obscured = !revealAll;
                }
            }
        }

        public void CalculateFieldOfView(Tile sourceTile, float viewRadius)
        {
            if (revealAll) return;

            sourceTile.Obscured = false;
            for (int txidx = 0; txidx < OctantTransform.s_octantTransform.Length; txidx++)
            {
                CastLight(sourceTile, viewRadius, 1, 1.0f, 0.0f, OctantTransform.s_octantTransform[txidx]);
            }
        }

        public void UpdateVisibility()
        {
            for (int y = 0; y < tiledMap.Height; y++)
            {
                for (int x = 0; x < tiledMap.Width; x++)
                {
                    tiles[x, y].UpdateVisibility();
                }
            }
        }

        /// <summary>
        /// Recursively casts light into cells.  Operates on a single octant.
        /// Adapted from source code at http://www.roguebasin.com/index.php?title=FOV_using_recursive_shadowcasting by Fadden
        /// </summary>
        /// <param name="sourceTile">The player's current tile after moving.</param>
        /// <param name="viewRadius">The view radius; can be a fractional value.</param>
        /// <param name="startColumn">Current column; pass 1 as initial value.</param>
        /// <param name="leftViewSlope">Slope of the left (upper) view edge; pass 1.0 as
        ///   the initial value.</param>
        /// <param name="rightViewSlope">Slope of the right (lower) view edge; pass 0.0 as
        ///   the initial value.</param>
        /// <param name="txfrm">Coordinate multipliers for the octant transform.</param>
        ///
        /// Maximum recursion depth is (Ceiling(viewRadius)).
        private void CastLight(Tile sourceTile, float viewRadius, int startColumn, float leftViewSlope, float rightViewSlope, OctantTransform txfrm)
        {
            // Used for distance test.
            float viewRadiusSquared = viewRadius * viewRadius;
            int viewCeiling = (int)Math.Ceiling(viewRadius);

            // Set true if the previous cell we encountered was blocked.
            bool prevWasBlocked = false;

            // As an optimization, when scanning past a block we keep track of the
            // rightmost corner (bottom-right) of the last one seen.  If the next cell
            // is empty, we can use this instead of having to compute the top-right corner
            // of the empty cell.
            float savedRightSlope = -1;

            // Outer loop: walk across each column, stopping when we reach the visibility limit.
            for (int currentCol = startColumn; currentCol <= viewCeiling; currentCol++)
            {
                int xc = currentCol;

                // Inner loop: walk down the current column.  We start at the top, where X==Y.
                //
                // TODO: we waste time walking across the entire column when the view area
                //   is narrow.  Experiment with computing the possible range of cells from
                //   the slopes, and iterate over that instead.
                for (int yc = currentCol; yc >= 0; yc--)
                {
                    // Translate local coordinates to grid coordinates.  For the various octants
                    // we need to invert one or both values, or swap X for Y.
                    int gridX = sourceTile.TileX + xc * txfrm.xx + yc * txfrm.xy;
                    int gridY = sourceTile.TileY + xc * txfrm.yx + yc * txfrm.yy;

                    // Range-check the values.  This lets us avoid the slope division for blocks
                    // that are outside the grid.
                    //
                    // Note that, while we will stop at a solid column of blocks, we do always
                    // start at the top of the column, which may be outside the grid if we're (say)
                    // checking the first octant while positioned at the north edge of the map.
                    if (gridX < 0 || gridX >= tiledMap.Width || gridY < 0 || gridY >= tiledMap.Height)
                    {
                        continue;
                    }

                    // Compute slopes to corners of current block.  We use the top-left and
                    // bottom-right corners.  If we were iterating through a quadrant, rather than
                    // an octant, we'd need to flip the corners we used when we hit the midpoint.
                    //
                    // Note these values will be outside the view angles for the blocks at the
                    // ends -- left value > 1, right value < 0.
                    float leftBlockSlope = (yc + 0.5f) / (xc - 0.5f);
                    float rightBlockSlope = (yc - 0.5f) / (xc + 0.5f);

                    // Check to see if the block is outside our view area.  Note that we allow
                    // a "corner hit" to make the block visible.  Changing the tests to >= / <=
                    // will reduce the number of cells visible through a corner (from a 3-wide
                    // swath to a single diagonal line), and affect how far you can see past a block
                    // as you approach it.  This is mostly a matter of personal preference.
                    if (rightBlockSlope > leftViewSlope)
                    {
                        // Block is above the left edge of our view area; skip.
                        continue;
                    }
                    else if (leftBlockSlope < rightViewSlope)
                    {
                        // Block is below the right edge of our view area; we're done.
                        break;
                    }

                    // This cell is visible, given infinite vision range.  If it's also within
                    // our finite vision range, light it up.
                    //
                    // To avoid having a single lit cell poking out N/S/E/W, use a fractional
                    // viewRadius, e.g. 8.5.
                    //
                    // TODO: we're testing the middle of the cell for visibility.  If we tested
                    //  the bottom-left corner, we could say definitively that no part of the
                    //  cell is visible, and reduce the view area as if it were a wall.  This
                    //  could reduce iteration at the corners.
                    float distanceSquared = xc * xc + yc * yc;
                    if (distanceSquared <= viewRadiusSquared)
                    {
                        tiles[gridX, gridY].Obscured = false;
                        visibleTiles.Add(tiles[gridX, gridY]);
                    }

                    bool curBlocked = tiles[gridX, gridY].BlockSight;

                    if (prevWasBlocked)
                    {
                        if (curBlocked)
                        {
                            // Still traversing a column of walls.
                            savedRightSlope = rightBlockSlope;
                        }
                        else
                        {
                            // Found the end of the column of walls.  Set the left edge of our
                            // view area to the right corner of the last wall we saw.
                            prevWasBlocked = false;
                            leftViewSlope = savedRightSlope;
                        }
                    }
                    else
                    {
                        if (curBlocked)
                        {
                            // Found a wall.  Split the view area, recursively pursuing the
                            // part to the left.  The leftmost corner of the wall we just found
                            // becomes the right boundary of the view area.
                            //
                            // If this is the first block in the column, the slope of the top-left
                            // corner will be greater than the initial view slope (1.0).  Handle
                            // that here.
                            if (leftBlockSlope <= leftViewSlope)
                            {
                                CastLight(sourceTile, viewRadius, currentCol + 1, leftViewSlope, leftBlockSlope, txfrm);
                            }

                            // Once that's done, we keep searching to the right (down the column),
                            // looking for another opening.
                            prevWasBlocked = true;
                            savedRightSlope = rightBlockSlope;
                        }
                    }
                }

                // Open areas are handled recursively, with the function continuing to search to
                // the right (down the column).  If we reach the bottom of the column without
                // finding an open cell, then the area defined by our view area is completely
                // obstructed, and we can stop working.
                if (prevWasBlocked)
                {
                    break;
                }
            }
        }

        public TiledMap MapData { get => tiledMap; }
        public List<Tuple<TiledLayer, TiledGroup>> ObjectData { get; } = new List<Tuple<TiledLayer, TiledGroup>>();

        public int TileWidth { get => tiledMap.TileWidth; }
        public int TileHeight { get => tiledMap.TileHeight; }
        public int Width { get => tiledMap.Width * TileWidth; }
        public int Height { get => tiledMap.Height * TileHeight; }
        public int Columns { get => tiledMap.Width; }
        public int Rows { get => tiledMap.Height; }

        public bool RevealAll { set => revealAll = value; get => revealAll; }
    }
}
