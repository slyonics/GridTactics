using GridTactics.Main;
using GridTactics.SceneObjects;
using Microsoft.Xna.Framework.Graphics;

namespace GridTactics.Scenes.ConversationScene
{
    public class PortraitController : Controller
    {
        protected Portrait portrait;
        private float transitionTime;
        private float transitionLength;
        protected float transitionInterval;

        public PortraitController(Portrait iPortrait, float iTransitionLength)
            : base(PriorityLevel.GameLevel)
        {
            portrait = iPortrait;
            transitionLength = iTransitionLength;
        }

        public override void PreUpdate(GameTime gameTime)
        {
            if (transitionTime < transitionLength)
            {
                transitionTime += gameTime.ElapsedGameTime.Milliseconds / 1000.0f;
                if (transitionTime >= transitionLength)
                {
                    FinishTransition();
                    Terminate();
                }

                transitionInterval = transitionTime / transitionLength;
            }
        }

        public virtual void FinishTransition()
        {

        }
    }

    public class PortraitPositionController : PortraitController
    {
        private Vector2 startPosition;
        private Vector2 endPosition;

        public PortraitPositionController(Portrait iPortrait, Vector2 iPosition, float iTransitionLength)
            : base(iPortrait, iTransitionLength)
        {
            startPosition = portrait.Position;
            endPosition = iPosition;
        }

        public override void PostUpdate(GameTime gameTime)
        {
            if (Terminated) return;
            portrait.Position = Vector2.Lerp(startPosition, endPosition, transitionInterval);
        }

        public override void FinishTransition()
        {
            portrait.Position = endPosition;
        }
    }

    public class PortraitSpriteController : PortraitController
    {
        private Effect shader;
        private Texture2D endSprite;

        public PortraitSpriteController(Portrait iPortrait, Texture2D iEndSprite, float iTransitionLength)
            : base(iPortrait, iTransitionLength)
        {
            endSprite = iEndSprite;

            shader = AssetCache.EFFECTS[GameShader.Portrait].Clone();
            shader.Parameters["s1"].SetValue(endSprite);
            portrait.Shader = shader;
        }

        public override void PostUpdate(GameTime gameTime)
        {
            shader.Parameters["transitionInterval"].SetValue(transitionInterval);
        }

        public override void FinishTransition()
        {
            Vector2 scale = portrait.AnimatedSprite.Scale;
            portrait.AnimatedSprite = new AnimatedSprite(endSprite, null);
            portrait.Shader = null;
            portrait.AnimatedSprite.Scale = scale;
        }
    }

    public class PortraitColorController : PortraitController
    {
        private Color startColor;
        private Color endColor;

        public PortraitColorController(Portrait iPortrait, Color iEndColor, float iTransitionLength)
            : base(iPortrait, iTransitionLength)
        {
            startColor = portrait.AnimatedSprite.SpriteColor;
            endColor = iEndColor;
        }

        public override void PostUpdate(GameTime gameTime)
        {
            if (Terminated) return;
            portrait.AnimatedSprite.SpriteColor = Color.Lerp(startColor, endColor, transitionInterval);
        }

        public override void FinishTransition()
        {
            portrait.AnimatedSprite.SpriteColor = endColor;
        }
    }
}
