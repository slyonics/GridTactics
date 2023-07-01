using GridTactics.Main;
using GridTactics.SceneObjects;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTactics.SceneObjects.Controllers
{
    public interface ISkippableWait
    {
        void Notify(SkippableWaitController sender);
        bool Terminated { get; }
    }

    public class SkippableWaitController : Controller
    {
        private const int DEFAULT_WAIT = 3000;

        private ISkippableWait subject;

        private bool skippable;
        private int waitTimeLeft;

        public SkippableWaitController(PriorityLevel iPriorityLevel, ISkippableWait initialSubject, bool iSkippable = true, int iWait = DEFAULT_WAIT)
            : base(iPriorityLevel)
        {
            subject = initialSubject;

            skippable = iSkippable;
            waitTimeLeft = iWait;
        }

        public override void PreUpdate(GameTime gameTime)
        {
            if (subject.Terminated)
            {
                Terminate();
                return;
            }
            if (waitTimeLeft <= 0) return;

            InputFrame inputFrame = Input.CurrentInput;
            if (skippable && inputFrame.AnythingPressed()) subject.Notify(this);
            else
            {
                waitTimeLeft -= gameTime.ElapsedGameTime.Milliseconds;
                if (waitTimeLeft <= 0) subject.Notify(this);
            }
        }

        public void Reset(bool iSkippable = true, int iWait = DEFAULT_WAIT)
        {
            skippable = iSkippable;
            waitTimeLeft = iWait;
        }
    }
}
