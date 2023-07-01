using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTactics.SceneObjects.Controllers
{
    public class ClickController : Controller
    {
        private Widget widget;

        public ClickController(PriorityLevel priorityLevel, Widget iWidget)
            : base(priorityLevel)
        {
            widget = iWidget;
        }

        public override void PreUpdate(GameTime gameTime)
        {
            Vector2 mousePosition = new Vector2(Input.MousePosition.X, Input.MousePosition.Y);
            Widget mousedWidget = widget.GetWidgetAt(mousePosition);

            if (widget == mousedWidget && Input.LeftMouseClicked)
            {
                Clicked?.Invoke();
            }
        }

        public event Action Clicked;
    }
}
