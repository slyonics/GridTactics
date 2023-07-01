using GridTactics.Models;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTactics.Scenes.MapScene
{
    public class InteractionPrompt : Overlay
    {
        private const GameFont GAME_FONT = GameFont.Label;

        private MapScene mapScene;
        private IInteractive target;

        private NinePatch textbox;

        private Color color = new Color(252, 224, 168);

        public InteractionPrompt(MapScene iMapScene)
        {
            mapScene = iMapScene;
            textbox = new NinePatch("TechLabel", 0.05f);


        }

        public override void Update(GameTime gameTime)
        {

        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (CrossPlatformGame.CurrentScene != mapScene) return;

            if (target != null && mapScene.PriorityLevel == PriorityLevel.GameLevel)
            {
                string[] textLines = target.Label.Split('\n');
                string longestLine = textLines.MaxBy(x => Text.GetStringLength(GAME_FONT, x));
                int width = Text.GetStringLength(GAME_FONT, longestLine);
                int height = Text.GetStringHeight(GAME_FONT);
                textbox.Bounds = new Rectangle(0, 0, width + 8, height * textLines.Count() + 2);
                Vector2 cameraOffset = new Vector2(mapScene.Camera.CenteringOffsetX, mapScene.Camera.CenteringOffsetY);

                textbox.Draw(spriteBatch, target.LabelPosition - mapScene.Camera.Position - new Vector2(textbox.Bounds.Width / 2, 0) - cameraOffset);

                int row = 0;
                foreach (string text in textLines)
                {
                    Text.DrawCenteredText(spriteBatch, target.LabelPosition + new Vector2(0, 5) - mapScene.Camera.Position - cameraOffset, GAME_FONT, text, color, 0.03f, row);
                    row++;
                }
            }
        }

        public void Target(IInteractive newTarget)
        {
            target = newTarget;
        }
    }
}
