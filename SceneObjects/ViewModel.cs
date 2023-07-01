using GridTactics.Main;
using GridTactics.Models;
using GridTactics.SceneObjects.Controllers;
using K4os.Compression.LZ4;
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

namespace GridTactics.SceneObjects
{
    public abstract class ViewModel : Widget
    {
        protected Scene parentScene;
        private WidgetController widgetController;
        private Dictionary<string, Widget> childWidgets = new Dictionary<string, Widget>();

        public ViewModel(Scene iScene, PriorityLevel iPriorityLevel)
            : base()
        {
            parentScene = iScene;

            widgetController = iScene.AddController(new WidgetController(iPriorityLevel, this));

            currentWindow = new Rectangle(0, 0, CrossPlatformGame.ScreenWidth, CrossPlatformGame.ScreenHeight);
        }

        public ViewModel(Scene iScene, PriorityLevel iPriorityLevel, GameView viewName)
            : base()
        {
            parentScene = iScene;

            widgetController = iScene.AddController(new WidgetController(iPriorityLevel, this));

            currentWindow = new Rectangle(0, 0, CrossPlatformGame.ScreenWidth, CrossPlatformGame.ScreenHeight);

            LoadView(viewName);
        }

        protected void LoadView(GameView viewName)
        {
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(AssetCache.VIEWS[viewName]);

            XmlNodeList nodeList = xml.SelectNodes("/View/*");
            LoadChildren(nodeList, WIDGET_START_DEPTH);
        }

        public override void AddChild(Widget widget, XmlNode node)
        {
            base.AddChild(widget, node);

            if (!String.IsNullOrEmpty(widget.Name))
            {
                childWidgets[widget.Name] = widget;
            }
        }

        public override T GetWidget<T>(string widgetName)
        {
            Widget result;
            if (childWidgets.TryGetValue(widgetName, out result))
            {
                return result as T;
            }
            else return base.GetWidget<T>(widgetName);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise, null, null);

            base.Draw(spriteBatch);
        }

        public override void Terminate()
        {
            base.Terminate();

            widgetController.Terminate();
            OnTerminated?.Invoke();
        }

        public Scene ParentScene { get => parentScene; }

        public event Action OnTerminated;
    }
}
