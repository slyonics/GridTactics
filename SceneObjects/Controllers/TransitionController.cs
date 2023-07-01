using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GridTactics.SceneObjects.Controllers
{
    public enum TransitionDirection
    {
        In,
        Out
    }

    public class TransitionController : Controller
    {
        private TransitionDirection direction;
        private float transitionTime;
        private float length;

        public TransitionController(TransitionDirection iDirection, int iLength, PriorityLevel iPriorityLevel = PriorityLevel.TransitionLevel)
            : base(iPriorityLevel)
        {
            direction = iDirection;
            transitionTime = 0;
            length = iLength;
        }

        public override void PreUpdate(GameTime gameTime)
        {
            transitionTime += gameTime.ElapsedGameTime.Milliseconds;
            if (transitionTime >= length)
            {
                transitionTime = length;
                UpdateTransition?.Invoke(TransitionProgress);
                FinishTransition?.Invoke(direction);
                Terminate();
            }
            else UpdateTransition?.Invoke(TransitionProgress);
        }

        public float TransitionProgress { get => (direction == TransitionDirection.In) ? transitionTime / length : 1.0f - (transitionTime / length); }

        public event Action<float> UpdateTransition;
        public event Action<TransitionDirection> FinishTransition;
    }
}
