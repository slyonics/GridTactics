using GridTactics.Main;
using GridTactics.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace GridTactics.SceneObjects.Widgets
{
    public class Gauge : Widget
    {
        private string frame;
        private string background;
        private NinePatch gaugeFrame;
        private NinePatch gaugeBackground;

        public int Height { get; set; }

        public float Minimum { get; private set; } = 0;
        public float Maximum { get; private set; } = 100;

        private string Frame
        {
            get => frame;
            set
            {
                frame = value;
                UpdateFrame();
            }
        }

        private string Background
        {
            get => background;
            set
            {
                background = value;
                UpdateBackground();
            }
        }

        public Gauge(Widget iParent, float widgetDepth)
            : base(iParent, widgetDepth)
        {

        }

        private void UpdateFrame()
        {
            if (frame != null)
            {
                gaugeFrame = new NinePatch("Gauges_" + frame, Depth - 0.01f);
                gaugeFrame.Bounds = currentWindow;
            }
        }

        private void UpdateBackground()
        {
            if (background != null)
            {
                gaugeBackground = new NinePatch("Gauges_" + background, Depth);
                gaugeBackground.Bounds = currentWindow;
            }
        }

        public override void ApplyAlignment()
        {
            base.ApplyAlignment();

            UpdateBackground();
            UpdateFrame();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            gaugeBackground?.Draw(spriteBatch, Position);
            gaugeFrame?.Draw(spriteBatch, Position);
        }
    }

    public class GaugeBar : Widget
    {
        private Gauge parentGauge;

        private float barValue;

        private string bar;
        private string Bar
        {
            get => bar;
            set
            {
                bar = value;
                UpdateBarSprite();
            }
        }

        private NinePatch gaugeBackground;

        private ModelProperty<float> binding;

        public GaugeBar(Gauge iParent, float widgetDepth)
            : base(iParent, widgetDepth)
        {
            parentGauge = iParent;
        }

        private void UpdateBarSprite()
        {
            if (bar != null)
            {
                gaugeBackground = new NinePatch("Gauges_" + bar, Depth);
            }
        }

        private void UpdateBarValue()
        {
            parentGauge = parent as Gauge;
            int barWidth = (int)(barValue / parentGauge.Maximum * parentGauge.InnerBounds.Width);
            currentWindow = bounds = new Rectangle(parentGauge.InnerBounds.Left, parentGauge.InnerBounds.Top, parentGauge.InnerBounds.Width, parentGauge.InnerBounds.Height);

            if (gaugeBackground != null)
            {
                gaugeBackground.Bounds = new Rectangle(currentWindow.Left, currentWindow.Top, barWidth, currentWindow.Height);
            }
        }

        public override void ApplyAlignment()
        {
            UpdateBarSprite();
            UpdateBarValue();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            gaugeBackground?.Draw(spriteBatch, Position);
        }

        public float Value
        {
            get => barValue;
            set
            {
                barValue = value;
                UpdateBarValue();
            }
        }
    }

    public class GaugeSlider : Widget
    {
        private Gauge parentGauge;
        private GaugeBar parentGaugeBar;

        private string slider;
        private NinePatch sliderBackground;

        private bool dragging;
        private float leftX;
        private float rightX;

        private string Slider
        {
            get => slider;
            set
            {
                slider = value;
                UpdateSlider();
            }
        }

        public GaugeSlider(GaugeBar iParentGaugeBar, float widgetDepth)
            : base(iParentGaugeBar.GetParent<Gauge>(), widgetDepth - 0.01f)
        {
            parentGauge = iParentGaugeBar.GetParent<Gauge>();
            parentGaugeBar = iParentGaugeBar;

            ApplyAlignment();
        }

        private void UpdateSlider()
        {
            if (slider != null)
            {
                sliderBackground = new NinePatch("Gauges_" + slider, Depth - 0.001f, true);

                int sliderWidth = sliderBackground.Sprite.Width;
                int sliderHeight = sliderBackground.Sprite.Height;
                int barWidth = (int)(parentGaugeBar.Value / parentGauge.Maximum * (parentGauge.InnerBounds.Width));

                Rectangle roughBounds = new Rectangle(parentGauge.InnerBounds.Left + barWidth - sliderWidth / 2, parentGauge.InnerBounds.Top + (parentGauge.InnerBounds.Height - sliderHeight) / 2, sliderWidth, sliderHeight);
                //roughBounds.X = parentGauge.InnerBounds.Left + barWidth - 48 + sliderWidth / 2 + 12;
                if (roughBounds.X > parent.InnerBounds.Right - sliderWidth) roughBounds.X = parent.InnerBounds.Right - sliderWidth;
                if (roughBounds.X < parent.InnerBounds.Left) roughBounds.X = parent.InnerBounds.Left;
                currentWindow = bounds = sliderBackground.Bounds = roughBounds;
            }
        }

        public override void ApplyAlignment()
        {
            if (parentGaugeBar == null) return;

            UpdateSlider();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (dragging)
            {
                if (Input.MousePosition.X <= leftX) parentGaugeBar.Value = parentGauge.Minimum;
                else if (Input.MousePosition.X >= rightX) parentGaugeBar.Value = parentGauge.Maximum;
                else parentGaugeBar.Value = MathHelper.Lerp(parentGauge.Minimum, parentGauge.Maximum, (Input.MousePosition.X - leftX) / (rightX - leftX));

                ApplyAlignment();
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            sliderBackground?.Draw(spriteBatch, Position);
        }

        public override void StartLeftClick(Vector2 mousePosition)
        {
            if (slider == null) return;

            dragging = true;

            float interval = (parentGaugeBar.Value - parentGauge.Minimum) / (parentGauge.Maximum - parentGauge.Minimum);
            int barWidth = parentGauge.InnerBounds.Width;
            leftX = mousePosition.X - (barWidth * interval);
            rightX = mousePosition.X + (barWidth * (1.0f - interval));
        }

        public override void EndLeftClick(Vector2 mouseStart, Vector2 mouseEnd, Widget otherWidget)
        {
            if (slider == null) return;

            dragging = false;
        }
    }
}
