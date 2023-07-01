using GridTactics.Main;
using GridTactics.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace GridTactics.SceneObjects.Widgets
{
    public class Image : Widget
    {
        public delegate void ImageDrawFunction(SpriteBatch spriteBatch, Rectangle bounds, Color color, float depth);

        private Texture2D icon;
        private string Icon { set { icon = AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), "Widgets_Icons_" + value)]; } }

        public AnimatedSprite Sprite { get; set; }

        private Texture2D picture;
        private Texture2D Picture { get => picture; set { picture = value; } }

        public ImageDrawFunction DrawDelegate { get; set; }

        private GameSprite GameSprite
        {
            set
            {
                picture = AssetCache.SPRITES[value];
            }
        }

        public float SpriteScale { get; set; } = 1.0f;

        public Image(Widget iParent, float widgetDepth)
            : base(iParent, widgetDepth)
        {

        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            Sprite?.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            Color drawColor = (parent.Enabled) ? Color : new Color(190, 190, 190, 255);

            if (icon != null)
            {
                spriteBatch.Draw(icon, new Vector2(currentWindow.Center.X - icon.Width / 2, currentWindow.Center.Y - icon.Height / 2) + Position, null, drawColor, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, Depth);
            }
            else if (Picture != null)
            {
                if (Alignment == Alignment.Bottom)
                    spriteBatch.Draw(Picture, new Rectangle(currentWindow.Left + (int)Position.X, (int)Position.Y + currentWindow.Top, currentWindow.Width, currentWindow.Height), null, drawColor, 0.0f, Vector2.Zero, SpriteEffects.None, Depth);
                // spriteBatch.Draw(picture, new Rectangle(currentWindow.Left + (int)Position.X, -currentWindow.Height + (int)Position.Y + parent.InnerBounds.Height / 2 - parent.InnerMargin.Y * CrossPlatformGame.Scale, currentWindow.Width, currentWindow.Height), null, color, 0.0f, Vector2.Zero, SpriteEffects.None, depth - 0.0001f);
                else spriteBatch.Draw(Picture, new Rectangle(currentWindow.X + (int)Position.X, currentWindow.Y + (int)Position.Y, currentWindow.Width, currentWindow.Height), null, drawColor, 0.0f, Vector2.Zero, SpriteEffects.None, Depth);
            }
            else if (Sprite != null)
            {
                Sprite.Scale = new Vector2(SpriteScale);

                switch (Alignment)
                {
                    case Alignment.BottomRight:
                        Sprite?.Draw(spriteBatch, new Vector2(currentWindow.Center.X - (Sprite.SpriteBounds().Width * SpriteScale) / 2, currentWindow.Center.Y) + Position, null, Depth);
                        break;

                    case Alignment.Center:
                        Sprite?.Draw(spriteBatch, new Vector2(currentWindow.Center.X, currentWindow.Center.Y + bounds.Y) + Position, null, Depth);
                        break;

                    case Alignment.Bottom:
                        Sprite?.Draw(spriteBatch, new Vector2(currentWindow.Center.X, currentWindow.Bottom - parent.InnerMargin.Height) + Position, null, Depth);
                        break;

                    default:
                        Sprite?.Draw(spriteBatch, new Vector2(currentWindow.Center.X - (Sprite.SpriteBounds().Width * SpriteScale) / 2, currentWindow.Center.Y - (Sprite.SpriteBounds().Height * SpriteScale) / 2) + Position, null, Depth);
                        break;
                }
            }
            else if (DrawDelegate != null)
            {
                DrawDelegate.Invoke(spriteBatch, currentWindow, Color.White, Depth);
            }
        }

        public override void EndLeftClick(Vector2 mouseStart, Vector2 mouseEnd, Widget otherWidget)
        {
            base.EndLeftClick(mouseStart, mouseEnd, otherWidget);

            if (otherWidget == this)
            {
                GetParent<ViewModel>().LeftClickChild(mouseStart, mouseEnd, this, otherWidget);
            }
        }
    }
}
