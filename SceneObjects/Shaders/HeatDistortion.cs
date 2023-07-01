using GridTactics.Main;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTactics.SceneObjects.Shaders
{
    public class HeatDistortion : Shader
    {
        private int distortionTime;

        public HeatDistortion()
            : base(AssetCache.EFFECTS[GameShader.HeatDistortion].Clone())
        {
            shaderEffect.Parameters["amplitude"].SetValue(2.0f);
            shaderEffect.Parameters["red"].SetValue(1.0f);
            shaderEffect.Parameters["green"].SetValue(0.8f);
            shaderEffect.Parameters["blue"].SetValue(0.6f);
            shaderEffect.Parameters["transparency"].SetValue(0.3f);
            shaderEffect.Parameters["time"].SetValue(0.0f);
        }

        public override void Update(GameTime gameTime, Camera camera)
        {
            distortionTime += gameTime.ElapsedGameTime.Milliseconds;
            shaderEffect.Parameters["time"].SetValue(distortionTime * 0.0005f);
        }
    }
}
