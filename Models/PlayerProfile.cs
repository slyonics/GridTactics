using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTactics.Models
{
    [Serializable]
    public class PlayerProfile
    {
        public PlayerProfile()
        {

        }


        public ModelProperty<int> Money { get; set; } = new ModelProperty<int>(5000);

        public ModelProperty<string> PlayerName { get; set; } = new ModelProperty<string>("Amari");


    }
}
