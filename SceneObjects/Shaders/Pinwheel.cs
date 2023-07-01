using GridTactics.Main;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GridTactics.SceneObjects.Shaders
{
    public class Pinwheel : Shader
    {
        public Pinwheel(Color color, float amount)
            : base(AssetCache.EFFECTS[GameShader.Pinwheel].Clone())
        {
            Effect.Parameters["filterRed"].SetValue(color.R / 255.0f);
            Effect.Parameters["filterGreen"].SetValue(color.G / 255.0f);
            Effect.Parameters["filterBlue"].SetValue(color.B / 255.0f);
            Amount = amount;
        }

        public float Amount
        {
            set
            {
                Effect.Parameters["amount"].SetValue(value);
            }
        }
    }
}
