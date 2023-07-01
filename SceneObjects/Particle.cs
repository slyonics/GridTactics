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
    public abstract class Particle : Entity
    {
        public bool Foreground { get; private set; } = false;

        public Particle(Scene iScene, Vector2 iPosition, Texture2D iSprite, Dictionary<string, Animation> iAnimationList, bool iForeground = false)
            : base(iScene, iPosition, iSprite, iAnimationList)
        {
            Foreground = iForeground;

            if (Foreground) position.Y += SpriteBounds.Height / 2;
        }

        public Particle(Scene iScene, Vector2 iPosition, bool iForeground = false)
            : base(iScene, iPosition)
        {
            Foreground = iForeground;
        }

        public override float DepthPosition
        {
            get
            {
                if (Foreground) return parentScene.Camera.MaxVisibleY;
                else return base.DepthPosition;
            }
        }
    }
}
