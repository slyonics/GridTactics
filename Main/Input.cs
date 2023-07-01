using GridTactics.SceneObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTactics.Main
{
    public static class Input
    {
        public const float FULL_THUMBSTICK_THRESHOLD = 0.9f;
        public const float THUMBSTICK_DEADZONE_THRESHOLD = 0.1f;

        private const float CONTROL_ICON_DEPTH = 0.11f;
        private static Texture2D keyboardSprite;
        private static Texture2D gamepadSprite;

        private static InputFrame inputFrame = new InputFrame();

        private static MouseState oldMouseState;
        private static MouseState newMouseState;

        public static bool MOUSE_MODE = true;

        public static void ApplySettings()
        {
            inputFrame.ApplySettings();

            oldMouseState = newMouseState = Mouse.GetState();
        }

        public static void Update(GameTime gameTime)
        {
            inputFrame.Update(gameTime);

            oldMouseState = newMouseState;
            newMouseState = Mouse.GetState();

            if (newMouseState.LeftButton == ButtonState.Pressed) MOUSE_MODE = true;

            MousePosition = new Vector2(newMouseState.Position.X, newMouseState.Position.Y) / CrossPlatformGame.Scale;

            DeltaMouseGame = new Vector2((newMouseState.Position.X - oldMouseState.Position.X) / 2.0f, (newMouseState.Position.Y - oldMouseState.Position.Y) / 2.0f) / CrossPlatformGame.Scale;
        }

        public static bool LeftMouseClicked { get => newMouseState.LeftButton == ButtonState.Released && oldMouseState.LeftButton == ButtonState.Pressed; }
        public static bool RightMouseClicked { get => newMouseState.RightButton == ButtonState.Released && oldMouseState.RightButton == ButtonState.Pressed; }
        public static ButtonState LeftMouseState { get => newMouseState.LeftButton; }
        public static ButtonState RightMouseState { get => newMouseState.RightButton; }
        public static Vector2 MousePosition { get; private set; }
        public static Vector2 DeltaMouseGame { get; private set; }
        public static int MouseWheel { get => newMouseState.ScrollWheelValue - oldMouseState.ScrollWheelValue; }

        public static InputFrame CurrentInput { get => inputFrame; }
    }
}

