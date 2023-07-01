using GridTactics.Main;
using GridTactics.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Xml;

namespace GridTactics.SceneObjects.Widgets
{
    public class Textbox : Widget
    {
        public MethodInfo Validator { get; set; }

        public bool NumbersOnly { get; set; }

        private string text;
        public string Text { get => text; set => text = value; }

        private NinePatch textboxFrame;
        private string style;
        private string activeStyle;
        private bool active;
        public bool Active { get => active; set { active = value; UpdateFrame(); } }

        private bool blinkCarat;
        private int blinkTime;

        public Textbox(Widget iParent, float widgetDepth)
            : base(iParent, widgetDepth)
        {

        }

        public override void LoadAttributes(XmlNode xmlNode)
        {
            base.LoadAttributes(xmlNode);

            UpdateFrame();
        }

        public override void ApplyAlignment()
        {
            base.ApplyAlignment();

            if (textboxFrame != null) textboxFrame.Bounds = currentWindow;
        }

        public override void Update(GameTime gameTime)
        {
            blinkTime += (int)gameTime.ElapsedGameTime.TotalMilliseconds;
            blinkTime %= 1000;

            if (active)
            {
                var key = Input.CurrentInput.GetKey();
                if (key != Microsoft.Xna.Framework.Input.Keys.None)
                {
                    bool shift = (Input.CurrentInput.KeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift) || Input.CurrentInput.KeyDown(Microsoft.Xna.Framework.Input.Keys.RightShift));
                    char keyChar;
                    if (key < Microsoft.Xna.Framework.Input.Keys.A)
                    {
                        keyChar = (char)(key - Microsoft.Xna.Framework.Input.Keys.D0 + '0');
                    }
                    else if (NumbersOnly) return;
                    else
                    {
                        keyChar = (char)(key - Microsoft.Xna.Framework.Input.Keys.A + (shift ? 'A' : 'a'));
                    }

                    if (Main.Text.GetStringLength(Font, Text + keyChar + '_') <= InnerBounds.Width)
                    {
                        Text = Text + keyChar;
                        blinkTime = 0;
                        Validate();
                    }
                }
                else if (Input.CurrentInput.KeyPressed(Microsoft.Xna.Framework.Input.Keys.Back) && Text.Length > 0)
                {
                    Text = Text.Substring(0, Text.Length - 1);

                    blinkTime = 0;

                    Validate();
                }
                else if (Input.CurrentInput.KeyPressed(Microsoft.Xna.Framework.Input.Keys.Enter))
                {
                    Active = false;

                    if (active) textboxFrame.SetSprite(activeStyle);
                    else textboxFrame.SetSprite(style);
                }
            }

            blinkCarat = blinkTime < 700;

            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            textboxFrame?.Draw(spriteBatch, Position);

            Color drawColor = (parent.Enabled) ? Color : new Color(160, 160, 160, 255);

            Main.Text.DrawText(spriteBatch, new Vector2(currentWindow.Left + InnerMargin.Left, currentWindow.Top + InnerMargin.Top) + Position, Font, text, drawColor);

            if (active && blinkCarat)
            {
                Main.Text.DrawText(spriteBatch, new Vector2(currentWindow.Left + InnerMargin.Left + Main.Text.GetStringLength(Font, Text), currentWindow.Top + InnerMargin.Top) + Position, Font, "_", drawColor);
            }
        }

        private void UpdateFrame()
        {
            if (style != null)
            {
                if (textboxFrame == null) textboxFrame = new NinePatch(style, Depth);
                textboxFrame.SetSprite(style);
            }

            if (active) textboxFrame?.SetSprite(activeStyle);
        }

        public override void StartLeftClick(Vector2 mousePosition)
        {
            Audio.PlaySound(GameSound.menu_select);

            Active = !active;

            if (active) textboxFrame.SetSprite(activeStyle);
            else textboxFrame.SetSprite(style);
        }

        public override void EndLeftClick(Vector2 mouseStart, Vector2 mouseEnd, Widget otherWidget)
        {
            if (otherWidget == this) Active = true;
            else Active = false;

            if (active) textboxFrame.SetSprite(activeStyle);
            else textboxFrame.SetSprite(style);
        }

        public override void LoseFocus()
        {
            Active = false;

            textboxFrame.SetSprite(style);
        }

        private string Style
        {
            get => style;
            set
            {
                style = value;
                UpdateFrame();
            }
        }

        public void Validate()
        {
            Textbox[] parameters = new Textbox[] { this };
            Validator?.Invoke(GetParent<ViewModel>(), parameters);
        }

        private string ActiveStyle
        {
            get => activeStyle;
            set
            {
                activeStyle = value;
                UpdateFrame();
            }
        }
    }
}
