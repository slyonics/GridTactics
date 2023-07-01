using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTactics.SceneObjects.Particles
{
    public class AfterImageParticle : Particle
    {
        private int duration;
        private int timeLeft;

        public AfterImageParticle(Scene iScene, Vector2 iPosition, AnimatedSprite iAnimatedSprite, int iDuration, bool iForeground = false)
            : base(iScene, iPosition, iForeground)
        {
            timeLeft = duration = iDuration;

            animatedSprite = new AnimatedSprite(iAnimatedSprite);
        }

        public override void Update(GameTime gameTime)
        {
            timeLeft -= gameTime.ElapsedGameTime.Milliseconds;
            if (timeLeft <= 0) Terminate();
            else
            {
                float interval = (float)timeLeft / duration;
                animatedSprite.SpriteColor = new Color(interval, interval, interval, interval);
            }
        }

        public override float DepthPosition => base.DepthPosition - 1;
    }
}
