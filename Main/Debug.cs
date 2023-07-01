using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTactics.Main
{
    public static class Debug
    {
        private static Texture2D lineSprite;

        public static void Initialize(GraphicsDevice graphicsDevice)
        {
            lineSprite = new Texture2D(graphicsDevice, 1, 1);
            Color[] colorData = new Color[1] { Color.White };
            lineSprite.SetData<Color>(colorData);
        }

        public static void DrawLine(SpriteBatch spriteBatch, Vector2 lineStart, Vector2 lineEnd)
        {
            float rotation = (float)Math.Atan2(lineEnd.Y - lineStart.Y, lineEnd.X - lineStart.X);
            Vector2 scale = new Vector2(Vector2.Distance(lineStart, lineEnd), 1.0f);

            spriteBatch.Draw(lineSprite, lineStart, null, Color.Red, rotation, Vector2.Zero, scale, SpriteEffects.None, 0.0f);
        }

        public static void DrawBox(SpriteBatch spriteBatch, Rectangle rectangle)
        {
            spriteBatch.Draw(lineSprite, new Rectangle(rectangle.Left, rectangle.Top, rectangle.Right - rectangle.Left, 1), Color.Red);
            spriteBatch.Draw(lineSprite, new Rectangle(rectangle.Right, rectangle.Top, 1, rectangle.Bottom - rectangle.Top), Color.Red);
            spriteBatch.Draw(lineSprite, new Rectangle(rectangle.Left, rectangle.Bottom, rectangle.Right - rectangle.Left + 1, 1), Color.Red);
            spriteBatch.Draw(lineSprite, new Rectangle(rectangle.Left, rectangle.Top, 1, rectangle.Bottom - rectangle.Top), Color.Red);
        }
    }
}
