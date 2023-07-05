using GridTactics.Main;
using GridTactics.SceneObjects.Maps;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GridTactics.SceneObjects.Particles
{
    public enum EmoteType
    {
        Question,
        Exclamation
    }

    public class EmoteParticle : Particle
    {
        private static readonly Dictionary<string, Animation> PARTICLE_ANIMATIONS = new Dictionary<string, Animation>()
        {
            { EmoteType.Exclamation.ToString(), new Animation(0, 0, 17, 16, 1, 1000) },
            { EmoteType.Question.ToString(), new Animation(0, 0, 17, 16, 1, 1000) }
        };

        private List<Tuple<int, FrameFollowup>> frameEventList = new List<Tuple<int, FrameFollowup>>();

        private Actor actor;

        public EmoteParticle(Scene iScene, Actor iActor, EmoteType iAnimationType)
            : base(iScene, iActor.Bottom - new Vector2(0, 20), true)
        {
            parentScene = iScene;
            actor = iActor;
            animatedSprite = new AnimatedSprite(AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), "Particles_" + iAnimationType)], PARTICLE_ANIMATIONS);
            animatedSprite.PlayAnimation(iAnimationType.ToString(), AnimationFinished);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            Position = actor.Bottom - new Vector2(0, 20);

            Tuple<int, FrameFollowup> frameEvent = frameEventList.Find(x => x.Item1 == animatedSprite.Frame);
            if (frameEvent != null)
            {
                frameEvent.Item2();
                frameEventList.Remove(frameEvent);
            }
        }

        public void AnimationFinished()
        {
            Terminate();
        }

        public void AddFrameEvent(int frame, FrameFollowup frameEvent)
        {
            frameEventList.Add(new Tuple<int, FrameFollowup>(frame, frameEvent));
        }
    }
}
