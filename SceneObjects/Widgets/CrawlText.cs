using GridTactics.Models;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;

namespace GridTactics.SceneObjects.Widgets
{
    public class CrawlText : Widget
    {
        public static readonly Color DEFAULT_TEXT_COLOR = new Color(142, 69, 95);

        private class TextElement
        {
            public int line;
            public int offset;
            public Color color = DEFAULT_TEXT_COLOR;
            public StringBuilder text = new StringBuilder();
            public Texture2D icon = null;
        }

        private const int TEXT_QUEUE_COOLDOWN = 10;
        private const int TALK_COOLDOWN = 180;

        private string text;
        public string Text { get => text; set { text = value; AddLines(text); } }
        private List<TextElement> textQueue = new List<TextElement>();
        private List<TextElement> textLines = new List<TextElement>();

        private int textQueueTimer = 0;
        private int maxTextLength = 0;
        private int crawlFactor = 1;

        private int talkTimer = 0;
        public GameSound VoiceSound { get; set; } = GameSound.None;

        public CrawlText(Widget iParent, float widgetDepth)
            : base(iParent, widgetDepth)
        {

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
        }

        public override void ApplyAlignment()
        {
            base.ApplyAlignment();

            maxTextLength = currentWindow.Width;

            if (!String.IsNullOrEmpty(Text))
            {
                AddLines(Text);
            }
        }

        private void AddLines(string text)
        {
            if (string.IsNullOrEmpty(text)) talkTimer = 10000;
            else talkTimer = 0;

            textLines.Clear();
            textQueueTimer = 0;

            textQueue = GetTextLines(ExpandText(text), maxTextLength);
            textLines.Add(new TextElement());
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (!Transitioning)
            {
                UpdateText(gameTime);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            if (!Transitioning)
            {
                foreach (TextElement textElement in textLines)
                {
                    if (textElement.icon != null)
                    {
                        spriteBatch.Draw(textElement.icon, base.Position + new Vector2(currentWindow.X + textElement.offset - 4, currentWindow.Y + 2 + textElement.line * 10), null, textElement.color, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.05f);
                    }
                    else Main.Text.DrawText(spriteBatch, base.Position + new Vector2(currentWindow.X + textElement.offset, currentWindow.Y), Font, textElement.text.ToString(), textElement.color, textElement.line);
                }
            }
        }

        private List<TextElement> ExpandText(string text)
        {
            int startIndex = text.IndexOf('{');
            int endIndex = text.IndexOf('}');

            while (startIndex != -1 && endIndex > startIndex)
            {
                string originalToken = text.Substring(startIndex, endIndex - startIndex + 1);

                string newToken;
                if (originalToken == "{RngSeed}")
                {
                    newToken = GameProfile.PlayerProfile.PlayerName.Value.Sum(x => x).ToString();
                }
                else if (originalToken == "{PlayerNameEx}")
                {
                    PropertyInfo propertyInfo = GameProfile.PlayerProfile.GetType().GetProperty(originalToken.Substring(1, originalToken.Length - 2));
                    newToken = (propertyInfo.GetValue(GameProfile.PlayerProfile) as ModelProperty<string>).Value;
                    newToken += '!';
                }
                else
                {
                    PropertyInfo propertyInfo = GameProfile.PlayerProfile.GetType().GetProperty(originalToken.Substring(1, originalToken.Length - 2));
                    newToken = (propertyInfo.GetValue(GameProfile.PlayerProfile) as ModelProperty<string>).Value;
                }

                text = text.Replace(originalToken, newToken.ToString());

                startIndex = text.IndexOf('{');
                endIndex = text.IndexOf('}');
            }

            List<TextElement> result = new List<TextElement>();
            string[] tokens = text.Split(' ');
            foreach (string token in tokens)
            {
                TextElement textElement = new TextElement() { color = DEFAULT_TEXT_COLOR };
                textElement.text.Append(token);
                result.Add(textElement);
            }

            return result;
        }

        private void UpdateText(GameTime gameTime)
        {
            if (!ReadyToProceed)
            {
                textQueueTimer -= gameTime.ElapsedGameTime.Milliseconds * crawlFactor;

                while (textQueueTimer <= 0 && !ReadyToProceed)
                {
                    textQueueTimer += TEXT_QUEUE_COOLDOWN;

                    talkTimer -= gameTime.ElapsedGameTime.Milliseconds;
                    if (talkTimer <= 0)
                    {
                        if (VoiceSound != GameSound.None) Audio.PlaySound(VoiceSound);
                        talkTimer += TALK_COOLDOWN;
                    }

                    AdvanceCharacter();
                }
            }
        }

        public void FinishText()
        {
            while (!ReadyToProceed)
            {
                AdvanceCharacter();
            }
        }

        private void AdvanceCharacter()
        {
            TextElement queueElement = textQueue.First();
            TextElement textElement = textLines.Last();

            if (queueElement.text.Length == 0)
            {
                textQueue.Remove(queueElement);
                queueElement = textQueue.FirstOrDefault();

                textElement = new TextElement() { icon = queueElement.icon, line = queueElement.line, color = queueElement.color, offset = queueElement.offset };
                textLines.Add(textElement);

                if (queueElement.icon != null)
                {
                    textElement = new TextElement() { line = queueElement.line, color = queueElement.color, offset = queueElement.offset + 8 };
                    textLines.Add(textElement);
                }
            }

            if (queueElement.text.Length > 0)
            {
                textElement.text.Append(queueElement.text[0]);
                queueElement.text.Remove(0, 1);
            }
        }

        private List<TextElement> GetTextLines(List<TextElement> textElements, int windowWidth)
        {
            List<TextElement> textLines = new List<TextElement>();
            textLines.Add(new TextElement());

            int currentLine = 0;
            int currentLength = 0;
            Color currentColor = DEFAULT_TEXT_COLOR;

            foreach (TextElement textElement in textElements)
            {
                if (textElement.text.Length > 0 && textElement.text[0] == '#')
                {
                    currentColor = Graphics.ParseHexcode(textElement.text.ToString());
                    textLines.Add(new TextElement() { line = currentLine, color = currentColor, offset = currentLength });
                    continue;
                }
                else if (textElement.text.Length > 0 && textElement.text[0] == '@')
                {
                    if (currentLength + 10 > windowWidth)
                    {
                        currentLine++;
                        currentLength = 0;
                    }

                    string iconSpriteName = "Widgets_Icons_" + textElement.text.ToString().Substring(1);
                    GameSprite gameSprite = (GameSprite)Enum.Parse(typeof(GameSprite), iconSpriteName);
                    textLines.Add(new TextElement() { line = currentLine, offset = currentLength + 4, color = currentColor, icon = AssetCache.SPRITES[gameSprite] });
                    currentLength += 10 + Main.Text.GetStringLength(Font, " ");
                    continue;
                }
                else if (textElement.text.ToString() == "[n]")
                {
                    currentLine++;
                    currentLength = 0;
                    textLines.Add(new TextElement() { line = currentLine, color = currentColor });
                    continue;
                }

                int tokenLength = Main.Text.GetStringLength(Font, textElement.text.ToString());
                if (currentLength + tokenLength > windowWidth)
                {
                    currentLine++;
                    currentLength = 0;
                    textLines.Add(new TextElement() { line = currentLine, color = currentColor });
                }

                StringBuilder lastElement = textLines.LastOrDefault(x => x.line == currentLine)?.text;
                lastElement?.Append(textElement.text + " ");

                currentLength += tokenLength + Main.Text.GetStringLength(Font, " ");
            }

            return textLines;
        }

        public override void EndLeftClick(Vector2 mouseStart, Vector2 mouseEnd, Widget otherWidget)
        {
            base.EndLeftClick(mouseStart, mouseEnd, otherWidget);

            if (otherWidget == this)
            {
                GetParent<ViewModel>().LeftClickChild(mouseStart, mouseEnd, this, otherWidget);
            }
        }

        public bool ReadyToProceed { get => textQueue.Count == 0 || textQueue.LastOrDefault().text.Length == 0; }
        public int CrawlFactor { set => crawlFactor = value; }
    }
}
