using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTactics.SceneObjects
{
    public enum PriorityLevel
    {
        GameLevel,
        CutsceneLevel,
        MenuLevel,
        TransitionLevel,
        SuspendedLevel
    }

    public delegate void TerminationFollowup();

    public class Controller
    {
        private PriorityLevel priorityLevel;

        private bool terminated = false;

        public Controller(PriorityLevel iPriorityLevel)
        {
            priorityLevel = iPriorityLevel;
        }

        public virtual void PreUpdate(GameTime gameTime)
        {

        }

        public virtual void PostUpdate(GameTime gameTime)
        {

        }

        public virtual void Terminate()
        {
            if (!terminated && OnTerminated != null) OnTerminated();

            terminated = true;
        }

        public PriorityLevel PriorityLevel { get => priorityLevel; }
        public bool Terminated { get => terminated; }
        public event TerminationFollowup OnTerminated;
    }
}
