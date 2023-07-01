using GridTactics.Main;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace GridTactics.Scenes.ConversationScene
{
    public class Portrait : Entity
    {
        public string Name { get; private set; }

        public Effect Shader { set; private get; }

        private List<PortraitController> portraitControllers = new List<PortraitController>();

        public Portrait(ConversationScene iScene, string iName, string iSprite, Vector2 iStartPosition, Vector2 iEndPosition, float iTransitionLength)
            : base(iScene, iStartPosition)
        {
            Name = iName;

            animatedSprite = new AnimatedSprite(AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), "Portraits_" + iSprite)], null);

            portraitControllers.Add(iScene.AddController(new PortraitPositionController(this, iEndPosition, iTransitionLength)));
        }

        public Portrait(ConversationScene iScene, string iName, string iSprite, Vector2 iPosition, float iTransitionLength)
            : base(iScene, iPosition)
        {
            Name = iName;

            animatedSprite = new AnimatedSprite(AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), "Portraits_" + iSprite)], null);
            animatedSprite.SpriteColor = new Color(0, 0, 0, 0);

            portraitControllers.Add(iScene.AddController(new PortraitColorController(this, new Color(255, 255, 255, 255), iTransitionLength)));
        }

        public override void Update(GameTime gameTime)
        {
            portraitControllers.RemoveAll(x => x.Terminated);
        }

        public override void Draw(SpriteBatch spriteBatch, Camera camera)
        {
            if (SpriteBounds.Top < 0) position = new Vector2(position.X, position.Y - SpriteBounds.Top);

            if (Shader == null)
            {
                animatedSprite?.Draw(spriteBatch, position - new Vector2(0.0f, positionZ), camera, 0.5f);
            }
        }

        public override void DrawShader(SpriteBatch spriteBatch, Camera camera, Matrix matrix)
        {
            if (SpriteBounds.Top < 0) position = new Vector2(position.X, position.Y - SpriteBounds.Top);

            if (Shader != null)
            {
                spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise, Shader, matrix);
                base.Draw(spriteBatch, camera);
                spriteBatch.End();
            }

        }

        public void FinishTransition()
        {
            foreach (PortraitController portraitController in portraitControllers)
            {
                portraitController.FinishTransition();
                portraitController.Terminate();
            }
        }

        public void Remove(Vector2 newPosition, float newTransitionLength = 1.0f)
        {
            PortraitPositionController controller = parentScene.AddController(new PortraitPositionController(this, newPosition, newTransitionLength));
            controller.OnTerminated += Terminate;
            portraitControllers.Add(controller);

            (parentScene as ConversationScene).Portraits.Remove(this);
        }

        public void Remove(float newTransitionLength = 1.0f)
        {
            PortraitColorController controller = parentScene.AddController(new PortraitColorController(this, new Color(), newTransitionLength));
            controller.OnTerminated += Terminate;
            portraitControllers.Add(controller);

            (parentScene as ConversationScene).Portraits.Remove(this);
        }

        public void SetSprite(string newSprite, float newTransitionLength = 1.0f)
        {
            portraitControllers.Add(parentScene.AddController(new PortraitSpriteController(this, AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), "Portraits_" + newSprite)], newTransitionLength)));
        }

        public void SetPosition(Vector2 newPosition, float newTransitionLength = 1.0f)
        {
            portraitControllers.Add(parentScene.AddController(new PortraitPositionController(this, newPosition, newTransitionLength)));
        }

        public void SetColor(Color newColor, float newTransitionLength = 1.0f)
        {
            portraitControllers.Add(parentScene.AddController(new PortraitColorController(this, newColor, newTransitionLength)));
        }
    }
}
