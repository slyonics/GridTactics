using GridTactics.Main;
using GridTactics.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GridTactics.SceneObjects.Widgets
{
    public class Tooltip : Widget
    {
        private const int TOOLTIP_MARGIN_WIDTH = 3;
        private const int TOOLTIP_MARGIN_HEIGHT = 2;

        private string text;
        private NinePatch tooltipFrame;

        public Tooltip(Vector2 mouseOverPosition, string tooltipText)
        {
            Color = Graphics.ParseHexcode("#E0BFA2FF");

            text = tooltipText;
            Font = GameFont.Tooltip;
            tooltipFrame = new NinePatch("Label", 0.05f);

            int startIndex = text.IndexOf('{');
            int endIndex = text.IndexOf('}');

            while (startIndex != -1 && endIndex > startIndex)
            {
                string originalToken = text.Substring(startIndex, endIndex - startIndex + 1);
                PropertyInfo propertyInfo = GameProfile.PlayerProfile.GetType().GetProperty(originalToken.Substring(1, originalToken.Length - 2));
                string newToken = (propertyInfo.GetValue(GameProfile.PlayerProfile) as ModelProperty<string>).Value;

                text = text.Replace(originalToken, newToken.ToString());

                startIndex = text.IndexOf('{');
                endIndex = text.IndexOf('}');
            }

            int width = Text.GetStringLength(Font, text) + TOOLTIP_MARGIN_WIDTH * 2;
            int height = Text.GetStringHeight(Font) + TOOLTIP_MARGIN_HEIGHT * 2;

            currentWindow = new Rectangle((int)mouseOverPosition.X - TOOLTIP_MARGIN_WIDTH + TOOLTIP_MARGIN_WIDTH, (int)mouseOverPosition.Y - Text.GetStringHeight(Font) - TOOLTIP_MARGIN_HEIGHT + height / 2, width, height);
            if (currentWindow.Right > CrossPlatformGame.ScreenWidth) currentWindow.X = (int)mouseOverPosition.X - TOOLTIP_MARGIN_WIDTH + TOOLTIP_MARGIN_WIDTH - Math.Max(Text.GetStringLength(Font, text) + TOOLTIP_MARGIN_WIDTH * 2, tooltipFrame.FrameWidth * 3);
            if (currentWindow.Bottom > CrossPlatformGame.ScreenHeight) currentWindow.Y -= currentWindow.Bottom - CrossPlatformGame.ScreenHeight;

            tooltipFrame.Bounds = currentWindow;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            Vector2 mouseOverPosition = Input.MousePosition;
            int width = Math.Max(Text.GetStringLength(Font, text) + TOOLTIP_MARGIN_WIDTH * 2, tooltipFrame.FrameWidth * 2);
            int height = Math.Max(Text.GetStringHeight(Font) + TOOLTIP_MARGIN_HEIGHT * 2, tooltipFrame.FrameHeight * 2);

            currentWindow.X = (int)mouseOverPosition.X - TOOLTIP_MARGIN_WIDTH + TOOLTIP_MARGIN_WIDTH;
            currentWindow.Y = (int)mouseOverPosition.Y - Text.GetStringHeight(Font);

            if (currentWindow.Right > CrossPlatformGame.ScreenWidth) currentWindow.X = (int)mouseOverPosition.X - TOOLTIP_MARGIN_WIDTH + TOOLTIP_MARGIN_WIDTH - Math.Max(Text.GetStringLength(Font, text) + TOOLTIP_MARGIN_WIDTH * 2, tooltipFrame.FrameWidth * 3);
            if (currentWindow.Bottom > CrossPlatformGame.ScreenHeight) currentWindow.Y -= currentWindow.Bottom - CrossPlatformGame.ScreenHeight;

            tooltipFrame.Bounds = currentWindow;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            tooltipFrame.Draw(spriteBatch, Vector2.Zero);
            Text.DrawCenteredText(spriteBatch, new Vector2(currentWindow.Center.X + 1, currentWindow.Center.Y - 1), Font, text, 0.04f, Color);
        }
    }
}
