using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTactics.SceneObjects.Particles
{
    public class DustParticle : AnimationParticle
    {
        private float spriteBottom;

        public DustParticle(Scene iScene, Vector2 iPosition, float iSpriteBottom)
            : base(iScene, iPosition, AnimationType.Smoke)
        {
            spriteBottom = iSpriteBottom;

            gravity = -200.0f;
        }

        public override void Update(GameTime gameTime)
        {
            float interval = Math.Max(0.0f, 1.0f - (positionZ / 8.0f));
            animatedSprite.SpriteColor = new Color(interval, interval, interval, interval);

            base.Update(gameTime);
        }

        public override float DepthPosition => spriteBottom;
    }
}