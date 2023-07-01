using GridTactics.Main;
using GridTactics.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace GridTactics.SceneObjects.Widgets
{
    public class Button : Widget
    {
        public MethodInfo Action { get; set; }
        private object ActionParameter { get; set; }

        private NinePatch buttonFrame;
        private string style;
        private string pushedStyle;
        private string disabledStyle;
        private bool clicking;
        private GameSound Sound { get; set; } = GameSound.menu_select;

        public bool Radio { get; private set; } = false;
        private bool selected = false;
        public bool Selected { get => selected; }

        public Button(Widget iParent, float widgetDepth)
            : base(iParent, widgetDepth)
        {

        }

        public override void LoadAttributes(XmlNode xmlNode)
        {
            base.LoadAttributes(xmlNode);

            UpdateFrame();
        }

        private void UpdateFrame()
        {
            if (style != null)
            {
                if (buttonFrame == null) buttonFrame = new NinePatch(style, Depth);
                buttonFrame.SetSprite(style);
            }

            if (!Enabled) buttonFrame?.SetSprite(disabledStyle);
            else if (selected) buttonFrame?.SetSprite(pushedStyle);
        }

        public override void ApplyAlignment()
        {
            base.ApplyAlignment();

            if (buttonFrame != null) buttonFrame.Bounds = currentWindow;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            Color drawColor = (Enabled) ? Color : new Color(120, 120, 120, 255);
            if (buttonFrame != null) buttonFrame.FrameColor = drawColor;



            buttonFrame?.Draw(spriteBatch, Position);
        }

        public override Widget GetWidgetAt(Vector2 mousePosition)
        {
            if (!currentWindow.Contains(mousePosition - Position)) return null;

            return this;
        }

        public override void StartLeftClick(Vector2 mousePosition)
        {
            if (!Enabled) return;

            //Audio.PlaySound(GameSound.a_mainmenuselection);

            clicking = true;
            buttonFrame?.SetSprite(pushedStyle);
        }

        public override void EndLeftClick(Vector2 mouseStart, Vector2 mouseEnd, Widget otherWidget)
        {
            if (!Enabled) return;

            clicking = false;

            if (otherWidget == this)
            {
                Activate();

                if (Radio)
                {
                    RadioSelect();
                }
                else
                {
                    buttonFrame?.SetSprite(style);
                    EndMouseOver();
                }

                Audio.PlaySound(Sound);
            }
            else
            {
                if (!Radio || !Selected) buttonFrame?.SetSprite(style);
            }
        }

        public void Activate()
        {
            object[] parameters = (ActionParameter == null) ? null : new object[] { ActionParameter };
            Action?.Invoke(GetParent<ViewModel>(), parameters);
        }

        public void RadioSelect()
        {
            foreach (Widget peer in parent.ChildList)
            {
                Button buttonPeer = peer as Button;
                if (buttonPeer != null && buttonPeer.Radio) buttonPeer.UnSelect();
            }

            selected = true;
            buttonFrame?.SetSprite(PushedStyle);
            if (buttonFrame != null)
            {
                buttonFrame.FrameDepth = ((Button)parent.ChildList.Last(x => x is Button)).Depth + Widget.WIDGET_PEER_DEPTH_OFFSET;
            }

            foreach (Widget child in ChildList) child.Depth = buttonFrame.FrameDepth + Widget.WIDGET_DEPTH_OFFSET;
        }

        public override void StartRightClick(Vector2 mousePosition)
        {
            base.StartRightClick(mousePosition);

            if (clicking && !selected) buttonFrame?.SetSprite(Style);
            clicking = false;
        }

        public override void StartMouseOver()
        {
            if (!Enabled) return;

            base.StartMouseOver();

            if (clicking) buttonFrame?.SetSprite(PushedStyle);
        }

        public override void EndMouseOver()
        {
            if (!Enabled) return;

            base.EndMouseOver();

            if (clicking && !selected) buttonFrame?.SetSprite(Style);
        }

        public void UnSelect()
        {
            //if (!enabled) return;

            selected = false;
            buttonFrame?.SetSprite(Style);

            if (buttonFrame != null)
            {
                buttonFrame.FrameDepth = Depth;
            }

            foreach (Widget child in ChildList) child.Depth = this.Depth + Widget.WIDGET_DEPTH_OFFSET;
        }

        public override bool Enabled
        {
            set
            {
                base.Enabled = value;
                if (base.Enabled) buttonFrame?.SetSprite(Style);
                else if (DisabledStyle != null) buttonFrame?.SetSprite(DisabledStyle);
            }
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
        private string PushedStyle { get => pushedStyle; set { pushedStyle = value; UpdateFrame(); } }
        private string DisabledStyle { get => disabledStyle; set { disabledStyle = value; UpdateFrame(); } }
    }
}
