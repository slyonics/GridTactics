using GridTactics.Models;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Reflection;
using System.Xml;

namespace GridTactics.SceneObjects.Widgets
{
    public class Label : Widget
    {
        private string text;
        public string Text { get => text; set { text = string.IsNullOrEmpty(value) ? "" : ExpandText(value); } }

        private Alignment TextAlignment { get; set; }

        public Label(Widget iParent, float widgetDepth)
            : base(iParent, widgetDepth)
        {
            TextAlignment = Alignment.Center;
        }

        public override void LoadAttributes(XmlNode xmlNode)
        {
            foreach (XmlAttribute xmlAttribute in xmlNode.Attributes)
            {
                switch (xmlAttribute.Name)
                {
                    default: ParseAttribute(xmlAttribute.Name, xmlAttribute.Value); break;
                }
            }

            if (bounds.Width == 0 && bounds.Height == 0 && bounds.X == 0 && bounds.Y == 0)
            {
                bounds = new Rectangle(0, 0, Main.Text.GetStringLength(Font, Text), Main.Text.GetStringHeight(Font));
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            Color drawColor = (parent.Enabled) ? Color : new Color(190, 190, 190, 255);

            switch (TextAlignment)
            {
                case Alignment.Left:
                    Main.Text.DrawText(spriteBatch, new Vector2(currentWindow.Left, currentWindow.Top) + Position, Font, Text, drawColor, Depth);
                    break;

                case Alignment.Center:
                    Main.Text.DrawCenteredText(spriteBatch, new Vector2(currentWindow.Center.X, currentWindow.Center.Y) + Position, Font, Text, drawColor, Depth);
                    break;

                case Alignment.Right:
                    Main.Text.DrawText(spriteBatch, new Vector2(currentWindow.Right - Main.Text.GetStringLength(Font, Text), currentWindow.Top) + Position, Font, Text, drawColor, Depth);
                    break;
            }
        }

        private string ExpandText(string text)
        {
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

            return text;
        }
    }
}
