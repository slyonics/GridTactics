using GridTactics.SceneObjects.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTactics.SceneObjects
{
    public delegate void LandingFollowup();

    public abstract class Entity
    {
        private const float DEFAULT_GRAVITY = 800.0f;

        protected Vector2 position;
        protected Vector2 velocity;
        protected AnimatedSprite animatedSprite;

        protected float positionZ;
        protected float velocityZ;
        protected float gravity = DEFAULT_GRAVITY;
        protected LandingFollowup landingFollowup;

        protected Scene parentScene;
        protected PriorityLevel priorityLevel;

        protected bool terminated = false;

        public Entity(Scene iScene, Vector2 iPosition, Texture2D iSprite, Dictionary<string, Animation> iAnimationList)
        {
            parentScene = iScene;
            position = iPosition;
            animatedSprite = new AnimatedSprite(iSprite, iAnimationList);
        }

        public Entity(Scene iScene, Vector2 iPosition)
        {
            parentScene = iScene;
            position = iPosition;
        }

        public virtual void Update(GameTime gameTime)
        {
            UpdatePosition(gameTime);
            UpdateElevation(gameTime);

            animatedSprite?.Update(gameTime);
        }

        public virtual void UpdatePosition(GameTime gameTime)
        {
            position += velocity * gameTime.ElapsedGameTime.Milliseconds / 1000.0f;
        }

        public virtual void UpdateElevation(GameTime gameTime)
        {
            velocityZ -= gravity * gameTime.ElapsedGameTime.Milliseconds / 1000.0f;
            positionZ += velocityZ * gameTime.ElapsedGameTime.Milliseconds / 1000.0f;
            if (positionZ <= 0.0f)
            {
                positionZ = 0.0f;

                if (landingFollowup != null && velocityZ <= 0.0f)
                {
                    landingFollowup();
                    landingFollowup = null;
                }

                velocityZ = 0.0f;
            }
        }

        public virtual void PlayAnimation(string animationName, AnimationFollowup animationFollowup = null)
        {
            if (animationFollowup == null) animatedSprite.PlayAnimation(animationName);
            else animatedSprite.PlayAnimation(animationName, animationFollowup);
        }

        public virtual void Move(GameTime gameTime)
        {
            position += velocity * gameTime.ElapsedGameTime.Milliseconds / 1000.0f;
        }

        public virtual void Draw(SpriteBatch spriteBatch, Camera camera)
        {
            float depth = (camera == null) ? 0 : camera.GetDepth(DepthPosition);
            animatedSprite?.Draw(spriteBatch, position - new Vector2(0.0f, positionZ), camera, depth);
        }

        public virtual void DrawShader(SpriteBatch spriteBatch, Camera camera, Matrix matrix)
        {

        }

        public void Terminate()
        {
            terminated = true;
            OnTerminated?.Invoke();
        }

        public virtual float DepthPosition { get => position.Y; }
        public Rectangle SpriteBounds { get => animatedSprite.SpriteBounds(position); }
        public AnimatedSprite AnimatedSprite { get => animatedSprite; set => animatedSprite = value; }
        public virtual Vector2 Position { get => position; set => position = value; }
        public float PositionZ { get => positionZ; }
        public Vector2 Velocity { get => velocity; set => velocity = value; }
        public float VelocityZ { get => velocityZ; set => velocityZ = value; }
        public LandingFollowup LandingFollowup { set => landingFollowup = value; }
        public event TerminationFollowup OnTerminated;
        public PriorityLevel PriorityLevel { get => priorityLevel; set => priorityLevel = value; }
        public bool Terminated { get => terminated; }
    }
}
