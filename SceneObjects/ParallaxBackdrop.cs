using GridTactics.Main;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTactics.SceneObjects
{
    public enum GameBackdrop
    {
        Canyon
    }

    public class ParallaxBackdrop
    {
        public class Layer
        {
            public Texture2D background;
            public Vector2 offset;
            public float speed;

            public Layer(Texture2D initialBackground, float initialSpeed)
            {
                background = initialBackground;
                offset = new Vector2();
                speed = initialSpeed;
            }
        }

        private const float STARTING_DEPTH = 0.95f;

        private static Dictionary<GameBackdrop, Texture2D[]> backgroundList = new Dictionary<GameBackdrop, Texture2D[]>();

        private List<Layer> layerList = new List<Layer>();
        private int backdropHeight;
        private Color color = Color.White;

        public ParallaxBackdrop(string backdropName, float[] parallaxSpeeds)
        {
            backdropHeight = 0;
            layerList = new List<Layer>();
            for (int i = 0; i < parallaxSpeeds.Length; i++)
            {
                Texture2D backgroundSprite = AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), "Background_" + backdropName + "_" + backdropName + i)];
                layerList.Add(new Layer(backgroundSprite, parallaxSpeeds[i]));
                if (backgroundSprite.Height > backdropHeight) backdropHeight = backgroundSprite.Height;
            }
        }

        public void Update(GameTime gameTime, Camera camera)
        {
            foreach (Layer layer in layerList)
            {
                layer.offset.X = -((Math.Max(camera.Position.X, 0) * layer.speed) % layer.background.Width);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            float depthOffset = 0.0f;

            foreach (Layer layer in layerList)
            {
                if (layer.speed > 0.001f)
                {
                    for (int i = 0; i < Math.Max(2, CrossPlatformGame.ScreenWidth / layer.background.Width); i++)
                    {
                        spriteBatch.Draw(layer.background, layer.offset + new Vector2(layer.background.Width * i, backdropHeight - layer.background.Height), null, color, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, STARTING_DEPTH - depthOffset);
                    }
                }
                else
                {
                    spriteBatch.Draw(layer.background, layer.offset + new Vector2(0, backdropHeight - layer.background.Height), null, color, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, STARTING_DEPTH + depthOffset);
                }

                depthOffset += 0.001f;
            }
        }

        public Color Color { set => color = value; }
    }
}
