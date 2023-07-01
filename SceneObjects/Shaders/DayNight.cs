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
    public class Light
    {
        public const float DEFAULT_INTENSITY = 64.0f;
        public const float DEFAULT_FLICKER_SIZE = 1.0f;
        public const float DEFAULT_FLICKER_MEAN = 100.0f;
        public const float DEFAULT_FLICKER_STD = 100.0f;
        public const float DEFAULT_FLICKER_SPEED = 20000.0f;

        private Vector2 position;
        private float positionZ;
        private Color color = Color.White;
        private float intensity = 64.0f;

        private float lightFlicker;
        private float flickerSize = 1.0f;
        private float flickerMean = 100.0f;
        private float flickerStd = 100.0f;
        private float flickerSpeed = 20000.0f;

        public Light(Vector2 iPosition, float iPositionZ)
        {
            position = iPosition;
            positionZ = iPositionZ;
        }

        public void Flicker(GameTime gameTime, float mean, float std, float speed, float size)
        {
            lightFlicker += (float)Math.Abs(Rng.GaussianDouble(mean, std)) * gameTime.ElapsedGameTime.Milliseconds / speed;
            flickerSize = size;
        }

        public void Flicker(GameTime gameTime)
        {
            lightFlicker += (float)Math.Abs(Rng.GaussianDouble(flickerMean, flickerStd)) * gameTime.ElapsedGameTime.Milliseconds / flickerSpeed;
        }

        public void SetFlicker(float mean, float std, float speed, float size)
        {
            flickerMean = mean;
            flickerStd = std;
            flickerSpeed = speed;
            flickerSize = size;
        }

        public Vector2 Position { set => position = value; get => position; }
        public float PositionZ { set => positionZ = value; get => positionZ; }
        public float Intensity { set => intensity = value; get => intensity + (float)Math.Sin(lightFlicker) * flickerSize; }
        public Color Color { set => color = value; get => color; }
    }

    public class DayNight : Shader
    {
        private List<Light> lightList = new List<Light>();

        private Matrix matrixX = new Matrix();
        private Matrix matrixY = new Matrix();
        private Matrix matrixI = new Matrix();
        private Matrix matrixR = new Matrix();
        private Matrix matrixG = new Matrix();
        private Matrix matrixB = new Matrix();

        public DayNight(Color ambient, float bloom)
            : base(AssetCache.EFFECTS[GameShader.DayNight].Clone())
        {
            Bloom = bloom;
            Ambient = ambient.ToVector4();
        }

        public override void Update(GameTime gameTime, Camera camera)
        {
            List<Light> lights = lightList.OrderBy(x => Vector2.Distance(x.Position, new Vector2(camera.View.Center.X, camera.View.Center.Y))).ToList();

            int i = 0;
            for (int row = 0; row < 3; row++)
            {
                for (int column = 0; column < 4; column++)
                {
                    if (i < lights.Count)
                    {
                        matrixX[row, column] = lights[i].Position.X - camera.View.Left;
                        matrixY[row, column] = lights[i].Position.Y - camera.View.Top;
                        matrixI[row, column] = lights[i].Intensity;
                        matrixR[row, column] = lights[i].Color.R;
                        matrixG[row, column] = lights[i].Color.G;
                        matrixB[row, column] = lights[i].Color.B;
                    }

                    i++;
                }
            }

            Effect.Parameters["lightX"].SetValue(matrixX);
            Effect.Parameters["lightY"].SetValue(matrixY);
            Effect.Parameters["lightI"].SetValue(matrixI);
            Effect.Parameters["lightR"].SetValue(matrixR);
            Effect.Parameters["lightG"].SetValue(matrixG);
            Effect.Parameters["lightB"].SetValue(matrixB);
        }

        public Vector4 Ambient
        {
            set
            {
                Effect.Parameters["ambient"].SetValue(value);
            }
        }

        public float Bloom
        {
            set
            {
                Effect.Parameters["bloom"].SetValue(value);
            }
        }

        public List<Light> Lights { get => lightList; }
    }
}
