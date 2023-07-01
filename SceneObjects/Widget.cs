using GridTactics.Models;
using GridTactics.SceneObjects.Controllers;
using GridTactics.SceneObjects.Widgets;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;


namespace GridTactics.SceneObjects
{
    public enum Alignment
    {
        Anchored,
        Relative,
        Absolute,
        Cascading,
        Vertical,
        ReverseVertical,
        Stretch,
        Left,
        Right,
        Center,
        BottomRight,
        Bottom,
        BottomLeft,
        InvertCascade,
        Horizontal
    }

    public class DataBinding
    {
        public IModelProperty ModelProperty { get; set; }
        public ModelChangeCallback ChangeCallback { get; set; }
        public string AttributeName { get; set; }
        public string AttributeValue { get; set; }
        public List<IModelProperty> RootDependencies { get; set; }
        public ModelChangeCallback UpdateRoot { get; set; }

        public DataBinding(IModelProperty iModelProperty, ModelChangeCallback iChangeCallback, string iAttributeName, string iAttributeValue, List<IModelProperty> iRootDependencies)
        {
            ModelProperty = iModelProperty;
            ChangeCallback = iChangeCallback;
            AttributeName = iAttributeName;
            AttributeValue = iAttributeValue;
            RootDependencies = iRootDependencies;
        }

        public void ClearRoots()
        {
            foreach (IModelProperty root in RootDependencies) root.ModelChanged -= UpdateRoot;
        }
    }

    public abstract class Widget : Overlay
    {
        protected const float WIDGET_START_DEPTH = 0.15f;
        protected const float WIDGET_DEPTH_OFFSET = -0.001f;
        protected const float WIDGET_PEER_DEPTH_OFFSET = -0.0005f;

        protected const int DEFAULT_TRANSITION_LENGTH = 250;

        public const int DEFAULT_TOOLTIP_DELAY = 400;
        private const int TOOLTIP_DESPAWN_RANGE = 32;
        private const int TOOLTIP_DESPAWN_DELTA = 8;

        protected static Assembly assembly = Assembly.GetAssembly(typeof(Widget));

        protected Rectangle bounds;
        private Vector2 anchor;
        protected virtual Rectangle Bounds { get => bounds; set => bounds = value; }

        protected Vector2[] layoutOffset = new Vector2[Enum.GetValues(typeof(Alignment)).Length];
        protected Alignment Alignment { get; set; } = Alignment.Absolute;
        protected int horizontalCenterAdjust;
        protected Rectangle currentWindow;

        protected Widget parent;

        protected TransitionController transition = null;

        protected int tooltipTime;
        protected Tooltip tooltipWidget;
        protected Vector2 tooltipOrigin;

        protected bool mousedOver;

        protected List<DataBinding> bindingList = new List<DataBinding>();

        public Widget()
            : base()
        {

        }

        public Widget(Widget iParent, float widgetDepth)
        {
            parent = iParent;
            Depth = widgetDepth;
        }

        public void LoadXml(XmlNode xmlNode)
        {
            LoadAttributes(xmlNode);

            ApplyAlignment();

            LoadChildren(xmlNode.ChildNodes, Depth + WIDGET_DEPTH_OFFSET);
        }

        protected virtual void ParseAttribute(string attributeName, string attributeValue)
        {
            PropertyInfo propertyInfo = GetType().GetProperty(attributeName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (propertyInfo == null) return;

            if (attributeValue.StartsWith("Bind "))
            {
                attributeValue = attributeValue.Substring(5);
                List<IModelProperty> rootDependencies = new List<IModelProperty>();
                IModelProperty modelProperty = LookupBinding<IModelProperty>(attributeValue, rootDependencies);
                ModelChangeCallback updateValue = () =>
                {
                    object value = modelProperty.GetValue();
                    if (value is string)
                    {
                        value = ParseString(value as string);
                        propertyInfo.SetValue(this, value);
                    }
                    else if (propertyInfo.PropertyType == typeof(string) && value != null) propertyInfo.SetValue(this, value.ToString());
                    else propertyInfo.SetValue(this, value);
                };
                modelProperty.ModelChanged += updateValue;
                updateValue();

                DataBinding dataBinding = new DataBinding(modelProperty, updateValue, attributeName, "Bind " + attributeValue, rootDependencies);
                ModelChangeCallback updateRoot = new ModelChangeCallback(() =>
                {
                    dataBinding.ModelProperty.ModelChanged -= updateValue;
                    bindingList.Remove(dataBinding);
                    ParseAttribute(dataBinding.AttributeName, dataBinding.AttributeValue);
                    dataBinding.ClearRoots();
                });
                dataBinding.UpdateRoot = updateRoot;
                foreach (IModelProperty root in rootDependencies) root.ModelChanged += updateRoot;

                bindingList.Add(dataBinding);
            }
            else if (attributeValue.StartsWith("Ref "))
            {
                attributeValue = attributeValue.Substring(4);
                object reference = LookupBinding<object>(attributeValue);
                if (reference is string)
                {
                    reference = ParseString(reference as string);
                    propertyInfo.SetValue(this, reference);
                }
                else if (propertyInfo.PropertyType == typeof(string) && reference != null) propertyInfo.SetValue(this, reference.ToString());
                else propertyInfo.SetValue(this, reference);
            }
            else
            {
                Type type = propertyInfo.PropertyType;
                if (type.IsEnum)
                {
                    propertyInfo.SetValue(this, Enum.Parse(type, attributeValue));
                }
                else if (type == typeof(Vector2))
                {
                    string[] tokens = attributeValue.Split(',');
                    propertyInfo.SetValue(this, new Vector2(ParseInt(tokens[0]), ParseInt(tokens[1])));
                }
                else if (type == typeof(Rectangle))
                {
                    string[] tokens = attributeValue.Split(',');
                    propertyInfo.SetValue(this, new Rectangle(ParseInt(tokens[0]), ParseInt(tokens[1]), ParseInt(tokens[2]), ParseInt(tokens[3])));
                }
                else if (type == typeof(AnimatedSprite))
                {
                    string[] tokens = attributeValue.Split(',');
                    Texture2D sprite = AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), tokens[0].Replace("\\", "_"))];
                    AnimatedSprite animatedSprite = new AnimatedSprite(sprite, new Dictionary<string, Animation>()
                    {
                        { "loop", new Animation(int.Parse(tokens[1]), int.Parse(tokens[2]), int.Parse(tokens[3]), int.Parse(tokens[4]), int.Parse(tokens[5]), int.Parse(tokens[6]), int.Parse(tokens[7])) }
                    });
                    animatedSprite.PlayAnimation("loop");
                    propertyInfo.SetValue(this, animatedSprite);
                }
                else if (type == typeof(Image.ImageDrawFunction))
                {
                    MethodInfo deletageInfo = GetType().GetMethod(attributeValue);
                    propertyInfo.SetValue(this, (Image.ImageDrawFunction)Delegate.CreateDelegate(typeof(Image.ImageDrawFunction), deletageInfo));
                }
                else if (type == typeof(bool)) propertyInfo.SetValue(this, bool.Parse(attributeValue));
                else if (type == typeof(int)) propertyInfo.SetValue(this, ParseInt(attributeValue));
                else if (type == typeof(float)) propertyInfo.SetValue(this, float.Parse(attributeValue, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture));
                else if (type == typeof(string)) propertyInfo.SetValue(this, ParseString(attributeValue));
                else if (type == typeof(Color)) propertyInfo.SetValue(this, Graphics.ParseHexcode(attributeValue));
                else if (type == typeof(MethodInfo)) propertyInfo.SetValue(this, GetParent<ViewModel>().GetType().GetMethod(attributeValue));
                else if (type == typeof(Texture2D)) propertyInfo.SetValue(this, AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), attributeValue)]);
            }
        }

        public virtual void LoadAttributes(XmlNode xmlNode)
        {
            foreach (XmlAttribute xmlAttribute in xmlNode.Attributes)
            {
                ParseAttribute(xmlAttribute.Name, xmlAttribute.Value);
            }
        }

        public virtual void ApplyAlignment()
        {
            switch (Alignment)
            {
                case Alignment.Left:
                    currentWindow = bounds;
                    currentWindow.X = parent.InnerBounds.Left + (int)parent.layoutOffset[(int)Alignment].X + bounds.X;
                    currentWindow.Y = parent.InnerBounds.Top + bounds.Y;
                    parent.AdjustLayoutOffset(Alignment, new Vector2(bounds.X + bounds.Width, 0));
                    break;

                case Alignment.Anchored:
                    currentWindow = bounds;

                    break;

                case Alignment.Cascading:
                    if ((int)parent.layoutOffset[(int)Alignment].X + bounds.Width > parent.InnerBounds.Width)
                    {
                        parent.AdjustLayoutOffset(Alignment, new Vector2(-parent.layoutOffset[(int)Alignment].X, bounds.Height));
                    }

                    currentWindow.X = parent.InnerBounds.Left + (int)parent.layoutOffset[(int)Alignment].X + bounds.X;
                    currentWindow.Y = parent.InnerBounds.Top + (int)parent.layoutOffset[(int)Alignment].Y + bounds.Y;
                    currentWindow.Width = bounds.Width;
                    currentWindow.Height = bounds.Height;

                    parent.AdjustLayoutOffset(Alignment, new Vector2(bounds.X + bounds.Width, 0));

                    break;

                case Alignment.Vertical:
                    currentWindow.X += (parent.OuterBounds.X - (bounds.Width - parent.OuterBounds.Width) / 2) + bounds.X;
                    currentWindow.Y = (parent.InnerBounds.Top + (int)parent.layoutOffset[(int)Alignment].Y) + bounds.Y;
                    currentWindow.Width = bounds.Width;
                    currentWindow.Height = bounds.Height;

                    parent.AdjustLayoutOffset(Alignment, new Vector2(0, bounds.Y + bounds.Height));
                    break;

                case Alignment.ReverseVertical:
                    currentWindow.X += parent.OuterBounds.X - (bounds.Width - parent.OuterBounds.Width) / 2;
                    currentWindow.Y = parent.InnerBounds.Bottom - (int)parent.layoutOffset[(int)Alignment].Y - bounds.Height;
                    currentWindow.Width = bounds.Width;
                    currentWindow.Height = bounds.Height;

                    parent.AdjustLayoutOffset(Alignment, new Vector2(0, -bounds.Height - bounds.Y));
                    break;

                case Alignment.Center:
                    currentWindow.X = (parent.InnerBounds.Center.X - bounds.Width / 2) + bounds.X;
                    currentWindow.Y = (parent.InnerBounds.Center.Y - bounds.Height / 2) + bounds.Y;
                    currentWindow.Width = bounds.Width;
                    currentWindow.Height = bounds.Height;
                    break;

                case Alignment.Absolute:
                    currentWindow = bounds;
                    if (parent != null)
                    {
                        currentWindow.X += parent.InnerBounds.X;
                        currentWindow.Y += parent.InnerBounds.Y;
                    }
                    break;

                case Alignment.Relative:
                    currentWindow = bounds;
                    currentWindow.X += parent.InnerBounds.X;
                    currentWindow.Y += parent.InnerBounds.Y;
                    break;

                case Alignment.Stretch:
                    bounds = currentWindow = parent.InnerBounds;
                    break;

                case Alignment.BottomRight:
                    currentWindow.X = parent.InnerBounds.Right + bounds.X;
                    currentWindow.Y = parent.InnerBounds.Bottom + bounds.Y;
                    break;

                case Alignment.Bottom:
                    currentWindow.X += parent.OuterBounds.X - (bounds.Width - parent.OuterBounds.Width) / 2;
                    currentWindow.Y = parent.InnerBounds.Bottom - bounds.Height;
                    currentWindow.Width = bounds.Width;
                    currentWindow.Height = bounds.Height;
                    break;

                case Alignment.InvertCascade:
                    if ((int)parent.layoutOffset[(int)Alignment].X + bounds.Width > parent.InnerBounds.Width)
                    {
                        parent.AdjustLayoutOffset(Alignment, new Vector2(parent.layoutOffset[(int)Alignment].X, bounds.Height));
                    }

                    currentWindow.X = parent.InnerBounds.Left + (int)parent.layoutOffset[(int)Alignment].X + bounds.X;
                    currentWindow.Y = parent.InnerBounds.Bottom + (int)parent.layoutOffset[(int)Alignment].Y - bounds.Height - bounds.Y;
                    currentWindow.Width = bounds.Width;
                    currentWindow.Height = bounds.Height;

                    parent.AdjustLayoutOffset(Alignment, new Vector2(bounds.X + bounds.Width, 0));
                    break;

                case Alignment.Horizontal:
                    currentWindow.X = parent.InnerBounds.Left + (int)parent.layoutOffset[(int)Alignment].X + bounds.X;
                    currentWindow.Y = parent.InnerBounds.Top + (parent.InnerBounds.Height - bounds.Height) / 2 + bounds.Y;
                    currentWindow.Width = bounds.Width;
                    currentWindow.Height = bounds.Height;

                    parent.AdjustLayoutOffset(Alignment, new Vector2(bounds.X + bounds.Width, 0));
                    break;

                case Alignment.BottomLeft:
                    currentWindow.X = parent.InnerBounds.Left + bounds.X;
                    currentWindow.Y = parent.InnerBounds.Bottom - bounds.Height + bounds.Y;
                    break;
            }
        }

        protected virtual int ParseInt(string token)
        {
            if (token[0] == '-') return -ParseInt(token.Remove(0, 1));
            else if (token[0] == '$')
            {
                switch (token)
                {
                    case "$ScreenWidth": return CrossPlatformGame.ScreenWidth;
                    case "$ScreenHeight": return CrossPlatformGame.ScreenHeight;
                    case "$CenterX": return CrossPlatformGame.ScreenWidth / 2;
                    case "$CenterY": return CrossPlatformGame.ScreenHeight / 2;
                    case "$Top": return 0;
                    case "$Bottom": return CrossPlatformGame.ScreenHeight;
                    default: throw new Exception();
                }
            }

            return int.Parse(token);
        }

        protected virtual string ParseString(string token)
        {
            if (String.IsNullOrEmpty(token)) return "";
            else if (token[0] != '$') return token;

            string[] tokens = token.Split('.');
            if (tokens.Length == 1)
            {
                return "ERROR";
            }
            else
            {
                switch (tokens[0])
                {
                    case "$PlayerProfile": return (GameProfile.PlayerProfile.GetType().GetProperty(tokens[1]).GetValue(GameProfile.PlayerProfile) as ModelProperty<string>).Value;
                }
            }

            return GetParent<ViewModel>().ParseString(token);
        }

        public virtual void LoadChildren(XmlNodeList nodeList, float widgetDepth)
        {
            float depthOffset = 0;
            foreach (XmlNode node in nodeList)
            {
                Widget widget;
                switch (node.Name)
                {
                    case "LineBreak":
                        if (node.Attributes != null && node.Attributes["Height"] != null)
                        {
                            int offset = int.Parse(node.Attributes["Height"].Value);
                            layoutOffset[(int)Alignment.Cascading] = new Vector2(0, layoutOffset[(int)Alignment.Cascading].Y + offset);
                            layoutOffset[(int)Alignment.Vertical] = new Vector2(0, layoutOffset[(int)Alignment.Vertical].Y + offset);
                        }
                        else
                        {
                            layoutOffset[(int)Alignment.Cascading] = new Vector2(0, layoutOffset[(int)Alignment.Cascading].Y + ChildList.Last().bounds.Height);
                            layoutOffset[(int)Alignment.Vertical] = new Vector2(0, layoutOffset[(int)Alignment.Vertical].Y + ChildList.Last().bounds.Height);
                        }
                        continue;

                    default:
                        if (node.Name.Contains('.')) widget = (Widget)assembly.CreateInstance(CrossPlatformGame.GAME_NAME + "." + node.Name, false, BindingFlags.CreateInstance, null, new object[] { this, widgetDepth + depthOffset }, null, null);
                        else widget = (Widget)assembly.CreateInstance(CrossPlatformGame.GAME_NAME + ".SceneObjects.Widgets." + node.Name, false, BindingFlags.CreateInstance, null, new object[] { this, widgetDepth + depthOffset }, null, null);
                        break;
                }

                AddChild(widget, node);
                depthOffset += WIDGET_PEER_DEPTH_OFFSET;
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (Closed && !ChildList.Any(x => x.Transitioning))
                Terminate();

            if (!Visible) return;

            foreach (Widget widget in ChildList) widget.Update(gameTime);

            if (mousedOver && !Transitioning && tooltipWidget == null && Input.DeltaMouseGame.Length() < 1 && Tooltip != "")
            {
                tooltipTime += gameTime.ElapsedGameTime.Milliseconds;
                if (tooltipTime >= TooltipDelay)
                {
                    tooltipWidget = new Tooltip(Input.MousePosition, Tooltip);
                    tooltipTime = 0;
                    tooltipOrigin = new Vector2(tooltipWidget.InnerBounds.X, tooltipWidget.InnerBounds.Y);
                }
            }

            if (tooltipWidget != null)
            {
                tooltipWidget.Update(gameTime);

                if (Vector2.Distance(tooltipOrigin, new Vector2(tooltipWidget.InnerBounds.X, tooltipWidget.InnerBounds.Y)) > TOOLTIP_DESPAWN_RANGE || Input.DeltaMouseGame.Length() > TOOLTIP_DESPAWN_DELTA)
                    DeleteTooltip();
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Transitioning && !terminated)
            {
                foreach (Widget widget in ChildList)
                {
                    if (widget.Visible)
                        widget.Draw(spriteBatch);
                }

                tooltipWidget?.Draw(spriteBatch);
            }
        }

        public virtual void AddChild(Widget widget, XmlNode node)
        {
            ChildList.Add(widget);
            widget.LoadXml(node);
        }

        public virtual Widget GetWidgetAt(Vector2 mousePosition)
        {
            if (!currentWindow.Contains(mousePosition - Position)) return null;
            foreach (Widget child in ChildList)
            {
                Widget widget = child.GetWidgetAt(mousePosition);
                if (widget != null) return widget;
            }

            return this;
        }

        public T GetParent<T>() where T : Widget
        {
            if (parent == null) return null;
            if (parent is T) return parent as T;
            return parent.GetParent<T>();
        }

        public Widget GetDescendent(Widget superParent)
        {
            if (parent == null) return null;
            if (parent == superParent) return this;

            return parent.GetDescendent(superParent);
        }

        public virtual T GetWidget<T>(string widgetName) where T : Widget
        {
            foreach (Widget child in ChildList)
            {
                if (child.Name == widgetName) return child as T;
                else
                {
                    Widget result = child.GetWidget<T>(widgetName);
                    if (result != null) return result as T;
                }
            }

            return null;
        }

        public T LookupBinding<T>(string bindingName, List<IModelProperty> rootDependencies = null) where T : class
        {
            string[] tokens = bindingName.Split('.');

            object dataContext;
            switch (tokens[0])
            {
                case "PlayerProfile":
                    dataContext = GameProfile.PlayerProfile;
                    tokens = tokens.TakeLast(tokens.Length - 1).ToArray();
                    break;

                case "DataGrid":
                    DataGrid dataGrid = GetParent<DataGrid>();
                    int i = dataGrid.ChildList.IndexOf(this);
                    if (i == -1) i = dataGrid.ChildList.IndexOf(GetDescendent(dataGrid));
                    dataContext = dataGrid.Items.ElementAtOrDefault(i);
                    if (tokens.Length == 1) return dataContext as T;
                    tokens = tokens.TakeLast(tokens.Length - 1).ToArray();
                    break;

                default:
                    dataContext = GetParent<ViewModel>();
                    break;
            }

            while (dataContext is IModelProperty)
            {
                if (rootDependencies != null) rootDependencies.Add(dataContext as IModelProperty);
                dataContext = ((IModelProperty)dataContext).GetValue();
            }

            while (tokens.Length > 1)
            {
                var property = dataContext.GetType().GetProperty(tokens[0]);
                dataContext = property.GetValue(dataContext);

                tokens = tokens.TakeLast(tokens.Length - 1).ToArray();

                while (dataContext is IModelProperty)
                {
                    if (rootDependencies != null) rootDependencies.Add(dataContext as IModelProperty);
                    dataContext = ((IModelProperty)dataContext).GetValue();
                }
            }

            while (dataContext is IModelProperty)
            {
                if (rootDependencies != null) rootDependencies.Add(dataContext as IModelProperty);
                dataContext = ((IModelProperty)dataContext).GetValue();
            }

            return dataContext.GetType().GetProperty(tokens[0]).GetValue(dataContext) as T;
        }

        public void AdjustLayoutOffset(Alignment alignment, Vector2 offset)
        {
            layoutOffset[(int)alignment] += offset;
        }

        public virtual void StartLeftClick(Vector2 mousePosition) { }
        public virtual void StartRightClick(Vector2 mousePosition) { }
        public virtual void EndLeftClick(Vector2 mouseStart, Vector2 mouseEnd, Widget otherWidget) { }
        public virtual void EndRightClick(Vector2 mouseStart, Vector2 mouseEnd, Widget otherWidget) { }
        public virtual void LeftClickChild(Vector2 mouseStart, Vector2 mouseEnd, Widget clickWidget, Widget otherWidget) { }
        public virtual void RightClickChild(Vector2 mouseStart, Vector2 mouseEnd, Widget clickWidget, Widget otherWidget) { }
        public virtual void LoseFocus() { }

        public virtual void StartMouseOver()
        {
            mousedOver = true;
        }

        public virtual void EndMouseOver()
        {
            mousedOver = false;
            DeleteTooltip();
        }

        private void DeleteTooltip()
        {
            tooltipWidget = null;
            tooltipTime = 0;
        }

        public override void Terminate()
        {
            base.Terminate();

            foreach (Widget widget in ChildList) widget.Terminate();
            foreach (DataBinding binding in bindingList)
            {
                binding.ModelProperty.Unbind(binding.ChangeCallback);
                binding.ClearRoots();
            }
        }

        public virtual void Close()
        {
            Closed = true;
            foreach (Widget widget in ChildList) widget.Close();

            if (!ChildList.Any(x => x.Transitioning)) Terminate();
        }

        public List<Widget> ChildList { get; protected set; } = new List<Widget>();

        public virtual Vector2 Position
        {
            get
            {
                if (Alignment == Alignment.Anchored) return Anchor;
                else if (parent != null) return parent.Position;
                else return Vector2.Zero;
            }
        }
        protected Vector2 Anchor { get => anchor; set { anchor = value; Alignment = Alignment.Anchored; } }
        public Rectangle OuterBounds { get => currentWindow; set => currentWindow = value; }
        public Rectangle InnerMargin { get; protected set; } = new Rectangle();
        public Rectangle InnerBounds { get => new Rectangle(currentWindow.Left + InnerMargin.Left, currentWindow.Top + InnerMargin.Top, currentWindow.Width - InnerMargin.Left - InnerMargin.Width, currentWindow.Height - InnerMargin.Top - InnerMargin.Height); }
        public Vector2 AbsolutePosition { get => Position + new Vector2(currentWindow.X, currentWindow.Y); }

        public string Name { get; protected set; } = "Widget";
        protected GameFont Font { get; set; } = GameFont.Tooltip;
        public Color Color { get; protected set; } = Color.White;
        public float Depth { get; set; } = 1.0f;
        public virtual bool Visible { get; set; } = true;
        public virtual bool Enabled { get; set; } = true;

        public int TooltipDelay { get; protected set; } = DEFAULT_TOOLTIP_DELAY;
        public string Tooltip { get; protected set; } = "";

        public virtual bool Transitioning { get => transition != null; }

        public bool Closed { get; protected set; } = false;
        public override bool Terminated { get => terminated && !Transitioning; }
    }
}
