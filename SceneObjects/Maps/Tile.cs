using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiledCS;

namespace GridTactics.SceneObjects.Maps
{
    public class Tile
    {
        private class TileSprite
        {
            public Rectangle source;
            public Texture2D atlas;
            public Color color = Color.White;
            public int height;
            public Vector2 offset;

            public Rectangle[] anims;
            public int animationTime;
            public int animFrame;
            public int animTimeLeft;
        }

        private Tilemap parentMap;
        private Vector2 position;
        private Vector2 center;
        private List<TileSprite> backgroundSprites = new List<TileSprite>();
        private Dictionary<int, List<TileSprite>> entitySprites = new Dictionary<int, List<TileSprite>>();

        private List<Tile> neighborList = new List<Tile>();

        private bool blockSight;
        private bool obscured;
        private float visibilityInterval;
        private Color visibilityColor;

        public Tile(Tilemap iTileMap, int iTileX, int iTileY)
        {
            parentMap = iTileMap;
            TileX = iTileX;
            TileY = iTileY;

            position = new Vector2(TileX * parentMap.TileWidth, TileY * parentMap.TileHeight);
            center = position + new Vector2(parentMap.TileWidth / 2, parentMap.TileHeight / 2);
        }

        public void Update(GameTime gameTime)
        {
            foreach (TileSprite backgroundSprite in backgroundSprites)
            {
                if (backgroundSprite.anims != null)
                {
                    backgroundSprite.animTimeLeft -= gameTime.ElapsedGameTime.Milliseconds;
                    while (backgroundSprite.animTimeLeft < 0)
                    {
                        backgroundSprite.animTimeLeft += backgroundSprite.animationTime;
                        backgroundSprite.animFrame++;
                        if (backgroundSprite.animFrame >= backgroundSprite.anims.Length) backgroundSprite.animFrame = 0;
                        backgroundSprite.source = backgroundSprite.anims[backgroundSprite.animFrame];
                    }
                }
            }

            UpdateVisibility(gameTime);
        }

        public void DrawBackground(SpriteBatch spriteBatch, Camera camera)
        {
            float depth = 0.9f;
            foreach (TileSprite backgroundSprite in backgroundSprites)
            {
                //spriteBatch.Draw(backgroundSprite.atlas, position + backgroundSprite.offset - camera.Position - new Vector2(camera.CenteringOffsetX, camera.CenteringOffsetY), backgroundSprite.source, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, depth);
                spriteBatch.Draw(backgroundSprite.atlas, position + backgroundSprite.offset - camera.Position - new Vector2(camera.CenteringOffsetX, camera.CenteringOffsetY), backgroundSprite.source, visibilityColor, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, depth);
                depth -= 0.001f;
            }
        }

        public void Draw(SpriteBatch spriteBatch, Camera camera)
        {
            foreach (KeyValuePair<int, List<TileSprite>> tileSprites in entitySprites)
            {
                float depth = camera.GetDepth(position.Y + parentMap.TileHeight / 2 + tileSprites.Key * parentMap.TileHeight);
                if (depth <= 0) depth = 0.0f;

                foreach (TileSprite tileSprite in tileSprites.Value)
                {
                    //spriteBatch.Draw(tileSprite.atlas, position + tileSprite.offset, tileSprite.source, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, depth);
                    spriteBatch.Draw(tileSprite.atlas, position + tileSprite.offset, tileSprite.source, visibilityColor, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, depth);
                    depth -= 0.0001f;
                }
            }
        }

        public void ApplyBackgroundTile(TiledTile tiledTile, TiledLayer tiledLayer, Rectangle source, Texture2D atlas)
        {
            TileSprite tileSprite = new TileSprite()
            {
                source = source,
                atlas = atlas
            };

            if (tiledTile != null)
            {
                foreach (TiledObject tiledObject in tiledTile.objects) ColliderList.Add(new Rectangle((int)(tiledObject.x + position.X + tiledLayer.offsetX), (int)(tiledObject.y + position.Y + tiledLayer.offsetY), (int)tiledObject.width, (int)tiledObject.height));
                foreach (TiledProperty tiledProperty in tiledTile.properties)
                {
                    switch (tiledProperty.name)
                    {
                        case "Encounter": Encounter = true; break;
                        case "BlockSight": blockSight = true; break;
                        case "Color": var color = System.Drawing.ColorTranslator.FromHtml(tiledProperty.value); tileSprite.color = new Color(color.R, color.G, color.B, color.A); break;
                        case "AnimationTime": tileSprite.animationTime = int.Parse(tiledProperty.value); break;
                        case "AnimationOffsets":
                            {
                                string[] tokens = tiledProperty.value.Split(',');
                                tileSprite.anims = new Rectangle[tokens.Length];
                                for (int i = 0; i < tokens.Length; i++)
                                {
                                    tileSprite.anims[i] = new Rectangle(source.X + source.Width * int.Parse(tokens[i]), source.Y, source.Width, source.Height);
                                }
                            }
                            break;
                    }
                }
            }

            tileSprite.offset = new Vector2(tiledLayer.offsetX, tiledLayer.offsetY);

            if (tileSprite.anims != null)
            {
                tileSprite.animTimeLeft = tileSprite.animationTime;
            }

            backgroundSprites.Add(tileSprite);
        }

        public void ApplyEntityTile(TiledTile tiledTile, TiledLayer tiledLayer, Rectangle source, Texture2D atlas, int height)
        {
            List<TileSprite> tileSprites;
            if (!entitySprites.TryGetValue(height, out tileSprites))
            {
                tileSprites = new List<TileSprite>();
                entitySprites.Add(height, tileSprites);
            }

            TileSprite tileSprite = new TileSprite()
            {
                source = source,
                atlas = atlas,
                height = height
            };

            if (tiledTile != null)
            {
                foreach (TiledObject tiledObject in tiledTile.objects) ColliderList.Add(new Rectangle((int)(tiledObject.x + position.X + tiledLayer.offsetX), (int)(tiledObject.y + position.Y + tiledLayer.offsetY), (int)tiledObject.width, (int)tiledObject.height));
                foreach (TiledProperty tiledProperty in tiledTile.properties)
                {
                    switch (tiledProperty.name)
                    {
                        case "BlockSight": blockSight = true; break;
                        case "Color": var color = System.Drawing.ColorTranslator.FromHtml(tiledProperty.value); tileSprite.color = new Color(color.R, color.G, color.B, color.A); break;
                        case "Encounter": Encounter = true; break;
                    }
                }
            }

            tileSprite.offset = new Vector2(tiledLayer.offsetX, tiledLayer.offsetY);

            tileSprites.Add(tileSprite);
        }

        public void AssignNeighbors()
        {
            if (TileX > 0) neighborList.Add(parentMap.GetTile(TileX - 1, TileY));
            if (TileY > 0) neighborList.Add(parentMap.GetTile(TileX, TileY - 1));
            if (TileY < parentMap.Columns - 1) neighborList.Add(parentMap.GetTile(TileX, TileY + 1));
            if (TileX < parentMap.Rows - 1) neighborList.Add(parentMap.GetTile(TileX + 1, TileY));

            if (TileX > 0 && TileY > 0) neighborList.Add(parentMap.GetTile(TileX - 1, TileY - 1));
            if (TileX > 0 && TileY < parentMap.Columns - 1) neighborList.Add(parentMap.GetTile(TileX - 1, TileY + 1));
            if (TileX < parentMap.Rows - 1 && TileY > 0) neighborList.Add(parentMap.GetTile(TileX + 1, TileY - 1));
            if (TileX < parentMap.Rows - 1 && TileY < parentMap.Columns - 1) neighborList.Add(parentMap.GetTile(TileX + 1, TileY + 1));
        }

        public void UpdateVisibility(GameTime gameTime)
        {
            if (obscured) visibilityInterval = Math.Max(0.0f, visibilityInterval - gameTime.ElapsedGameTime.Milliseconds / 200.0f);
            else visibilityInterval = Math.Min(1.0f, visibilityInterval + gameTime.ElapsedGameTime.Milliseconds / 200.0f);
            visibilityColor = Color.Lerp(Color.Black, Color.White, visibilityInterval);
        }

        public void UpdateVisibility()
        {
            if (obscured) visibilityColor = Color.Black;
            else
            {
                visibilityColor = Color.White;
                visibilityInterval = 1.0f;
            }
        }

        public int TileX { get; private set; }
        public int TileY { get; private set; }
        public Vector2 Center { get => center; }
        public Vector2 Bottom { get => center + new Vector2(0, parentMap.TileHeight / 2); }

        public List<Tile> NeighborList { get => neighborList; }
        public List<Rectangle> ColliderList { get; private set; } = new List<Rectangle>();


        public bool Blocked { get => ColliderList.Count > 0 || Occupants.Count > 0; }
        public List<Actor> Occupants { get; private set; } = new List<Actor>();

        public bool BlockSight { set => blockSight = value; get => blockSight; }
        public bool Obscured { set => obscured = value; get => obscured; }
        public Color VisibilityColor { get => visibilityColor; }
        public Color HostVisibilityColor
        {
            get
            {
                return Color.Lerp(Color.Black, Color.White, visibilityInterval);
            }
        }

        public bool Encounter { get; private set; }
    }
}
