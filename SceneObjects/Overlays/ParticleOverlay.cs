using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTactics.SceneObjects.Overlays
{
    public class ParticleOverlay : Overlay
    {
        private Particle particle;

        public ParticleOverlay(Particle iParticle)
        {
            particle = iParticle;
            particle.OnTerminated += new TerminationFollowup(() => Terminate());
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            particle.Draw(spriteBatch, null);
        }
    }
}
