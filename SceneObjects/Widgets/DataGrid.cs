using GridTactics.Main;
using GridTactics.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;

namespace GridTactics.SceneObjects.Widgets
{
    public class DataGrid : Widget
    {
        private bool finishedLoading = false;

        private IEnumerable<object> items;
        public IEnumerable<object> Items { get => items; private set { items = value; ItemsChanged(); } }

        private XmlNode dataTemplate;

        public bool Scrolling { get; set; } = true;

        public DataGrid(Widget iParent, float widgetDepth)
            : base(iParent, widgetDepth)
        {

        }

        public override void LoadAttributes(XmlNode xmlNode)
        {
            dataTemplate = xmlNode.ChildNodes[0];

            foreach (XmlAttribute xmlAttribute in xmlNode.Attributes)
            {
                switch (xmlAttribute.Name)
                {
                    default: ParseAttribute(xmlAttribute.Name, xmlAttribute.Value); break;
                }
            }

            finishedLoading = true;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Transitioning && !terminated)
            {
                foreach (Widget widget in ChildList)
                {
                    if (widget.Visible && IsChildVisible(widget))
                        widget.Draw(spriteBatch);
                }

                tooltipWidget?.Draw(spriteBatch);
            }

        }

        private void ItemsChanged()
        {
            if (!finishedLoading) return;

            for (int i = 0; i < layoutOffset.Length; i++) { layoutOffset[i] = new Vector2(); }
            foreach (Widget widget in ChildList) widget.Terminate();
            ChildList.Clear();

            float depthOffset = 0.0f;
            foreach (var modelProperty in items)
            {
                Widget childWidget;
                if (!dataTemplate.Name.Contains('.')) childWidget = (Widget)assembly.CreateInstance(CrossPlatformGame.GAME_NAME + ".SceneObjects.Widgets." + dataTemplate.Name, false, BindingFlags.CreateInstance, null, new object[] { this, Depth + depthOffset + WIDGET_DEPTH_OFFSET }, null, null);
                else childWidget = (Widget)assembly.CreateInstance(CrossPlatformGame.GAME_NAME + "." + dataTemplate.Name, false, BindingFlags.CreateInstance, null, new object[] { this, Depth + depthOffset + WIDGET_DEPTH_OFFSET }, null, null);

                AddChild(childWidget, dataTemplate);

                depthOffset -= 0.0001f;
            }
        }

        public override void LoadChildren(XmlNodeList nodeList, float widgetDepth)
        {
            ItemsChanged();
        }

        public override Widget GetWidgetAt(Vector2 mousePosition)
        {
            if (!currentWindow.Contains(mousePosition - base.Position)) return null;
            foreach (Widget child in ChildList)
            {
                Widget widget = child.GetWidgetAt(mousePosition);
                if (widget != null) return widget;
            }

            return this;
        }

        public bool IsChildVisible(Widget child)
        {
            if (!Scrolling) return true;

            return (child.OuterBounds.Bottom - scrollOffset.Y < InnerBounds.Bottom + InnerMargin.Y + 1) &&
                   (child.OuterBounds.Top - scrollOffset.Y >= InnerBounds.Top + InnerMargin.Y);
        }

        public void ScrollUp()
        {
            if (!Scrolling) return;

            scrollOffset.Y -= ChildList.Last().OuterBounds.Height;
        }

        public void ScrollDown()
        {
            if (!Scrolling) return;

            scrollOffset.Y += ChildList.Last().OuterBounds.Height;
        }

        private Vector2 scrollOffset;

        public override Vector2 Position
        {
            get
            {
                return base.Position - scrollOffset;
            }
        }
    }
}
