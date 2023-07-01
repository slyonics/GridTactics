using GridTactics.Main;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTactics.SceneObjects.Controllers
{
    public class WidgetController : Controller
    {
        private Widget subject;

        private Vector2? leftClickStart;
        private Vector2? rightClickStart;

        private Widget leftClickStartWidget;
        private Widget rightClickStartWidget;
        private Widget lastMousedWidget;
        private Widget lastLeftClickedWidget;

        public WidgetController(PriorityLevel iPriorityLevel, Widget iSubject)
            : base(iPriorityLevel)
        {
            subject = iSubject;
        }

        public override void PreUpdate(GameTime gameTime)
        {
            if (subject.Transitioning || subject.Closed || !subject.Visible) return;

            Vector2 mousePosition = new Vector2(Input.MousePosition.X, Input.MousePosition.Y);
            Widget mousedWidget = subject.GetWidgetAt(mousePosition);

            if (Input.LeftMouseState == ButtonState.Pressed && Input.RightMouseState == ButtonState.Released && !leftClickStart.HasValue)
            {
                lastLeftClickedWidget?.LoseFocus();

                leftClickStart = mousePosition;
                leftClickStartWidget = mousedWidget;
                leftClickStartWidget?.StartLeftClick(mousePosition);
            }

            if (Input.RightMouseState == ButtonState.Pressed && Input.LeftMouseState == ButtonState.Released && !rightClickStart.HasValue)
            {
                rightClickStart = mousePosition;
                rightClickStartWidget = mousedWidget;
                rightClickStartWidget?.StartRightClick(mousePosition);
            }

            if (Input.LeftMouseClicked && leftClickStart.HasValue)
            {
                mousedWidget?.EndLeftClick(leftClickStart.Value, mousePosition, leftClickStartWidget);
                if (mousedWidget != leftClickStartWidget) leftClickStartWidget?.EndLeftClick(leftClickStart.Value, mousePosition, mousedWidget);
                leftClickStart = null;

                lastLeftClickedWidget = mousedWidget;
            }

            if (Input.RightMouseClicked && rightClickStart.HasValue)
            {
                mousedWidget?.EndRightClick(rightClickStart.Value, mousePosition, rightClickStartWidget);
                if (mousedWidget != rightClickStartWidget) rightClickStartWidget?.EndRightClick(rightClickStart.Value, mousePosition, mousedWidget);
                rightClickStart = null;
            }

            if (mousedWidget != lastMousedWidget)
            {
                mousedWidget?.StartMouseOver();
                lastMousedWidget?.EndMouseOver();
                lastMousedWidget = mousedWidget;
            }
        }
    }
}
