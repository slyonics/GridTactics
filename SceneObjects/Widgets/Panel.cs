using GridTactics.Main;
using GridTactics.SceneObjects.Controllers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace GridTactics.SceneObjects.Widgets
{
    public class Panel : Widget
    {
        private enum TransitionType
        {
            None,
            Shrink,
            Expand,
            FadeIn,
            FadeOut
        }

        private enum ResizeType
        {
            None,
            Width,
            Height,
            Both
        }

        private NinePatch panelFrame;

        private TransitionType TransitionIn { get; set; } = TransitionType.None;
        private TransitionType TransitionOut { get; set; } = TransitionType.None;

        private Rectangle startWindow;
        private Rectangle endWindow;
        private Color startColor;
        private Color endColor;

        private ResizeType Resize { get; set; } = ResizeType.None;

        public Panel(Widget iParent, float widgetDepth)
            : base(iParent, widgetDepth)
        {

        }

        public override void LoadAttributes(XmlNode xmlNode)
        {
            foreach (XmlAttribute xmlAttribute in xmlNode.Attributes)
            {
                ParseAttribute(xmlAttribute.Name, xmlAttribute.Value);
            }

            UpdateFrame();
        }

        public void UpdateFrame()
        {
            if (style != null)
            {
                if (panelFrame == null) panelFrame = new NinePatch(style, Depth);
                panelFrame.SetSprite(style);
            }
        }

        public override void LoadChildren(XmlNodeList nodeList, float widgetDepth)
        {
            base.LoadChildren(nodeList, widgetDepth);

            StartTransitionIn();

            if (panelFrame != null) panelFrame.Bounds = currentWindow;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if ((!Transitioning || TransitionIn != TransitionType.Expand) && !terminated)
            {
                foreach (Widget widget in ChildList)
                {
                    if (widget.Visible)
                        widget.Draw(spriteBatch);
                }

                tooltipWidget?.Draw(spriteBatch);
            }

            panelFrame?.Draw(spriteBatch, Position);
        }

        public override void Close()
        {
            base.Close();

            StartTransitionOut();
        }

        public void StartTransitionIn()
        {
            switch (TransitionIn)
            {
                case TransitionType.Expand:
                    endWindow = currentWindow;
                    currentWindow = startWindow = new Rectangle((int)currentWindow.Center.X, (int)currentWindow.Center.Y, 0, 0);
                    transition = new TransitionController(TransitionDirection.In, DEFAULT_TRANSITION_LENGTH);
                    transition.UpdateTransition += UpdateTransition;
                    transition.FinishTransition += FinishTransition;
                    GetParent<ViewModel>().ParentScene.AddController(transition);
                    break;

                    /*
                case TransitionType.FadeIn:
                    endWindow = startWindow = currentWindow;
                    startColor = new Color(0, 0, 0, 0);
                    endColor = new Color(255, 255, 255, 255);
                    transitionState = TransitionState.In;
                    break;*/
            }
        }

        public void StartTransitionOut()
        {
            switch (TransitionOut)
            {
                case TransitionType.Shrink:
                    endWindow = currentWindow;
                    startWindow = new Rectangle((int)currentWindow.Center.X, (int)currentWindow.Center.Y, 0, 0);
                    transition = new TransitionController(TransitionDirection.Out, DEFAULT_TRANSITION_LENGTH);
                    transition.UpdateTransition += UpdateTransition;
                    transition.FinishTransition += FinishTransition;
                    GetParent<ViewModel>().ParentScene.AddController(transition);
                    break;
            }
        }

        private void UpdateTransition(float transitionProgress)
        {
            currentWindow = Extensions.Lerp(startWindow, endWindow, transitionProgress);

            if (panelFrame != null) panelFrame.Bounds = currentWindow;
        }

        private void FinishTransition(TransitionDirection transitionDirection)
        {
            transition = null;
            if (Closed) Terminate();
        }

        private string style;
        private string Style { get => style; set { style = value; UpdateFrame(); } }
    }
}
