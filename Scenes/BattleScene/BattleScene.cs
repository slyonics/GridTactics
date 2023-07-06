using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTactics.Scenes.BattleScene
{
    public class BattleScene : Scene
    {
        BattleViewModel battleViewModel;

        public BattleScene()
        {
            battleViewModel = AddView(new BattleViewModel(this));
        }
    }
}
