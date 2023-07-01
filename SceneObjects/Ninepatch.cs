using GridTactics.Main;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTactics.SceneObjects
{
    public class NinePatch
    {
        private static readonly List<string> WIDGET_FRAMES = new List<string>()
        {
            "Windows", "Buttons", "Textplate", "Gauges"
        };

        public static readonly Dictionary<Texture2D, int> FRAME_WIDTH = new Dictionary<Texture2D, int>();
        public static readonly Dictionary<Texture2D, int> FRAME_HEIGHT = new Dictionary<Texture2D, int>();
        public static readonly Dictionary<Texture2D, Rectangle[]> FRAME_SOURCES = new Dictionary<Texture2D, Rectangle[]>();
        public static readonly Dictionary<string, Texture2D> FRAME_SPRITES = new Dictionary<string, Texture2D>();

        private Rectangle bounds;

        private int frameWidth;
        private int frameHeight;
        public float FrameDepth { get; set; }
        private Rectangle[] frameSource;
        private Texture2D frameSprite;
        private Color frameColor = Color.White;

        private bool flatFrame;

        public NinePatch(string spriteName, float iFrameDepth, bool flatsize = false)
        {
            SetSprite(spriteName);
            FrameDepth = iFrameDepth;

            flatFrame = flatsize;
        }

        public NinePatch(Texture2D spriteName, float iFrameDepth, bool flatsize = false)
        {
            SetSprite(spriteName);
            FrameDepth = iFrameDepth;

            flatFrame = flatsize;
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 offset)
        {
            int x = bounds.X + (int)offset.X;
            int y = bounds.Y + (int)offset.Y;
            int width = bounds.Width;
            int height = bounds.Height;

            Vector2 position = new Vector2(x, y);

            if (flatFrame)
            {
                spriteBatch.Draw(frameSprite, new Rectangle(x, y, width, height), null, frameColor, 0.0f, Vector2.Zero, SpriteEffects.None, FrameDepth);
                return;
            }

            if (frameSprite == null) return;

            if (width < frameWidth * 2 && height < frameHeight * 2)
            {
                int widthOffset = (width % 2 == 1) ? 1 : 0;
                int heightOffset = (height % 2 == 1) ? 1 : 0;

                Rectangle topLeft = new Rectangle(0, 0, width / 2 + widthOffset, height / 2 + heightOffset);
                Rectangle topRight = new Rectangle(frameWidth * 3 - width / 2, 0, width / 2, height / 2 + heightOffset);
                Rectangle bottomLeft = new Rectangle(0, frameHeight * 3 - height / 2, width / 2 + widthOffset, height / 2);
                Rectangle bottomRight = new Rectangle(frameWidth * 3 - width / 2, frameHeight * 3 - height / 2, width / 2, height / 2);

                spriteBatch.Draw(frameSprite, position, topLeft, frameColor, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, FrameDepth);
                spriteBatch.Draw(frameSprite, position + new Vector2(topLeft.Width, 0), topRight, frameColor, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, FrameDepth);
                spriteBatch.Draw(frameSprite, position + new Vector2(0, topLeft.Height), bottomLeft, frameColor, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, FrameDepth);
                spriteBatch.Draw(frameSprite, position + new Vector2(topLeft.Width, topLeft.Height), bottomRight, frameColor, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, FrameDepth);
            }
            else if (width < frameWidth * 2 && height >= frameHeight * 2)
            {
                int widthOffset = (width % 2 == 1) ? 1 : 0;

                Rectangle lefttopSource = new Rectangle(0, 0, width / 2 + widthOffset, frameHeight);
                Rectangle leftcenterSource = new Rectangle(0, frameHeight, width / 2 + widthOffset, frameHeight);
                Rectangle leftbottomSource = new Rectangle(0, frameHeight * 2, width / 2 + widthOffset, frameHeight);
                Rectangle righttopSource = new Rectangle(frameWidth * 3 - width / 2, 0, width / 2 + widthOffset, frameHeight);
                Rectangle rightcenterSource = new Rectangle(frameWidth * 3 - width / 2, frameHeight, width / 2 + widthOffset, frameHeight);
                Rectangle rightbottomSource = new Rectangle(frameWidth * 3 - width / 2, frameHeight * 2, width / 2 + widthOffset, frameHeight);

                spriteBatch.Draw(frameSprite, position, lefttopSource, frameColor, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, FrameDepth);
                spriteBatch.Draw(frameSprite, position + new Vector2(width / 2 + widthOffset, 0), righttopSource, frameColor, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, FrameDepth);
                position.Y += frameHeight;
                spriteBatch.Draw(frameSprite, position, leftcenterSource, frameColor, 0.0f, Vector2.Zero, new Vector2(1.0f, (float)(height - frameHeight * 2) / frameHeight), SpriteEffects.None, FrameDepth);
                spriteBatch.Draw(frameSprite, position + new Vector2(width / 2 + widthOffset, 0), rightcenterSource, frameColor, 0.0f, Vector2.Zero, new Vector2(1.0f, (float)(height - frameHeight * 2) / frameHeight), SpriteEffects.None, FrameDepth);
                position.Y += height - frameHeight * 2;
                spriteBatch.Draw(frameSprite, position, leftbottomSource, frameColor, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, FrameDepth);
                spriteBatch.Draw(frameSprite, position + new Vector2(width / 2 + widthOffset, 0), rightbottomSource, frameColor, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, FrameDepth);
            }
            else if (width >= frameWidth * 2 && height < frameHeight * 2)
            {
                int heightOffset = (height % 2 == 1) ? 1 : 0;

                Rectangle topleftSource = new Rectangle(0, 0, frameWidth, height / 2 + heightOffset);
                Rectangle topcenterSource = new Rectangle(frameWidth, 0, frameWidth, height / 2 + heightOffset);
                Rectangle toprightSource = new Rectangle(frameWidth * 2, 0, frameWidth, height / 2 + heightOffset);
                Rectangle bottomleftSource = new Rectangle(0, frameHeight * 3 - height / 2, frameWidth, height / 2 + heightOffset);
                Rectangle bottomcenterSource = new Rectangle(frameWidth, frameHeight * 3 - height / 2, frameWidth, height / 2 + heightOffset);
                Rectangle bottomrightSource = new Rectangle(frameWidth * 2, frameHeight * 3 - height / 2, frameWidth, height / 2 + heightOffset);

                spriteBatch.Draw(frameSprite, position, topleftSource, frameColor, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, FrameDepth);
                spriteBatch.Draw(frameSprite, position + new Vector2(0, height / 2 + heightOffset), bottomleftSource, frameColor, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, FrameDepth);
                position.X += frameWidth;
                spriteBatch.Draw(frameSprite, position, topcenterSource, frameColor, 0.0f, Vector2.Zero, new Vector2((float)(width - frameWidth * 2) / frameWidth, 1.0f), SpriteEffects.None, FrameDepth);
                spriteBatch.Draw(frameSprite, position + new Vector2(0, height / 2 + heightOffset), bottomcenterSource, frameColor, 0.0f, Vector2.Zero, new Vector2((float)(width - frameWidth * 2) / frameWidth, 1.0f), SpriteEffects.None, FrameDepth);
                position.X += width - frameWidth * 2;
                spriteBatch.Draw(frameSprite, position, toprightSource, frameColor, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, FrameDepth);
                spriteBatch.Draw(frameSprite, position + new Vector2(0, height / 2 + heightOffset), bottomrightSource, frameColor, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, FrameDepth);
            }
            else
            {
                spriteBatch.Draw(frameSprite, new Rectangle(x + frameWidth, y + frameHeight, width - frameWidth * 2, height - frameHeight * 2), frameSource[4], frameColor, 0.0f, Vector2.Zero, SpriteEffects.None, FrameDepth);

                spriteBatch.Draw(frameSprite, position, frameSource[0], frameColor, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, FrameDepth);
                position.X += frameWidth;
                spriteBatch.Draw(frameSprite, position, frameSource[1], frameColor, 0.0f, Vector2.Zero, new Vector2((float)(width - frameWidth * 2) / frameWidth, 1.0f), SpriteEffects.None, FrameDepth);
                position.X += width - frameWidth * 2;
                spriteBatch.Draw(frameSprite, position, frameSource[2], frameColor, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, FrameDepth);

                position.Y += frameHeight;
                spriteBatch.Draw(frameSprite, position, frameSource[5], frameColor, 0.0f, Vector2.Zero, new Vector2(1.0f, (float)(height - frameHeight * 2) / frameHeight), SpriteEffects.None, FrameDepth);
                position.X = x;
                spriteBatch.Draw(frameSprite, position, frameSource[3], frameColor, 0.0f, Vector2.Zero, new Vector2(1.0f, (float)(height - frameHeight * 2) / frameHeight), SpriteEffects.None, FrameDepth);
                position.Y += height - frameHeight * 2;

                spriteBatch.Draw(frameSprite, position, frameSource[6], frameColor, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, FrameDepth);
                position.X += frameWidth;
                spriteBatch.Draw(frameSprite, position, frameSource[7], frameColor, 0.0f, Vector2.Zero, new Vector2((float)(width - frameWidth * 2) / frameWidth, 1.0f), SpriteEffects.None, FrameDepth);
                position.X += width - frameWidth * 2;
                spriteBatch.Draw(frameSprite, position, frameSource[8], frameColor, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, FrameDepth);
            }
        }

        public void SetSprite(string frameName)
        {
            if (frameName == null) return;

            frameSprite = AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), "Widgets_" + frameName)];
            frameSource = new Rectangle[9];
            frameWidth = frameSprite.Width / 3;
            frameHeight = frameSprite.Height / 3;
            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    frameSource[x + y * 3] = new Rectangle(x * frameWidth, y * frameHeight, frameWidth, frameHeight);
                }
            }
            frameSource[4] = new Rectangle(frameWidth, frameHeight, frameWidth, frameHeight);
        }

        public void SetSprite(Texture2D frameName)
        {
            if (frameName == null) return;

            frameSprite = frameName;
            frameSource = new Rectangle[9];
            frameWidth = frameSprite.Width / 3;
            frameHeight = frameSprite.Height / 3;
            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    frameSource[x + y * 3] = new Rectangle(x * frameWidth, y * frameHeight, frameWidth, frameHeight);
                }
            }
            frameSource[4] = new Rectangle(frameWidth, frameHeight, frameWidth, frameHeight);
        }

        public Rectangle Bounds
        {
            get => bounds;
            set => bounds = value;
        }

        public Color FrameColor { get => frameColor; set => frameColor = value; }

        public Texture2D Sprite { get => frameSprite; }

        public int FrameWidth { get => frameWidth; }
        public int FrameHeight { get => frameHeight; }
    }
}
