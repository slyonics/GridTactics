using Microsoft.Xna.Framework.Graphics;

namespace GridTactics.SceneObjects
{
    public abstract class Overlay
    {
        protected bool terminated = false;

        public Overlay()
        {

        }

        public virtual void Update(GameTime gameTime)
        {

        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {

        }

        public virtual void Terminate()
        {
            terminated = true;
        }

        public virtual bool Terminated { get => terminated; }
    }
}
