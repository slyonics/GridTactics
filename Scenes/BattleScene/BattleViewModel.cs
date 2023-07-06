using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTactics.Scenes.BattleScene
{
    public class BattleViewModel : ViewModel
    {
        BattleScene battleScene;

        public BattleViewModel(BattleScene iScene)
            : base(iScene, PriorityLevel.GameLevel)
        {
            battleScene = iScene;
        }
    }
}
